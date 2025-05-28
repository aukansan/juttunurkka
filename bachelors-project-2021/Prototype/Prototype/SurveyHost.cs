
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Emmi Poutanen

This file is part of "Juttunurkka".

Juttunurkka is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

Juttunurkka is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Juttunurkka.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.Maui;
using System.IO;
using System.Diagnostics;

namespace Prototype
{
	/// <summary>
	/// Provides functionality to communicate with clients in order to host a survey
	/// </summary>

	// DISCLAIMER!
	// Throughout this project I was *learning* networking and async programming nearly from the ground up
	// I cannot guarantee the safety of this code and I apologize for any bad approaches
	// No sensitive data is exchanged luckily!
	// Many of the functions here can be converted to generic ReceiveData and SendData functions to improve efficiency and error tolerance
    public class SurveyHost
    {
        /// <summary>
        ///     If this is true, host uses TCP client instead of the UDP. This enables developing with two Android emulators.
        /// </summary>
        private readonly bool _testMode;

        /// <value>
        /// Instance of Survey containing the details of the hosted survey
        /// </value>
        private Survey survey;

		/// <value>
		/// Instance of SurveyData containing the data of the survey results
		/// </value>
        public SurveyData data { get; private set; }

		/// <value>
		/// Integer value depicting how many clients are currently connected
		/// </value>
        public int clientCount { get; private set; }

		/// <value>
		/// List of TcpClient instances for each connected client
		/// </value>
        private List<TcpClient> clients;

		/// <value>
		/// List of IP adresses which have joined the survey. Entries remain even if the client in question disconnects.
		/// </value>
        private List<IPAddress> clientHistory;

        private int _activityVoteAnswerCount;

		/// <value>
		/// List of running Tasks which can be cancelled
		/// </value>
        private List<Task> cancellableTasks;

		/// <value>
		/// Instance of CancellationTokenSource which can be used to call cancellation of tasks in the cancellableTasks list
		/// </value>
        private CancellationTokenSource tokenSource;

		/// <value>
		/// Instance of CancellationToken fed to the cancellable tasks
		/// </value>
        private CancellationToken token;

		/// <value>
		/// Instance of ActivityVote to serve activity voting
		/// </value>
        public ActivityVote voteCalc { get; private set; } = null;

		/// <value>
		/// Boolean indicating whether the voting has concluded
		/// </value>
        public bool isVoteConcluded { get; private set; } = false;

        /// <summary>
        /// Default constructor
        /// <remarks>
        /// The instance created does not start running any tasks automatically
        /// </remarks>
        /// </summary>
        /// <param name="testMode">Run host in test mode</param>
        public SurveyHost(bool testMode) {
            data = new SurveyData();
            survey = SurveyManager.GetInstance().GetSurvey();
            clientCount = 0;
            clients = new List<TcpClient>();
            clientHistory = new List<IPAddress>();
            cancellableTasks = new List<Task>();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            _testMode = testMode;
        }

        /// <summary>
        /// The main sequence of running a survey
        /// </summary>
        /// <remarks>
        /// CloseSurvey method must be called to advance this task from the initial phase of accepting new clients
        /// </remarks>
        /// <returns>
        /// Task object resulting in a boolean indicating whether a fatal error occured in the process terminating hosting as a whole
        /// </returns>
        public async Task<bool> RunSurvey(bool isEmulatorTesting = false)
        {
            if (_testMode)
            {
                // In emulator testing mode, only start the TCP server
                Console.WriteLine("Running in emulator testing mode (TCP only)");
                Task<bool> tcpTask = AcceptClient();
                cancellableTasks.Add(tcpTask);
                await Task.WhenAll(cancellableTasks.ToArray());
                if (tcpTask.Result == false)
                {
                    // Fatal unexpected error
                    return false;
                }
            }
            else
            {
                //Phase 1 - making client connections and collecting emojis
                Task<bool> task1 = ReplyBroadcast();
                Task<bool> task2 = AcceptClient();
                cancellableTasks.Add(task1);
                cancellableTasks.Add(task2);

                await Task.WhenAll(cancellableTasks.ToArray());
                if (task1.Result == false || task2.Result == false)
                {
                    //Fatal unexpected error do something...
                    return false;
                }
            }
            //Phase 2 - time after the survey has concluded in which users view results
            Console.WriteLine($"Results: {data}");
            Console.WriteLine("Sending results to clients");
            SendToAllClients(data);
            return true;
        }

        /// <summary>
        /// The main sequence of running activity vote after running the survey
        /// </summary>
        /// <returns>
        /// Task object of the running process
        /// </returns>
        public async Task RunActivityVote()
        {
            Console.WriteLine("Running activity vote");
			//send first vote to all candidates
            SendToAllClients(voteCalc.GetVote1Candidates());

            var voteDurationMs = 1000 * voteCalc.vote1Timer;
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var voteTasks = clients.Select(client => Task.Run(async () =>
            {
                var stream = client.GetStream();
                var buffer = new byte[4];

                while (!token.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        try
                        {
                            var activity = await AcceptVote1(client);
                            if (activity != null)
                            {
                                Console.WriteLine("Vote received");
                                _activityVoteAnswerCount++;
                                lock (data)
                                {
                                    data.AddVote1Results(activity);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing vote from client: {ex.Message}");
                            break;
                        }
                    }
                    else
                    {
                        await Task.Delay(100); // non-blocking wait
                    }
                }
            })).ToList();

            await Task.Delay(voteDurationMs);
            cts.Cancel();

            await Task.WhenAll(voteTasks);
            Console.WriteLine("Stopped accepting activity votes");
        }

        public async Task SendActivityVoteResults()
        {
            Console.WriteLine("SendingActivity vote results");
            //prepare result and send it to all clients
            var result = data.GetVote1Results();
            // Send the number of client that participated to the survey
            result.Add(new Activity { Title = "Clients", ImageSource = "" }, clientCount);
            var serializableResult = result.Select(kvp => new
            {
                Activity = new
                {
                    kvp.Key.Title,
                    kvp.Key.ImageSource
                },
                Votes = kvp.Value
            }).ToList();

            // Send the JSON string to all clients
            SendToAllClients(serializableResult);
            Console.WriteLine("SendingActivity vote results finish");

            isVoteConcluded = true;
        }

		/// <summary>
		/// Blocks further clients from entering and answering the survey
		/// <remarks>
		/// Call after the RunSurvey Task has been started to move on in the process
		/// </remarks>
		/// </summary>
		/// <returns>
		/// Task object representing the work
		/// </returns>
        public async Task CloseSurvey() {
            tokenSource.Cancel();
            await Task.WhenAll(cancellableTasks.ToArray());
            return;
        }

		/// <summary>
		/// Starts activity vote with the connected clients
		/// </summary>
		/// <remarks>
		/// RunSurvey task must have been concluded before starting this task
		/// </remarks>
        public void StartActivityVote() {
            //prepare first vote
            Console.WriteLine("Start activity vote");
            isVoteConcluded = false;
            voteCalc = new ActivityVote();
            var activites = voteCalc.calcVote1Candidates(survey.emojis, data.GetEmojiResults());
            // Init the vote results to 0 for all candidates
            foreach (var activity in activites)
            {
                Main.GetInstance().host.data.AddVote1Results(activity);
            }

            Task.Run(() =>
            {
                Task voteTask = RunActivityVote();
            });
        }

		/// <summary>
		/// Looping task replying to server discovery broadcasts from the clients so that they can learn the host's address
		/// </summary>
		/// <returns>
		/// Task resulting in a boolean indicating whether the task ended in a fatal error
		/// </returns>
        private async Task<bool> ReplyBroadcast() {
            UdpClient? listener = null;
            Socket? socket = null;
            try
            {
                listener = new UdpClient(Const.Network.ServerUDPClientPort);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                //loop to serve all broadcasts
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    Task<UdpReceiveResult> broadcast = listener.ReceiveAsync();

                    //allow cancellation of this task between each udp message
                    do
                    {
                        if (token.IsCancellationRequested)
                        {
                            listener.Close();
                            socket.Close();
                            token.ThrowIfCancellationRequested();
                        }
                        await Task.WhenAny(new Task[] { Task.Delay(1000), broadcast });
                    } while (broadcast.Status != TaskStatus.RanToCompletion);

                    //message received
                    string message = Encoding.Unicode.GetString(broadcast.Result.Buffer, 0, broadcast.Result.Buffer.Length);
                    Console.WriteLine($"Received broadcast from {broadcast.Result.RemoteEndPoint} : {message}");

                    if (message == survey.RoomCode)
                    {
                        //has this client answered the survey already?
                        if (!clientHistory.Contains(broadcast.Result.RemoteEndPoint.Address))
                        {
                            //prepare message and destination
                            byte[] sendbuf = Encoding.Unicode.GetBytes("Connect please");
                            IPEndPoint ep = new IPEndPoint(broadcast.Result.RemoteEndPoint.Address, Const.Network.ClientUDPClientPort);
                            //reply
                            Console.WriteLine($"Replying... EP: {ep}");
                            socket.SendTo(sendbuf, ep);
                        }
                        else
                        {
                            Console.WriteLine("Old client tried to connect again");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Received invalid Room Code");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("ReplyBroadcast task was cancelled gracefully");
                return true;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                // Always clean up resources, even if an exception occurs
                if (listener != null)
                {
                    listener.Close();
                    listener.Dispose();
                }

                if (socket != null)
                {
                    socket.Close();
                    socket.Dispose();
                }
            }

            //handle unexpected errors
            Console.WriteLine("Fatal error occured, aborting survey.");
            tokenSource.Cancel();
            return false;
        }

		/// <summary>
		/// Looping task allowing new tcp connections to be built to the host
		/// </summary>
		/// <remarks>
		/// Clients are not permanently added to the list of connected clients unless an answer to the survey is received before CloseSurvey call
		/// </remarks>
		/// <returns>
		/// Task object resulting in a boolean indicating whether the task ended in a fatal error
		/// </returns>
        private async Task<bool> AcceptClient() {
            TcpListener? listener = null;
            try
            {
                int tcpPort = _testMode ? 8000 : Const.Network.ServerTCPListenerPort;
                listener = new TcpListener(IPAddress.Any, tcpPort);
                listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                listener.Start();
                Console.WriteLine($"TCP listener started on port {Const.Network.ServerTCPListenerPort}");

                while (true)
                {
                    Console.WriteLine("Waiting to accept tcp client");
                    Task<TcpClient> newClient = listener.AcceptTcpClientAsync();

                    // Allow cancellation of task between adding each client
                    do
                    {
                        if (token.IsCancellationRequested)
                        {
                            listener.Stop();
                            token.ThrowIfCancellationRequested();
                        }
                        await Task.WhenAny(new Task[] { Task.Delay(1000), newClient });
                    } while (newClient.Status != TaskStatus.RanToCompletion);

                    // For emulator testing, assume any connection is valid and allowed
                    // Otherwise stick with the normal validation logic in ServeNewClient
                    if (_testMode)
                    {
                        Console.WriteLine("Emulator testing mode: Accepting client without UDP validation");
                        TcpClient client = newClient.Result;
                        IPAddress clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                        
                        // Read the room code that the client sent directly
                        NetworkStream stream = client.GetStream();
                        byte[] buffer = new byte[128];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string roomCode = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                        
                        Console.WriteLine($"Received direct connection with room code: {roomCode}");
                        
                        // Validate room code
                        if (roomCode == survey.RoomCode)
                        {
                            // Add to client history to prevent duplicates
                            if (!clientHistory.Contains(clientIp))
                            {
                                clientHistory.Add(clientIp);
                                Task childTask = Task.Run(() => ServeNewClient(client, token));
                            }
                            else
                            {
                            Console.WriteLine("Client already connected previously");
                            // You might want to handle reconnection differently
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid room code received");
                            client.Close();
                        }
                    }
                    else
                    {
                        // Normal operation - use existing ServeNewClient
                        Task childtask = Task.Run(() => ServeNewClient(newClient.Result, token));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("AcceptClient task was cancelled gracefully");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in AcceptClient: {e.Message}");
                Console.WriteLine(e);
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }

            //handle unexpected errors
            Console.WriteLine("Fatal error occured, aborting survey.");
            tokenSource.Cancel();
            return false;
        }

        /// <summary>
        /// Services a freshly joined client by sending initial data and waiting for their emoji answer
        /// </summary>
        /// <param name="client">
        /// The client to serve
        /// </param>
        /// <param name="token">
        /// The cancellation token
        /// </param>
        private async void ServeNewClient(TcpClient client, CancellationToken token)
        {
            try
            {
                NetworkStream ns = client.GetStream();

                string introMessage = survey.introMessage;
                byte[] introBytes = Encoding.Unicode.GetBytes(introMessage);
                byte[] introSizeBytes = BitConverter.GetBytes(introBytes.Length);

                await ns.WriteAsync(introSizeBytes, 0, introSizeBytes.Length);
                await ns.WriteAsync(introBytes, 0, introBytes.Length);

                string emojiData = string.Join(",", survey.emojis.Select(e => e.Name)) + ",";
                byte[] emojiBytes = Encoding.Unicode.GetBytes(emojiData);
                byte[] emojiSizeBytes = BitConverter.GetBytes(emojiBytes.Length);

                await ns.WriteAsync(emojiSizeBytes, 0, emojiSizeBytes.Length);
                await ns.WriteAsync(emojiBytes, 0, emojiBytes.Length);

                byte[] buffer = new byte[4];
                Task<int> emojiReply = ns.ReadAsync(buffer, 0, buffer.Length);

                //allow cancellation of task here.
                do
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        token.ThrowIfCancellationRequested();
                    }
                    await Task.WhenAny(new Task[] { Task.Delay(1000), emojiReply });
                } while (emojiReply.Status != TaskStatus.RanToCompletion);

                if (emojiReply.Result == 0)
                {
					//we read nothing out of disconnected network, nice
					//we don't want this client
                    return;
                }

				//process reply
                string reply = Encoding.Unicode.GetString(buffer, 0, emojiReply.Result);
                Console.WriteLine($"Bytes read: {emojiReply.Result}");
                Console.WriteLine($"Client sent: {reply}");

				//add to surveydata
                data.AddEmojiResults(int.Parse(reply));

				//add this client to list of clients
                clientCount++;
                clients.Add(client);
                clientHistory.Add(((IPEndPoint)client.Client.RemoteEndPoint).Address);
				//flush the netstream for others to use too
                ns.Flush();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelling task, Client was dropped for being slow poke");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something went wrong in first communication with client: {client.Client.RemoteEndPoint}");
                Console.WriteLine(e);
                client.Close();
            }
        }

		/// <summary>
		/// Tries to read the first phase vote answer from a client
		/// </summary>
		/// <remarks>
		/// Not all clients send a full answer, or an answer at all
		/// </remarks>
		/// <param name="client">
		/// The client to try get the answer from
		/// </param>
		/// <returns>
		/// Task representing the work
		/// </returns>
        private async Task<Activity?> AcceptVote1(TcpClient client)
        {
            try
            {
                NetworkStream ns = client.GetStream();

                // Read the length prefix (4 bytes)
                byte[] lengthBuffer = new byte[4];
                int bytesRead = await ns.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
                if (bytesRead != 4)
                {
                    Console.WriteLine("Failed to read length prefix or connection closed");
                    return null;
                }

                // Determine the size of the JSON message
                int messageSize = BitConverter.ToInt32(lengthBuffer, 0);
                Console.WriteLine($"Expecting JSON message of size: {messageSize} bytes");

                // Read the JSON message
                byte[] messageBuffer = new byte[messageSize];
                bytesRead = await ns.ReadAsync(messageBuffer, 0, messageSize);
                if (bytesRead != messageSize)
                {
                    Console.WriteLine($"Failed to read complete JSON message. Expected {messageSize} bytes, got {bytesRead}");
                    return null;
                }

                // Deserialize the JSON into a dictionary
                string json = Encoding.Unicode.GetString(messageBuffer);
                Console.WriteLine($"Received JSON: {json}");
                var resultDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                if (resultDict != null && resultDict.ContainsKey("title") && resultDict.ContainsKey("imageSource"))
                {
                    // Map the dictionary to an Activity object
                    return new Activity
                    {
                        Title = resultDict["title"],
                        ImageSource = resultDict["imageSource"]
                    };
                }

                Console.WriteLine("Invalid data received. Missing required keys.");
                return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine("Received bad JSON");
                Console.WriteLine(e);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine("Connection lost to client");
                Console.WriteLine(e);
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("Error reading socket or network");
                Console.WriteLine(e);
            }

			//so... you have chosen death
            clients.Remove(client);
            clientCount--;
            return null;
        }

		/// <summary>
		/// Sends all clients a serialized JSON string of an object
		/// </summary>
		/// <param name="obj">
		/// The object representing the data to be sent
		/// </param>
        private void SendToAllClients(object obj) {
            byte[] messageData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(obj));
            byte[] lengthPrefix = BitConverter.GetBytes(messageData.Length);

            // Iterate each recorded client
            foreach (var client in clients)
            {
                try
                {
                    NetworkStream ns = client.GetStream();
                    if (ns.CanWrite)
                    {
                        // Write the length prefix and message
                        ns.Write(lengthPrefix, 0, lengthPrefix.Length);
                        ns.Write(messageData, 0, messageData.Length);
                        // Make sure data is sent
                        ns.Flush();
                    }
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine($"Connection lost with client: {client.Client.RemoteEndPoint}. Dropping client");
                    Console.WriteLine(e);
                    clients.Remove(client);
                    clientCount--;
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine("Error reading socket or network");
                    Console.WriteLine(e);
                    clients.Remove(client);
                    clientCount--;
                }
            }
        }

        public int GetActivityVoteAnswerCount()
        {
            return _activityVoteAnswerCount;
        }

		/// <summary>
		/// Sufficiently terminates the host processes and client connections when the survey concludes or is aborted
		/// </summary>
        public void DestroyHost() {
            Console.WriteLine("Destroying survey host");
            //cancel tasks
            tokenSource.Cancel();

            // Close all connections
            foreach (var item in clients)
            {
                item.Close();
            }

            // Reset state for new survey
            clients.Clear();
            clientCount = 0;
            data = new SurveyData();
            survey = SurveyManager.GetInstance().GetSurvey();
            // Create new cancellation mechanism
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            // Clear task list
            cancellableTasks.Clear();

            // Reset voting state
            _activityVoteAnswerCount = 0;
            voteCalc = null;
            isVoteConcluded = false;
        }
    }
}
