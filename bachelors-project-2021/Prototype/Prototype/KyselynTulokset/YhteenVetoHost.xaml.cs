
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
    public partial class YhteenVetoHost : ContentPage
    {
        public IList<string> resultImages { get; set; }
        public IList<double> resultScale { get; set; }
        public YhteenVetoHost()
        {
            InitializeComponent();
            resultImages = new List<string>();
            resultScale = new List<double>();

            int count = 0;
            double calculateScale = 0.0;
            Dictionary<int, int> sorted = new Dictionary<int, int>();
			foreach (var item in Main.GetInstance().host.data.GetEmojiResults().OrderByDescending(item => item.Value))
			{
                sorted.Add(item.Key, item.Value);
                count += item.Value;
            }

            foreach (int key in sorted.Keys)
            {
                resultImages.Add("emoji" + key.ToString() + ".png");
            }

            if (count == 0)
            {
                foreach (int value in sorted.Values)
                {
                    resultScale.Add(0);
                }
            } else 
            {
                foreach (int value in sorted.Values)
                {
                    calculateScale = 3 * (double)value / count;
                    resultScale.Add(calculateScale);
                }
            }
            BindingContext = this;
        }
        async void LopetaClicked(object sender, EventArgs e)
        {
            //Sulkee kyselyn kaikilta osallistujilta

            var res = await DisplayAlert("Oletko varma että tahdot sulkea kyselyn?", "", "Kyllä", "Ei");

            if (res == true)
            {
                await Navigation.PopToRootAsync();
            }
            else return;

           
        }
        async void JatkaClicked(object sender, EventArgs e)
        {
            
            //Siirrytään odottamaan äänestyksen tuloksia (HOST)
            await Navigation.PushAsync(new TulostenOdotus());
        }
    }
}