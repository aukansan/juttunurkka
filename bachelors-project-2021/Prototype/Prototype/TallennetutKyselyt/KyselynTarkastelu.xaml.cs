
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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KyselynTarkastelu : ContentPage
    {

		public IList<CollectionItem> Emojit { get; private set; } = null;
        public string roomCode { get; set; } = "Roomcode: ";
        public string introMessage { get; set; } = "Intro: ";

        public static bool canDelete = true;
        public static bool canEdit = true;

        public class CollectionItem
        {
            public Emoji Item { get; set; } = null;
            public IList<Activity> ActivityChoises { get; set; } = null;
            public string Color { get; set; } = null;
        }


        public KyselynTarkastelu()
        {
            InitializeComponent();

            Survey s = SurveyManager.GetInstance().GetSurvey();

            //alustetaan emojit kyselyn emojeilla
            Emojit = new List<CollectionItem>();
            List<Emoji> temp = s.emojis;

            //asetetaan otsikoksi kyselyn nimi
            title.Text = s.Name;

            //asetetaan kyselyn roomCode ja intro
            roomCode += s.RoomCode;
            introMessage += s.introMessage;

            //asetetaan poista napin tila (oletusta ei voi poistaa)
            PoistaButton.IsEnabled = canDelete;

            //asetetaan muokkaa napin tila
            MuokkaaButton.IsEnabled = canEdit;

            //Kuvataan emojien vakavuusasteet
            foreach (var item in temp)
            {
                CollectionItem i = new CollectionItem();
                i.Item = item;
                i.ActivityChoises = item.Activities;
                switch (item.Impact)
                {
                    case "positive":
                        i.Color = "Green";
                        break;
                    case "neutral":
                        i.Color = "Yellow";
                        break;
                    case "negative":
                        i.Color = "Red";
                        break;
                }
                Emojit.Add(i);
            }

            BindingContext = this;
        }

        //Device back button navigation 
        protected override bool OnBackButtonPressed()
        {
            Navigation.PushAsync(new TallennetutKyselyt());
            return true;

        }


        async void JaaClicked(object sender, EventArgs e)
        {
            //siirrytään OdotetaanVastauksia sivulle
            await Navigation.PushAsync(new OdotetaanOsallistujiaOpettaja());
        }
        async void MuokkaaClicked(object sender, EventArgs e)
        {
            // Siirrytään kyselyn muokkaukseen
            Main.GetInstance().EditSurvey();
            await Navigation.PushAsync(new LuoKyselyJohdatus());
        }

        void PoistaClicked(object sender, EventArgs e)
        {
            popupSelection.IsVisible = true;

        }

        void X_Clicked(object sender, EventArgs e)
        {

            // Suljetaan popup
            popupSelection.IsVisible = false;

        }

        void Ei_Clicked(object sender, EventArgs e)
        {
            // siirrytään yhteenveto Host sivulle, ei tallenneta kyselyä
             popupSelection.IsVisible = false;
        }

        async void Kyllä_Clicked(object sender, EventArgs e)
        { 

            //kyselyn Poistaminen!
            SurveyManager.GetInstance().DeleteSurvey(SurveyManager.GetInstance().GetSurvey().Name + ".txt");
            // siirrytään Tallenetut kyselyt sivulle 
           
             await Navigation.PushAsync(new TallennetutKyselyt());

        }


        void btnPopupButton_Clicked(object sender, EventArgs e)
        {
        
            if (sender is Button b && b.Parent is Microsoft.Maui.Controls.Compatibility.Grid g && g.Children[2] is Frame f)
            {
                if (f.IsVisible == false) { 
              
                    f.IsVisible = true;
                }

                else if (f.IsVisible == true)
                {

                    f.IsVisible = false;
                }

            }               
         } 
        
        }
    }
