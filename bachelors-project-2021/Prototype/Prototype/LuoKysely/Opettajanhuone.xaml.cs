/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Petri Pentinpuro, Emmi Poutanen

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
using System.Diagnostics;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Opettajanhuone : ContentPage
    {
        public string SelectedSurvey { get; set; }
        public IList<string> Surveys { get; set; }

        public Opettajanhuone()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            TallennetutKyselyt();
            BindingContext = this;
        }

        void TallennetutKyselyt()
        {
            Surveys = new List<String>();
            SurveyManager manager = SurveyManager.GetInstance();

            //NavigationPage.SetHasBackButton(this, false);

            Surveys = manager.GetTemplates();
            Surveys.Insert(0, "Oletuskysely");

            BindingContext = this;
        }

        async void UusiJuttunurkkaClicked(object sender, EventArgs e)
        {

            //siirrytään "luo uus kysely 1/3" sivulle 
            await Navigation.PushAsync(new LuoKyselyJohdatus());
        }
        private async void ArkistoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TallennetutKyselyt());
        }

        async void OletusClicked(object sender, EventArgs e)
        {
            SurveyManager.GetInstance().SetDefaultSurvey();
            KyselynTarkastelu.canDelete = false;
            KyselynTarkastelu.canEdit = false;
            await Navigation.PushAsync(new KyselynTarkastelu());
        }

        async void AvaaClicked(object sender, EventArgs e)
        {
            string surveyName = SelectedSurvey + ".txt";
            SurveyManager manager = SurveyManager.GetInstance();
            manager.LoadSurvey(surveyName);

            Console.WriteLine(surveyName);

            KyselynTarkastelu.canDelete = true;
            KyselynTarkastelu.canEdit = true;
            await Navigation.PushAsync(new KyselynTarkastelu());

        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex == -1)
            {
                return;
            }

            SelectedSurvey = picker.Items[picker.SelectedIndex];


            if (selectedIndex == 0)
            {
                OletusClicked(sender, e);  //avaa oletuskyselyn
            }
            else
            {
                AvaaClicked(sender, e);   //avaa tallennetun kyselyn
            }

            
        }


    }
}