
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
    public partial class LuoKyselyJohdatus : ContentPage
    {

        public IList<string> introMessage { get; set; }
        public string selectedItem = null;



        public LuoKyselyJohdatus()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            introMessage = Const.intros;



            if(Main.GetInstance().GetMainState() == Main.MainState.Editing)
            {
                selectedItem = SurveyManager.GetInstance().GetSurvey().introMessage;
                
            }
           

       
            BindingContext = this;


 
        }


        async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;
            
            if (selectedIndex != -1) {

                selectedItem = KysymysPicker.Items[selectedIndex];

                if (selectedItem == "Luo oma kysymys...")
                {
                    await Navigation.PushAsync(new Omakysymys());
                    return;
                }
                selectedItem = KysymysPicker.Items[KysymysPicker.SelectedIndex];
                JatkaBtn.IsEnabled = true;
            }

            else {
                JatkaBtn.IsEnabled = false;
            }
        }



        async void JatkaButtonClicked(object sender, EventArgs e)
        {
            //tallentaa kyselyn kysymyksen
            SurveyManager.GetInstance().GetSurvey().introMessage = selectedItem;
          

            //siirrytään emojin valinta sivulle 
            await Navigation.PushAsync(new LuoKyselyEmojit());
        }

        async void EdellinenButtonClicked(object sender, EventArgs e) 
        {
            //survey resetoidaan
            SurveyManager.GetInstance().ResetSurvey();

            await Navigation.PushAsync(new Opettajanhuone());
        }
    }

  
}