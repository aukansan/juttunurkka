
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Riina Kaipia

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
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LuoKyselyToimenpiteetIloinen : ContentPage
    {
        public IList<CollectionItem> Items { get; set; }
        private string myStringProperty = string.Empty;

        public string MyStringProperty
        {
            get { return myStringProperty; }
            set
            {
                myStringProperty = value;
                OnPropertyChanged(nameof(MyStringProperty));
            }
        }

        public class CollectionItem
        {
            public Emoji Emoji { get; set; }
            public ObservableCollection<Activity> ActivityChoises { get; set; }
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

        public LuoKyselyToimenpiteetIloinen()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            Items = new List<CollectionItem>();

            int numero = 0;
            foreach (var emo in SurveyManager.GetInstance().GetSurvey().emojis)
            {
                if (emo.Name == "Iloinen")
                {
                    var activities = new List<Activity>(Const.activities[0]);
                    activities.Add(new Activity { Title= "Luo oma vaihtoehto..." });
                    Items.Add(new CollectionItem(emo, activities));
                    break;
                }
                numero++;
            }

            int selectedEmojis = SurveyManager.GetInstance().GetSurvey().emojis.Count;
            int emojiNumber = numero + 1;
            MyStringProperty = $"Aktiviteetti {emojiNumber}/{selectedEmojis}";

            BindingContext = this;
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
            if (!ActivitiesSet())
            {
                await DisplayAlert("Kaikkia valintoja ei ole tehty", "Sinun on valittava vähintään kaksi aktiviteettia", "OK");
                return;
            }

            List<Activity> tempActivities = [];

            foreach (var item in Items)
            {
                foreach (var selection in item.Selected)
                {
                    Activity? selectedActivity = selection as Activity;
                    if (selectedActivity != null && !string.IsNullOrEmpty(selectedActivity.Title))
                        tempActivities.Add(selectedActivity);
                }
                item.Emoji.Activities = tempActivities;
            }

            var emojis = SurveyManager.GetInstance().GetSurvey().emojis;
            int numero = 0;

            foreach (var individual in emojis)
            {
                if (individual.Name == "Iloinen")
                {
                    emojis[numero].Activities = tempActivities;
                    break;
                }
                numero++;
            }

            int nextEmojiNumber = numero + 1;
            int luku = emojis.Count;

            if (nextEmojiNumber < luku)
            {
                string name = emojis[nextEmojiNumber].Name;

                if (name == "Iloinen")
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetIloinen());
                else if (name == "Hämmästynyt")
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetHammastynyt());
                else if (name == "Neutraali")
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetNeutraali());
                else if (name == "Vihainen")
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetVihainen());
                else if (name == "Väsynyt")
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetVasynyt());
                else if (name == "Miettivä")
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetMiettiva());
                else if (name == "Itkunauru")
                    await Navigation.PushAsync(new LuoKyselyToimenpiteetItkunauru());
                else
                    await Navigation.PushAsync(new LuoKyselyLopetus());
            }
            else
            {
                await Navigation.PushAsync(new LuoKyselyLopetus());
            }
        }

        private bool ActivitiesSet()
        {
            foreach (var item in Items)
            {
                if (item.Selected.Count != 2)
                    return false;
            }
            return true;
        }
    }
}