
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

using System.Collections.Generic;
using System;

namespace Prototype
{
	public class Survey
	{
		public string introMessage { get; set; }
		public List<Emoji> emojis { get; set; }
		public string RoomCode { get; set; }
		public string Name { get; set; }
		
		//empty constructor creates blank survey
		public Survey() {
			introMessage = null;
			emojis = new List<Emoji>();

			emojis.Add(new Emoji(0, "Iloinen", "neutral", false, new List<Activity>(), "emoji0lowres.png"));
			emojis.Add(new Emoji(1, "Hämmästynyt", "neutral", false, new List<Activity>(), "emoji1lowres.png"));
			emojis.Add(new Emoji(2, "Neutraali", "neutral", false, new List<Activity>(), "emoji2lowres.png"));
			emojis.Add(new Emoji(3, "Vihainen", "neutral", false, new List<Activity>(), "emoji3lowres.png"));
			emojis.Add(new Emoji(4, "Väsynyt", "neutral", false, new List<Activity>(), "emoji4lowres.png"));
			emojis.Add(new Emoji(5, "Miettivä", "neutral", false, new List<Activity>(), "emoji5lowres.png"));
			emojis.Add(new Emoji(6, "Itkunauru", "neutral", false, new List<Activity>(), "emoji6lowres.png"));

			RoomCode = null;
			Name = null;
		}
		public Survey(string introMessage, List<Emoji> emojis, string RoomCode, string Name)
		{
			this.introMessage = introMessage;
			this.emojis = emojis;
			this.RoomCode = RoomCode;
			this.Name = Name;
		}

		//default survey consists of first intro entry, 7 emojis with various impact each with 3 first entries of activities and a random roomcode
		public static Survey GetDefaultSurvey()
		{
			string tempIntro = Const.intros[0];
			List<Emoji> tempEmojis = new List<Emoji>();

			List<Activity> activities;

			Const.activities.TryGetValue(0, out activities);
			activities = activities.GetRange(0, 3);
			tempEmojis.Add(new Emoji(0, "Iloinen", "neutral", false, activities, "emoji0lowres.png"));

			Const.activities.TryGetValue(1, out activities);
			activities = activities.GetRange(0, 2);
			tempEmojis.Add(new Emoji(1, "Hämmästynyt", "neutral", false, activities, "emoji1lowres.png"));

			Const.activities.TryGetValue(2, out activities);
			activities = activities.GetRange(0, 3);
			tempEmojis.Add(new Emoji(2, "Neutraali", "neutral", false, activities, "emoji2lowres.png"));

			Const.activities.TryGetValue(3, out activities);
			activities = activities.GetRange(0, 2);
			tempEmojis.Add(new Emoji(3, "Vihainen", "neutral", false, activities, "emoji3lowres.png"));

			Const.activities.TryGetValue(4, out activities);
			activities = activities.GetRange(0, 2);
			tempEmojis.Add(new Emoji(4, "Väsynyt", "neutral", false, activities, "emoji4lowres.png"));

			Const.activities.TryGetValue(5, out activities);
			activities = activities.GetRange(0, 3);
			tempEmojis.Add(new Emoji(5, "Miettivä", "neutral", false, activities, "emoji5lowres.png"));

			Const.activities.TryGetValue(6, out activities);
			activities = activities.GetRange(0, 3);
			tempEmojis.Add(new Emoji(6, "Itkunauru", "neutral", false, activities, "emoji6lowres.png"));

			string TempRoomCode = GenerateRandomCode();

			return new Survey(tempIntro, tempEmojis, TempRoomCode, "Oletus");
		}

		//generates random room code of 5 numbers
		private static string GenerateRandomCode()
		{
			Random r = new Random();
			string temp = "";

			for (int i = 0; i < 5; i++)
			{
				temp += r.Next(10).ToString();
			}

			return temp;
		}
		//toString method for getting the info of the survey in a string for data purposes
		public override string ToString() {
			string value = "";

			value += $"Intro: {introMessage}\n";

			foreach (var item in emojis)
			{
				value += item.ToString();
				value += "\n";
			}

			value += $"RoomCode: {RoomCode}";

			return value;
		}
	}
}
