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

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;

namespace Prototype.LuoKysely
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LuoOmaVaihtoehto : ContentPage
    {
        private readonly Action<string> _callback;

        public LuoOmaVaihtoehto(Action<string> callback)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _callback = callback;
        }

        private async void TallennaButtonClicked(object sender, EventArgs e)
        {
            string input = OmaTeksti.Text?.Trim() ?? "";
            _callback?.Invoke(input);
            await Navigation.PopAsync();
        }

        private async void EdellinenButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
