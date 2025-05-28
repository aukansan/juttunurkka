/*
Copyright 2025 Riina Kaipia

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
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Prototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmojiAnswered : ContentPage
    {
        public string EmojiImageSource { get; set; }

        private readonly int _selectedEmojiId;
        private readonly int _remainingTime = 5;

        public EmojiAnswered(int selectedEmojiId)
        { 
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);

            _selectedEmojiId = selectedEmojiId;
            EmojiImageSource = $"emoji{_selectedEmojiId}.png";

            BindingContext = this;

            StartCountdown();
        }

        private async void StartCountdown()
        {
            int totalSeconds = _remainingTime;
            int elapsed = 0;

            while (elapsed < totalSeconds)
            {
                double progress = 1.0 - (double)elapsed / totalSeconds;
                progressBar.Progress = progress;
                await Task.Delay(1000);
                elapsed++;
            }

            progressBar.Progress = 0;

            await Navigation.PushAsync(new OdotetaanVastauksiaClient());
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}

