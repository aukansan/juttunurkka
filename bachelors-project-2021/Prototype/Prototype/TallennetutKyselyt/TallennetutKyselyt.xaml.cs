
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
    public partial class TallennetutKyselyt : ContentPage
    {

        public enum ToolbarItemPosition { Start, End }



        public string SelectedSurvey { get; set; }
        public List<string> Surveys { get; set; }
        public TallennetutKyselyt()
        {
            InitializeComponent();
            Surveys = new List<String>();
            SurveyManager manager = SurveyManager.GetInstance();

            NavigationPage.SetHasBackButton(this, false);


            Surveys = manager.GetTemplates();



            BindingContext = this;
        }

        //Device back button navigation 
        protected override bool OnBackButtonPressed()
        {
            Navigation.PushAsync(new MainPage());
            return true;

        }

        void OnListSelection(object sender, SelectionChangedEventArgs e)
        {
            SelectedSurvey = e.CurrentSelection[0] as string;

            //button enabled only when there is survey selected

            if (SelectedSurvey != null)
                TButton.IsEnabled = true;

            else
                TButton.IsEnabled = false;
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

        async void BackBtnClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}