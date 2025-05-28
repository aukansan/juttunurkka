
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Riina Kaipia, Eemil Alamäki

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
    public partial class LuoKyselyToimenpiteetMiettiva : ContentPage
    {

        public IList<CollectionItem> Items { get; set; }
        private string myStringProperty;

        public string MyStringProperty
        {
            get { return myStringProperty; }
            set
            {
                myStringProperty = value;
                OnPropertyChanged(nameof(MyStringProperty)); // Notify that there was a change on this property
            }
        }

        public class CollectionItem
        {
            public Emoji Emoji { get; set; }
            public IList<Activity> ActivityChoises { get; set; }
            public ObservableCollection<object> Selected { get; set; }
            public CollectionItem(Emoji emoji, IList<Activity> activities)
            {
                Emoji = emoji;
                ActivityChoises = new ObservableCollection<Activity>(activities);

                if (emoji.Activities == null)
                    emoji.Activities = new List<Activity>();

                Selected = [];
                foreach (var item in emoji.Activities)
                {
                    if (ActivityChoises.Contains(item))
                        Selected.Add(item);
                }
            }
        }

        public LuoKyselyToimenpiteetMiettiva()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //alustus
            Items = new List<CollectionItem>();
            int numero = 0;
            foreach (var individual in SurveyManager.GetInstance().GetSurvey().emojis)
            {
                if (individual.Name == "Miettivä")
                {
                    var activities = new List<Activity>(Const.activities[5]);
                    activities.Add(new Activity { Title = "Luo oma vaihtoehto..." });
                    Items.Add(new CollectionItem(individual, activities));
                    break;
                }
                else
                {
                    numero++;
                }
            }
            BindingContext = this;

            int selectedEmojis = SurveyManager.GetInstance().GetSurvey().emojis.Count;
            int emojiNumber = numero + 1;
            String title = "Aktiviteetti " + emojiNumber + "/" + selectedEmojis;
            MyStringProperty = title;
        }


        async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is CollectionView cv && cv.BindingContext is CollectionItem item)
            {
                if (item.Selected.Count > 2)
                {
                    var lastSelected = e.CurrentSelection[e.CurrentSelection.Count - 1];
                    item.Selected.Remove(lastSelected);
                    cv.SelectedItems.Remove(lastSelected);
                    await DisplayAlert("Vain kaksi aktiviteettia", "Valitse vain kaksi aktiviteettia", "OK");
                    return;
                }

                foreach (var selectedItem in e.CurrentSelection)
                {
                    var selectedActivity = selectedItem as Activity;
                    if (selectedActivity == null)
                        continue;

                    if (selectedActivity.Title == "Luo oma vaihtoehto...")
                    {
                        // Poista valinta heti, jotta se ei jää aktiiviseksi
                        cv.SelectedItems.Remove(selectedActivity);

                        await Navigation.PushAsync(new Prototype.LuoKysely.LuoOmaVaihtoehto((syote) =>
                        {
                            if (!string.IsNullOrWhiteSpace(syote) && !item.ActivityChoises.Any(activity => activity.Title == syote))
                            {
                                var newActivity = new Activity { Title = syote, ImageSource = "dice.png"};
                                item.ActivityChoises.Insert(item.ActivityChoises.Count - 1, newActivity);
                                item.Selected.Add(newActivity);
                            }
                        }));

                        // Break after handling "Luo oma vaihtoehto...
                        break;
                    }
                }
            }
        }

        async void JatkaButtonClicked(object sender, EventArgs e)
        {

            //error if not all emojis have at least 2 selected activity
            if (!ActivitiesSet())
            {
                await DisplayAlert("Kaikkia valintoja ei ole tehty", "Sinun on valittava vähintään kaksi aktiviteettia", "OK");
                return;
            }

            //asetetaan emojit survey olioon
            List<Emoji> tempEmojis = [];
            List<Activity> tempActivities = [];

            foreach (var item in Items)
            {
                foreach (var selection in item.Selected)
                {
                    if (selection is Activity selectedActivity && !string.IsNullOrEmpty(selectedActivity.Title))
                        tempActivities.Add(selectedActivity);
                }
                item.Emoji.Activities = tempActivities;
                tempEmojis.Add(item.Emoji);
            }
            int numero = 0;
            foreach (var individual in SurveyManager.GetInstance().GetSurvey().emojis)
            {
                if (individual.Name == "Miettivä")
                {
                    SurveyManager.GetInstance().GetSurvey().emojis[numero].Activities = tempActivities;
                    break;
                }
                else
                {
                    numero++;
                }
            }
            int nextEmojiNumber = numero + 1;
            int luku = SurveyManager.GetInstance().GetSurvey().emojis.Count;

            if (nextEmojiNumber < luku)
            {
                String name = SurveyManager.GetInstance().GetSurvey().emojis[nextEmojiNumber].Name;

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
                else { }
            }
            else
            {
                await Navigation.PushAsync(new LuoKyselyLopetus());
            }
        }

        //function which checks whether the user has selected at least 2 activity for each emoji.
        private bool ActivitiesSet()
        {
            foreach (var item in Items)
            {
                if (item.Selected.Count != 2)
                {
                    return false;
                }
            }

            return true;
        }
    }
}