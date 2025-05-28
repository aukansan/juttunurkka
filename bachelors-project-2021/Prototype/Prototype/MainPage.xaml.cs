/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Petri Pentinpuro

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
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;

namespace Prototype
{
    public partial class MainPage : ContentPage
    {
        // Launcher.OpenAsync is provided by Xamarin.Essentials.
        public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));

        public string SelectedSurvey { get; set; }
        public IList<string> Surveys { get; set; }
        public MainPage()
        {
            NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();
            TallennetutKyselyt();
            BindingContext = Main.GetInstance();
            BindingContext = this;

        }

        void TallennetutKyselyt()
        {
            Surveys = new List<String>();
            SurveyManager manager = SurveyManager.GetInstance();

            //NavigationPage.SetHasBackButton(this, false);

            Surveys = manager.GetTemplates();
            Surveys.Insert(0, "Oletuskysely");

            BindingContext = this;
        }


        //Device back button navigation test to close the application from the back button 
        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
        {
            var res = await this.DisplayAlert("Do you really want to exit the application?", "", "Yes", "No").ConfigureAwait(false);

            if (res) System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
        });
            return true;

        }



        void InfoClicked(object sender, EventArgs e)
        {
            InfoPopUp.IsVisible = true;
        }



        void InfoOKClicked(object sender, EventArgs e)
        {
            //commented out testing for ActivityVote vote1candidates
            /*
            Main.GetInstance().host.data.AddEmojiResults(2);
            Main.GetInstance().host.data.AddEmojiResults(5);
            Main.GetInstance().host.data.AddEmojiResults(4);
            Main.GetInstance().host.data.AddEmojiResults(4);
            Main.GetInstance().host.data.AddEmojiResults(2);
            Main.GetInstance().host.data.AddEmojiResults(5);
            Main.GetInstance().host.data.AddEmojiResults(5);
            Main.GetInstance().host.data.AddEmojiResults(4);
            Main.GetInstance().host.data.AddEmojiResults(2);
            Main.GetInstance().host.data.AddEmojiResults(3);

            Console.WriteLine(Main.GetInstance().host.data.ToString());

            Survey survey = new Survey();
            ActivityVote aVote = new ActivityVote();
            List<Emoji> emojis = survey.emojis;
            aVote.calcVote1Candidates(emojis, Main.GetInstance().host.data.GetEmojiResults());
            Console.WriteLine(survey.ToString());
            Console.WriteLine(aVote.ToString());
            Console.WriteLine("Time to vote in the 1st vote: {0}", aVote.vote1Timer);
            */

            //commented out testing for ActivityVote vote2candidates

            /*
            Dictionary<int, string> dict1 = new Dictionary<int, string>();
            dict1.Add(0,"fii");
            dict1.Add(1, "bar");
            dict1.Add(2, "heh");
            dict1.Add(3, "this");
            Dictionary<int, string> dict2 = new Dictionary<int, string>();
            dict2.Add(0, "fii");
            dict2.Add(1, "bar");
            dict2.Add(2, "heh");
            dict2.Add(3, "that");
            Dictionary<int, string> dict3 = new Dictionary<int, string>();
            dict3.Add(0, "fii");
            dict3.Add(1, "bor");
            dict3.Add(2, "hah");
            dict3.Add(3, "that");
            Main.GetInstance().host.data.AddVote1Results(dict1);
            Main.GetInstance().host.data.AddVote1Results(dict2);
            Main.GetInstance().host.data.AddVote1Results(dict3);


            Console.WriteLine(Main.GetInstance().host.data.ToString());

            ActivityVote aVote = new ActivityVote();
            aVote.calcVote2Candidates(Main.GetInstance().host.data.GetVote1Results());
            Console.WriteLine(aVote.ToString());
            */
            /*
            Main.GetInstance().host.data.AddVote2Results("Tunti ulkona");
            Main.GetInstance().host.data.AddVote2Results("Tunti ulkona");
            Main.GetInstance().host.data.AddVote2Results("5 min tauko");
            Main.GetInstance().host.data.AddVote2Results("5 min tauko");
            Main.GetInstance().host.data.AddVote2Results("5 min tauko");
            Main.GetInstance().host.data.AddVote2Results("Tunti ulkona");
            Main.GetInstance().host.data.AddVote2Results("Tunti ulkona");

            Console.WriteLine(Main.GetInstance().host.data.ToString());

            ActivityVote aVote = new ActivityVote();
            aVote.calcFinalResult(Main.GetInstance().host.data.GetVote2Results());
            Console.WriteLine(aVote.ToString());
            */
            InfoPopUp.IsVisible = false;
        }



        async void AvaaOpettaja(object sender, EventArgs e)
        {
            // siirrytään "luo uus kysely" sivulle
            Main.GetInstance().CreateNewSurvey();
            await Navigation.PushAsync(new Kirjautuminen());
        }

        async void TallennetutKyselytClicked(object sender, EventArgs e)
        {
            // siirrytään "Tallenetut Kyselyt" sivulle
            Main.GetInstance().BrowseSurveys();
            await Navigation.PushAsync(new TallennetutKyselyt());
        }

        async void LiityKyselyynClicked(object sender, EventArgs e)
        {
            // Siirrytään SyotaAvainkoodi-sivulle
            await Navigation.PushAsync(new SyotaAvainkoodi());
        }
    }
}
