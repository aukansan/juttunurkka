/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen

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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Esikatselu : ContentPage
    {
        public string introMessage { get; set; }
        public string RoomCode { get; set; }
        public string Name { get; set; }
        public IList<CollectionItem> Emojit { get; private set; } = null;

        public class CollectionItem
        {
            public Emoji Item { get; set; } = null;
            public IList<string> ActivityChoises { get; set; } = null;
        }

        public Esikatselu()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Survey survey = SurveyManager.GetInstance().GetSurvey();
            introMessage += survey.introMessage;
            RoomCode = survey.RoomCode;
            Name = survey.Name;

            Emojit = [];
            List<Emoji> temp = survey.emojis;

            foreach (var item in temp)
            {
                CollectionItem i = new CollectionItem
                {
                    Item = item,
                    ActivityChoises = [.. item.Activities.Select(activity => activity.Title!)]
                };
                Emojit.Add(i);
            }

            BindingContext = this;
        }

        async void ValmisButtonClicked(object sender, EventArgs e)
        {
            var res = await DisplayAlert("Juttunurkka tallennettu! Haluatko avata juttunurkan heti?", "", "Kyllä", "Ei");

            if (res == true)
            {
                //siirrytään odottamaan osallistujia
                await Navigation.PushAsync(new OdotetaanOsallistujiaOpettaja());

            }
            else
            {
                SurveyManager.GetInstance().ResetSurvey();
                await Navigation.PushAsync(new Opettajanhuone());
            }

        }
    }
}