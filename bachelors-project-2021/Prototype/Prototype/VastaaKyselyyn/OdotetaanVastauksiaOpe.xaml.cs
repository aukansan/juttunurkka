/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Emmi Poutanen, Riina Kaipia

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OdotetaanVastauksiaOpe : ContentPage, INotifyPropertyChanged
    {

        CancellationTokenSource cts;
        public string RoomCode { get; set; }

        private int countdownSeconds = 60;

        private int participantsCount;

        public int ParticipantsCount
        {
            get => participantsCount;
            set
            {
                if (participantsCount != value)
                {
                    participantsCount = value;
                    OnPropertyChanged(nameof(ParticipantsCount));
                    OnPropertyChanged(nameof(RespondentsDisplay));
                }
            }
        }

        private int respondentsCount;
        public int RespondentsCount
        {
            get => respondentsCount;
            set
            {
                if (respondentsCount != value)
                {
                    respondentsCount = value;
                    OnPropertyChanged(nameof(RespondentsCount));
                    OnPropertyChanged(nameof(RespondentsDisplay));
                }
            }
        }

        public string RespondentsDisplay => $"{RespondentsCount} / {ParticipantsCount}";

        public OdotetaanVastauksiaOpe()
        {
            InitializeComponent();
            // Set as true for testing
            NavigationPage.SetHasNavigationBar(this, false);
            Survey s = SurveyManager.GetInstance().GetSurvey();
            RoomCode = s.RoomCode;
            BindingContext = this;

            Host();
            StartUpdatingCounts();
        }

        private async void Host()
        {
            Main.GetInstance().host?.DestroyHost();
            Main.GetInstance().host = new SurveyHost(false);

            if (!await Main.GetInstance().HostSurvey())
            {
                //host survey ended in a fatal unexpected error, aborting survey.
                //pop to root and display error
                await Navigation.PopToRootAsync();
                await DisplayAlert("Kysely suljettiin automaattisesti3", "Tapahtui odottamaton virhe.", "OK");
            }
        }

        async protected override void OnAppearing()
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;
            base.OnAppearing();

            try
            {
                await UpdateProgressBar(0, 60000, token);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Task cancelled", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("ex {0}", e.Message);
            }
            finally
            {
                cts.Dispose();
            }
            
        }

        async Task UpdateProgressBar(double Progress, uint time, CancellationToken token)
        {
            var countdownTask = UpdateCountdownLabel(time, token);

            await progressBar.ProgressTo(Progress, time, Easing.Linear);
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            await countdownTask;
            //siirtyy eteenpäin automaattisesti 60 sekunnin jälkeen
            if (progressBar.Progress == 0)
                {
                    await Main.GetInstance().host.CloseSurvey();
                    await Navigation.PushAsync(new LisätiedotHost());
                }   
        }

        private async Task UpdateCountdownLabel(uint totalTimeMs, CancellationToken token)
        {
            int totalSeconds = (int)(totalTimeMs / 1000);
            for (int i = totalSeconds; i >= 0; i--)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    countdownLabel.Text = i.ToString() + " s";
                });
                await Task.Delay(1000, token);
            }
        }


        private async void LopetaClicked(object sender, EventArgs e)
        {
        cts.Cancel(); //cancel task if button clicked

            await Main.GetInstance().host.CloseSurvey();
            await Navigation.PushAsync(new LisätiedotHost());
        }
        private void StartUpdatingCounts()
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                var host = Main.GetInstance().host;
                ParticipantsCount = host.clientCount;

                // Laske vastanneet opiskelijat, jotka ovat tehneet vähintään yhden valinnan
                RespondentsCount = host?.data?.GetEmojiResults()?.Where(kv => kv.Value > 0).Count() ?? 0;

                return true;
            });
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
    }
