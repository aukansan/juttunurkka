
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System.ComponentModel;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class TulostenOdotus : ContentPage, INotifyPropertyChanged
    {
        
        public string RoomCode { get; set; }
        public int ParticipantsCount { get; set; }
        private int _answerCount;
        public int AnswerCount
        {
            get => _answerCount;
            set
            {
                if (_answerCount != value)
                {
                    _answerCount = value;
                    OnPropertyChanged(nameof(AnswerCount));
                }
            }
        }
        private int _countSeconds = 35;
        private int _timeLeft = 35;
        public int TimeLeft
        {
            get => _timeLeft;
            set
            {
                if (_timeLeft != value)
                {
                    _timeLeft = value;
                    OnPropertyChanged(nameof(TimeLeft));
                }
            }
        }

        public TulostenOdotus()
        {
            InitializeComponent();
            Survey s = SurveyManager.GetInstance().GetSurvey();
            RoomCode = s.RoomCode;
            ParticipantsCount = Main.GetInstance().host.clientCount;
            AnswerCount = 0;
            BindingContext = this;

            //poistetaan turha navigointipalkki
            NavigationPage.SetHasNavigationBar(this, false);

            Console.WriteLine("Starting activity vote");
            Main.GetInstance().host.StartActivityVote();

            
            //timer set to vote times, cooldowns, plus one extra
            _countSeconds = Main.GetInstance().host.voteCalc.vote1Timer + ( 3 * Main.GetInstance().host.voteCalc.coolDown);
            // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                _countSeconds--;
                TimeLeft--;

                AnswerCount = Main.GetInstance().host.GetActivityVoteAnswerCount();

                if (Main.GetInstance().host.isVoteConcluded)
				{
                    return false;
                }
                    

                 if (_countSeconds == 0) {
                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        return false;
                    });

                  
                 }

                return Convert.ToBoolean(_countSeconds);
            });
        }

        async protected override void OnAppearing()
        {
           
            base.OnAppearing();
            // This is now the real timer that triggers the navigation
            // TODO: Use only one timer
            await UpdateProgressBar(0, 35000);
        }



        async Task UpdateProgressBar(double Progress, uint time)
        {
            await progressBar.ProgressTo(Progress, time, Easing.Linear);
            //siirtyy eteenpäin automaattisesti 45 sekunnin jälkeen
            if (progressBar.Progress == 0 && !Main.GetInstance().host.isVoteConcluded)
            {
                await Main.GetInstance().host.CloseSurvey();
                await Main.GetInstance().host.SendActivityVoteResults();
                await Navigation.PushAsync(new AktiviteettiäänestysTulokset());
            }
        }

        async void GoToResultsClicked(object sender, EventArgs e)
        {
            await Main.GetInstance().host.CloseSurvey();
            await Main.GetInstance().host.SendActivityVoteResults();
            await Navigation.PushAsync(new AktiviteettiäänestysTulokset());
        }

        protected override bool OnBackButtonPressed()
        {
            return true;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
