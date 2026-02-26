
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2026 Aurora Kansanoja

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

/* In this class, the user gives the juttunurkka a name and the juttunurkka key code is generated. */

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
    public partial class LuoKyselyLopetus : ContentPage
    {
        public string introMessage { get; set; } = "Kysymys: ";
        public LuoKyselyLopetus()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Survey s = SurveyManager.GetInstance().GetSurvey();
            introMessage += s.introMessage;
            BindingContext = this;

            NameEditor.Text = SurveyManager.GetInstance().GetSurvey().Name;

            String generatedCode = GenerateKeyNumber();
            KeyEditor.Text = generatedCode;
            s.RoomCode = generatedCode;
        }

        async void EdellinenButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        async void JatkaButtonClicked(object sender, EventArgs e)
        {
            if (NameEditor != null && !string.IsNullOrEmpty(NameEditor.Text) && KeyEditor != null && !string.IsNullOrEmpty(KeyEditor.Text))
            {

                SurveyManager man = SurveyManager.GetInstance();
                //save survey code and name
                man.GetSurvey().RoomCode = KeyEditor.Text;
                man.GetSurvey().Name = NameEditor.Text;
                //save survey
                man.SaveSurvey(NameEditor.Text + ".txt");

                // siirrytään esikatseluun
                await Navigation.PushAsync(new Esikatselu());

            }

            else await DisplayAlert("Nimi tai avainkoodi puuttuu", "Sinun on asetettava kyselylle nimi ja avainkoodi", "OK");
          
        }
       
        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            // jos tekstikenttä ei ole tyhjä
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                //isValid bool true/false arvo tarkastetaan käymällä läpi syötetyt characterit, että onko ne kirjaimia tai numeroita
                bool isValid = args.NewTextValue.ToCharArray().All(x => char.IsLetterOrDigit(x));
                
                //jos isValid on false, niin poistetaan kirjain heti, kun se kirjoitetaan
                ((Entry)sender).Text = isValid ? args.NewTextValue : args.NewTextValue.Remove(args.NewTextValue.Length - 1);
            }
        }

        private string GenerateKeyNumber(int length = 4)
        {
            const string numbers = "1234567890";
            Random random = new Random();
            return new string(Enumerable.Repeat(numbers, length).Select(s => s[random.Next(s.Length)]).ToArray()); ;

        }
    }
}