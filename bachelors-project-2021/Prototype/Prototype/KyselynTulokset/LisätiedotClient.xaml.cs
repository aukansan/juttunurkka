
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Emmi Poutanen, Joni Lapinkoski

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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Prototype
{
    public partial class LisätiedotClient : ContentPage
    {
        public ResultsViewModel ViewModel { get; set; }

        public LisätiedotClient()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ViewModel = new ResultsViewModel();
            BindingContext = ViewModel;

            ProcessEmojiResults();
            ReceiveVote1();
        }

        void ProcessEmojiResults()
        {
            var emojiResults = Main.GetInstance()
                                   .client.summary
                                   .GetEmojiResults()
                                   .OrderByDescending(kvp => kvp.Value);

            int totalVotes = emojiResults.Sum(kvp => kvp.Value);

            foreach (var kvp in emojiResults)
            {
                var image = $"emoji{kvp.Key}lowres.png";
                var amount = kvp.Value;
                var height = totalVotes == 0
                             ? 0
                             : (int)((double)amount / totalVotes * 200);

                ViewModel.Results.Add(new ResultItem
                {
                    Image = image,
                    Amount = amount.ToString(),
                    Scale = height
                });
            }
        }

        async void ReceiveVote1()
        {
            bool success = await Main.GetInstance()
                                     .client
                                     .ReceiveVote1Candidates();
            if (success)
            {
                await Navigation.PushAsync(
                  new AktiviteettiäänestysEka());
            }
        }

        async void PoistuClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
              "Oletko varma että tahdot poistua kyselystä?",
              "",
              "Kyllä",
              "Ei");

            if (answer)
            {
                Main.GetInstance().client.DestroyClient();
                await Navigation.PopToRootAsync();
            }
        }
    }

    public class ResultsViewModel
    {
        public ObservableCollection<ResultItem> Results { get; }
            = new ObservableCollection<ResultItem>();
    }

    public class ResultItem
    {
        public string Image { get; set; }
        public string Amount { get; set; }
        public int Scale { get; set; }
    }
}