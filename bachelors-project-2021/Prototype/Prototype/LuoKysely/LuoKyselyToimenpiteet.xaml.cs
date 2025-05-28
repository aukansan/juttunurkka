
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
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LuoKyselyToimenpiteet : ContentPage
    {

        public IList<CollectionItem> Items { get; set; }
        public class CollectionItem

        {
            public Emoji Emoji { get; set; }
            public IList<Activity> ActivityChoises { get; set; }

            public ObservableCollection<Activity> Selected { get; set; }
            public CollectionItem(Emoji emoji, IList<Activity> activities)
            {
                Emoji = emoji;
                ActivityChoises = activities;
               
                Selected = new ObservableCollection<Activity>();
                foreach (var item in emoji.Activities)
                {
                    Selected.Add(ActivityChoises[ActivityChoises.IndexOf(item)]);
                }
            }
        }

        public LuoKyselyToimenpiteet()
        {
            InitializeComponent();

                  //alustus
                Items = new List<CollectionItem>();
                if(SurveyManager.GetInstance().GetSurvey().emojis[0].Name == "Iloinen")
            {
                Items.Add(new CollectionItem(SurveyManager.GetInstance().GetSurvey().emojis[0], Const.activities[0]));
                Console.WriteLine("Emojin nimi:" + new CollectionItem(SurveyManager.GetInstance().GetSurvey().emojis[0], Const.activities[0]).Emoji.Name);
                Console.WriteLine("Aktiviteetteja: " + new CollectionItem(SurveyManager.GetInstance().GetSurvey().emojis[0], Const.activities[0]).ActivityChoises.Count);
                Console.WriteLine("Emojin aktiviteetit: " + new CollectionItem(SurveyManager.GetInstance().GetSurvey().emojis[0], Const.activities[0]).ActivityChoises[0]);

            }
            else {
                //dosmthing else
            }
            
            BindingContext = this;
         
        }

        async void EdellinenButtonClicked(object sender, EventArgs e)
        {
            var res = await DisplayAlert("Tahdotko varmasti keskeyttää kyselyn tekemisen?", "", "Kyllä", "Ei");

            if (res == true)
            {
                //survey resetoidaan
                SurveyManager.GetInstance().ResetSurvey();

                //Jos ollaan edit tilassa, niin siirrytään takaisin kyselyntarkastelu sivulle, muutoin main menuun
                if (Main.GetInstance().GetMainState() == Main.MainState.Editing)
                {
                    Main.GetInstance().BrowseSurveys();
                    await Navigation.PopAsync();
                }
                else
                {
                    // siirrytään etusivulle
                    await Navigation.PopToRootAsync();
                }

            }
            else return;
        }

        //?
        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is CollectionView cv && cv.SelectionChangedCommandParameter is CollectionItem item)
            {

            }
        }


        async void JatkaButtonClicked(object sender, EventArgs e)
        {
            
			//error if not all emojis have at least 1 selected activity
/*			if (!ActivitiesSet())
			{
                await DisplayAlert("Kaikkia valintoja ei ole tehty", "Sinun on valittava jokaiselle mielialalle vähintään yksi aktiviteetti", "OK");
                return;
            }
*/
            //asetetaan emojit survey olioon
            List<Emoji> tempEmojis = new List<Emoji>();
            foreach (var item in Items)
            {
                List<Activity> tempActivities = [];
                foreach (var selection in item.Selected)
                {
                    Console.WriteLine("Lisätään aktiviteetti:" +selection as string);
                    Console.WriteLine(item.Emoji.Name +"emojin yhteyteen");
                    tempActivities.Add(selection);
                }
                item.Emoji.Activities = tempActivities;
                tempEmojis.Add(item.Emoji);
            }
            SurveyManager.GetInstance().GetSurvey().emojis = tempEmojis;

            // siirrytään "Luo kysely -lopetus" sivulle 
            await Navigation.PushAsync(new LuoKyselyLopetus()); ;
        }
/*
        //function which checks whether the user has selected at least 1 activity for each emoji.
        private bool ActivitiesSet()
        {
            foreach (var item in Items)
            {
                if (item.Selected.Count == 0)
                {
                    return false;
                }
            }

            return true;
        }*/
    }
}