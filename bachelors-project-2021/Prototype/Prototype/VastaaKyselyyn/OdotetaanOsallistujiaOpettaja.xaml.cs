
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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OdotetaanOsallistujiaOpettaja : ContentPage, System.ComponentModel.INotifyPropertyChanged
    {
        public string roomCode { get; set; } = "Avainkoodi: ";

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
                }
            }
        }

        public OdotetaanOsallistujiaOpettaja()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            roomCode += SurveyManager.GetInstance().GetSurvey().RoomCode;
            BindingContext = this; 

            //Ei enää mahdollista päästä takaisin kysleyn luontiin painamalla navigoinnin backbuttonia 
            NavigationPage.SetHasBackButton(this, true);

            //actually run the survey
            //Host();
        }
        
        
        private async void Host()
		{
            //hakee osallistujien määrän?
            //clientCount = Main.GetInstance().host.clientCount;
            
            if (!await Main.GetInstance().HostSurvey())
            {
                //host survey ended in a fatal unexpected error, aborting survey.
                //pop to root and display error
                await Navigation.PopToRootAsync();
                await DisplayAlert("Kysely suljettiin automaattisesti1", "Tapahtui odottamaton virhe.", "OK");
            }
        }
        

        private async void AloitaButtonClicked(object sender, EventArgs e)
        {
            //siirrytään odottamaan vastauksia
            await Navigation.PushAsync(new OdotetaanVastauksiaOpe());
        }
                 

        private async void KeskeytaButtonClicked(object sender, EventArgs e)
        {

            //Varmistu kyselyn peruuttamisen yhteydessä keskeyttää

            var res = await DisplayAlert("Oletko varma että tahdot keskeyttää kyselyn?", "", "Kyllä", "Ei");

            if (res == true) {
                await Navigation.PopToRootAsync();
            }
            else return; 
           

        }
    }
}