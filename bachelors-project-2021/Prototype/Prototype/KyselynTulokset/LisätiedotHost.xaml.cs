/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Joni Lapinkoski

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
using Microsoft.Maui.Graphics;

namespace Prototype
{
    public partial class LisätiedotHost : ContentPage
    {
        public ObservableCollection<HostResultItem> Results { get; } = new();

        public LisätiedotHost()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;
            LoadHostEmojiResults();
        }

        void LoadHostEmojiResults()
        {
            var raw = Main
                .GetInstance()
                .host
                .data
                .GetEmojiResults()
                .OrderByDescending(kv => kv.Value)
                .ToList();

            double maxCount = raw.Any() ? raw.Max(kv => kv.Value) : 1;
            const double maxBarHeight = 200;
            const double minBarHeight = 30;

            var barColors = new[] { Colors.Blue, Colors.Red, Colors.Green };
            var survey = SurveyManager.GetInstance().GetSurvey();

            for (int i = 0; i < raw.Count; i++)
            {
                var kv = raw[i];

                double rawH = (kv.Value / maxCount) * maxBarHeight;
                double heightPx = Math.Max(rawH, minBarHeight);

                Results.Add(new HostResultItem
                {
                    Image = $"emoji{kv.Key}lowres.png",
                    Title = survey.emojis[i].Name,
                    Amount = kv.Value.ToString(),   // "0" if zero
                    ScalePx = heightPx,
                    Color = barColors.Length > i
                                ? barColors[i]
                                : Colors.Gray
                });
            }
        }

        async void KeskeytäClicked(object sender, EventArgs e)
        {
            bool ok = await DisplayAlert("Haluatko varmasti sulkea huoneen?", "", "Kyllä", "Ei");
            if (!ok) return;

            if (Main.GetInstance().state == Main.MainState.Participating)
                Main.GetInstance().client.DestroyClient();
            else
                Main.GetInstance().host.DestroyHost();

            await Navigation.PopToRootAsync();
        }

        async void JatkaClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new TulostenOdotus());
    }

    public class HostResultItem
    {
        public string Image { get; set; }
        public string Title { get; set; }
        public string Amount { get; set; }
        public double ScalePx { get; set; }
        public Color Color { get; set; }
    }
}