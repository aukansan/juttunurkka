
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
using System.Diagnostics;
using System.Text;

namespace Prototype
{
	public class SurveyData
	{
		public Dictionary<int, int> emojiResults { get; private set; }
		public Dictionary<Activity, int> vote1Results { get; private set; }
		public Dictionary<string, int> vote2Results { get; private set; }

		public string voteResult { get; set; }
		public int totalEmojis = 0;

		public SurveyData() {
			emojiResults = new Dictionary<int, int>(SurveyManager.GetInstance().GetSurvey().emojis.Count);
			vote1Results = new Dictionary<Activity, int>();

			foreach(var item in SurveyManager.GetInstance().GetSurvey().emojis)
            {
				if (item.Name == "Iloinen")
				{
					emojiResults.Add(0, 0);
				}
				else if (item.Name == "Hämmästynyt")
				{
					emojiResults.Add(1, 0);
				}
				else if (item.Name == "Neutraali")
				{
					emojiResults.Add(2, 0);

				}
				else if (item.Name == "Vihainen")
				{
					emojiResults.Add(3, 0);

				}
				else if (item.Name == "Väsynyt")
				{
					emojiResults.Add(4, 0);

				}
				else if (item.Name == "Miettivä")
				{
					emojiResults.Add(5, 0);
				}
				else if (item.Name == "Itkunauru")
                {
					emojiResults.Add(6, 0);

				}

			}

			/*	emojiResults.Add(0, 0);
			emojiResults.Add(1, 0);
			emojiResults.Add(2, 0);
			emojiResults.Add(3, 0);
			emojiResults.Add(4, 0);
			emojiResults.Add(5, 0);
			emojiResults.Add(6, 0);*/
		}
		
		//Adds a single emoji answer to results
		public void AddEmojiResults(int emoji) {
			int count;
			if (emojiResults.TryGetValue(emoji, out count)) {
				count++;
				emojiResults[emoji] = count;
				totalEmojis++;
				return;
			}
		}

		public void AddVote1Results(Activity activity)
        {
            if (vote1Results.ContainsKey(activity))
            {
                // Increment the vote count for the existing activity
                vote1Results[activity]++;
                Console.WriteLine($"Activity '{activity.Title}' already exists. Incremented vote count to {vote1Results[activity]}.");
            }
            else
            {
                // Add the new activity to the dictionary with an initial vote count of 1
                vote1Results.Add(activity, 0);
                Console.WriteLine($"Activity '{activity.Title}' added to vote1Results with an initial vote count of 1.");
            }
        }

		public void AddVote2Results(string activity)
        {
			int count;
			
			//Check if activity has been voted already
			if (vote2Results.TryGetValue(activity, out count))
			{
				Console.WriteLine("In Loop:: Activity: {0}", activity);
				count++;
				vote2Results[activity] = count;
			}
			Console.WriteLine("Activity: {0}", activity);

			//check if activity exists at all
			if (vote2Results.ContainsKey(activity) == false)
			{
				vote2Results.Add(activity, 1);
			}
		}

		//get emojiResults
		public Dictionary<int, int> GetEmojiResults() {
			return emojiResults;
		}

		//get Vote1Results
		public Dictionary<Activity, int> GetVote1Results()
        {
			return vote1Results;
        }

		//get Vote2Results
		public Dictionary<string, int> GetVote2Results()
        {
			return vote2Results;
        }
		public override string ToString() {
			string s = "{emojiResults: ";
			foreach (var item in emojiResults)
			{
				s += "[";
				s += item.Key;
				s += ": ";
				s += item.Value;
				s += "],";
			}
			s = s.Substring(0, s.Length - 1);
			s += "}";
			s += "{vote1Results: ";
			foreach (var item in vote1Results)
			{
				s += "[";
				s += item.Key;
				s += ": ";
				s += item.Value;
				s += "],";
			}
			s = s.Substring(0, s.Length - 1);
			s += "}";
			return s;
		}
	}
}
