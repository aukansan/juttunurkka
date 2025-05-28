
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

namespace Prototype
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ActivityAnswered : ContentPage
	{
        public Activity ActivityToShow { get; set; }

        private readonly Activity? _votedAcitivy;
		private readonly int _remainingTime;
        private bool _voteResultReceived = false;

        public ActivityAnswered(Activity? votedActivity, int remainingTime)
		{
			_votedAcitivy = votedActivity;
			_remainingTime = remainingTime;
			NavigationPage.SetHasBackButton(this, false);
            ActivityToShow = votedActivity ?? new Activity();
            InitializeComponent();
            BindingContext = this;

            StartCountdownAndListen();
        }

        private async void StartCountdownAndListen()
        {
            int totalSeconds = _remainingTime;
            int elapsed = 0;

            // Cancellation token to cancel both tasks if needed
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Start a task to listen for the results while countdown is running
            var listenTask = ListenForResultsAsync(cancellationTokenSource.Token);

            while (elapsed < totalSeconds)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                // Update progress bar
                double progress = 1.0 - (double)elapsed / totalSeconds;
                progressBar.Progress = progress;

                await Task.Delay(1000);
                elapsed++;
            }
            progressBar.Progress = 0;

            await Task.Delay(5000);
            // Timeout, Cancel listen task and handle timeout
            cancellationTokenSource.Cancel();

            if (!_voteResultReceived)
            {
                // If no result received yet, show error and navigate to the main page
                await DisplayAlert("VIRHE", "Tulosten haku epäonnistui", "OK");
                await Navigation.PushAsync(new MainPage());
            }
        }

        // Listen for vote results asynchronously
        private async Task ListenForResultsAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Loop to try receiving results every 1 second
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool success = await Main.GetInstance().client.ReceiveVoteResult();
                    if (success)
                    {
                        _voteResultReceived = true;
                        // If successful, navigate to the results page and cancel the countdown
                        await Navigation.PushAsync(new AktiviteettiäänestysTulokset());
                        break;
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while receiving results: {ex.Message}");
            }
        }
    }
}