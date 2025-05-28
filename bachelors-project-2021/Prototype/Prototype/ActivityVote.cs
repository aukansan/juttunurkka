
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

namespace Prototype
{
    public class ActivityVote
    {
        private Dictionary<int, IList<Activity>> vote1Candidates;
        private readonly int totalCount = Main.GetInstance().host.data.totalEmojis;
        private string finalResult;
        public int vote1Timer = 0;
        public int coolDown = 5;


        public ActivityVote ()
        {
            vote1Candidates = new Dictionary<int, IList<Activity>>();
        }

        public IList<Activity> calcVote1Candidates(List<Emoji> emojis, Dictionary<int, int> emojiResults)
        {
            int topAnswerKey = 0;
            int topVotes = 0;
            int answerKey = 0;
            
            foreach (KeyValuePair<int, int> answer in emojiResults)
            {
                if (answer.Value > topVotes)
                {
                    topAnswerKey = answerKey;
                }
                answerKey++;
            }

            vote1Candidates.Add(topAnswerKey, emojis[topAnswerKey].Activities);
            Console.WriteLine($"Added candidates {topAnswerKey} {emojis[topAnswerKey].Activities}");
            // TODO: Check where to get this but it can be hard coded now
            vote1Timer = 30;
            return emojis[topAnswerKey].Activities;
        }

        //get vote1candidates
        public Dictionary<int, IList<Activity>> GetVote1Candidates() {
            return vote1Candidates;
		}
        //set vote1candidates
        public void SetVote1Candidates(Dictionary<int, IList<Activity>> candidates) {
            vote1Candidates = candidates;
		}

        public string calcFinalResult(Dictionary<Activity, int> vote1Results)
        {
            // TODO: Use the vote 1 results here
            //fallback, if nobody voted in phase 2
            /*
			if (vote2Results.Count == 0)
			{
                Console.WriteLine("We did not receive any votes in phase 2, default fallback = activity of the emoji with highest level of concern");
                //return vote2Candidates.ElementAt(0);
			}

            //creating empty dictionary sorted
            Dictionary<string, int> sorted = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> item in vote2Results.OrderByDescending(key => key.Value))
            {
                sorted.Add(item.Key, item.Value);
            }

            //final result is the top from sorted list of vote2results
            finalResult = sorted.Keys.ElementAt(0);
            */
            return finalResult;
        }

        public override string ToString()
        {
            string value = "";

            
            foreach (var item in vote1Candidates)
            {
                value += $"ID: {item.Key.ToString()}, ";
                value += "Activities: [";
                foreach (var activity in item.Value)
                {
                    value += $"{activity} ";
                }
                value += "]";
                value += "\n";
            }
            
            /* Can be removed?
            foreach(var item in vote2Candidates)
            {
                value += $"Activity: {item}";
                value += "\n";
            }*/
            

            value += finalResult;
            
            return value;
        }
    }
}
