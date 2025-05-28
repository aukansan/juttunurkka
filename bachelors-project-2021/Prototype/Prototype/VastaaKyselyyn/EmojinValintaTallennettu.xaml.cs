
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Riina Kaipia

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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmojinValintaTallennettu : ContentPage
    {
        public string introMessage { get; set; }
        private int answer;

        // <---
        public IList<CollectionItem> Emojit { get; private set; } = null;
        // --->

        //Haettu esikatseluosiosta apuun
        //<---
        public class CollectionItem
        {
            public Emoji Item { get; set; } = null;
        }
        //--->

        public EmojinValintaTallennettu()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);

            //<---
            Survey s = SurveyManager.GetInstance().GetSurvey();
            introMessage += s.introMessage; 

            Emojit = new List<CollectionItem>();
            List<Emoji> temp = s.emojis;

            foreach (var item in temp)
            {
                CollectionItem i = new CollectionItem();
                i.Item = item;
                Emojit.Add(i);
            }
            

            BindingContext = this;
        }

        //Device back button disabled
        protected override bool OnBackButtonPressed()
        {
            return true;

        }



        private void Button_Clicked(object sender, EventArgs e)
        {

            ImageButton emoji = sender as ImageButton;

            emoji.Scale = 1.75;
            answer = int.Parse(emoji.ClassId.ToString());

            // Tarkistetaan, että vaan yhden valihtee
            Console.WriteLine("valittu " + answer);
            Vastaus.IsEnabled = true;

        }

        private async void Vastaa_Clicked(object sender, EventArgs e)
        {
            await Main.GetInstance().client.SendResult(answer.ToString());
            await Navigation.PushAsync(new EmojiAnswered(answer));
        }
    }
}