
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
    public partial class LuoKyselyEmojit : ContentPage
    {

        public IList<CollectionItem> Emojit { get; private set; }
        
        public class CollectionItem {
            public Emoji Item { get; set; } = null;
            public bool CheckBox { get; set; } = false;
            public bool IsNegative { get; set; } = false;
            public bool IsNeutral { get; set; } = false;
            public bool IsPositive { get; set; } = false;
		}

       

        public LuoKyselyEmojit()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //alustetaan emojit kyselyn emojeilla
            Emojit = new List<CollectionItem>();

            List<Emoji> temp = SurveyManager.GetInstance().GetSurvey().emojis;
            
			foreach (var item in temp)
			{
                CollectionItem i = new CollectionItem();
				i.Item = item;

				switch (item.Impact)
				{
					case "positive":
                        i.IsPositive = true;
						break;
                    case "neutral":
                        i.IsNeutral =true;
                        break;
                    case "negative":
                        i.IsNegative = true;
                        break;
                }
                i.CheckBox = false;
                Emojit.Add(i);
			}

            BindingContext = this;
        }

        void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            JatkaBtn.IsEnabled = EmojisSet();
        }

        async void EdellinenButtonClicked(object sender, EventArgs e) 
        {
            await Navigation.PopAsync();
        }
        async void JatkaButtonClicked(object sender, EventArgs e)
        {
            if (!EmojisSet())
            {
                await DisplayAlert("Kaikkia valintoja ei ole tehty", "Valitse vähintään kaksi emojia", "OK");
                return;

            }
            //asetetaan emojit survey olioon
            List<Emoji> temp = new List<Emoji>();
            foreach (var item in Emojit)
			{
                if (item.IsPositive) {
                    item.Item.Impact = "positive";
				} else if (item.IsNeutral) {
                    item.Item.Impact = "neutral";
                } else if (item.IsNegative)
                    item.Item.Impact = "negative";
                if (item.CheckBox ==true )
                {
                    Console.WriteLine("on tosi");
                    item.Item.IsChecked = true;
                    // vain jos emojiin liittyvä checkbox on chekattu niin lisätään se emojilistaan.PP
                    temp.Add(item.Item);
                    Console.WriteLine(item.Item.Name);
                }
                else
                {
                    //
                }
 
                

            }
            //katsotaan mikä on listan pituus konsolilta PP
            Console.WriteLine(temp.Count);
            SurveyManager.GetInstance().GetSurvey().emojis = temp;


            // siirrytään aktiviteetit sivulle 
            
            //Siirtymä aktiviteettien valintaan eri emojien sivuille
            String name = SurveyManager.GetInstance().GetSurvey().emojis[0].Name;
                        {
                if (name == "Iloinen")
                {
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetIloinen());
                }
                else if (name == "Hämmästynyt")
                {
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetHammastynyt());
                }
                else if (name == "Neutraali")
                {
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetNeutraali());

                }
                else if (name == "Vihainen")
                {
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetVihainen());

                }
                else if (name == "Väsynyt")
                {
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetVasynyt());

                }
                else if (name == "Miettivä")
                {
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetMiettiva());

                }
                else if (name == "Itkunauru")
                {
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetItkunauru());

                }         
                else
                {
                    await Navigation.PushAsync(new LuoKyselyLopetus());
                }
                
             }
        }

        private bool EmojisSet()
        {
            int numberOfSelected = 0;

            foreach (var item in Emojit)
            {
                if (item.CheckBox == true)
                {
                    numberOfSelected++;
                    if (numberOfSelected > 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}