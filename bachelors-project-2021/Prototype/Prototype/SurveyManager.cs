
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen
          2025 Eemil Alamäki

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
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Prototype
{
    public class SurveyManager
    {
        private static SurveyManager instance = null;
        private Survey survey;
        private string[] surveyTemplates;
        private readonly string folder;

        //Constructor for surveymanager that is going to be when surveymanager is used
        private SurveyManager() {
            survey = new Survey();
            folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        //Method for saving the survey with name of user's choosing
        public bool SaveSurvey(string name)
        {   
            string jsonString = JsonConvert.SerializeObject(survey);
            string path = Path.Combine(folder, name.ToLower());
            File.WriteAllText(path, jsonString);

            if (File.Exists(path) == true)
            {
                Console.WriteLine("File saving successful");
                return true;
            }
            else
            {
                Console.WriteLine("File saving failed");
                return false;
            }
        }

        //Method for loading the survey by selecting the file with name
        public Survey LoadSurvey(string name)
        {
            string path = Path.Combine(folder, name.ToLower());
            try
            {
                string jsontext = File.ReadAllText(path);
                survey = JsonConvert.DeserializeObject<Survey>(jsontext, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found");
            }
            return survey;
        }
        public void SetDefaultSurvey()
		{
            survey = Survey.GetDefaultSurvey();
		}
        
        //Method for getting the names of all the saved files in the folder
        public List<string> GetTemplates()
        {
            surveyTemplates = Directory.GetFiles(folder);
            List<string> surveyNames = new List<string>();

            foreach (string name in surveyTemplates)
            {
                string filename = Path.GetFileNameWithoutExtension(name);

                if (!string.IsNullOrEmpty(filename))
                {
                    surveyNames.Add(filename);
                    Console.WriteLine(filename);
                }
                else
                {
                    Console.WriteLine("Invalid or empty file name: " + name);
                }
            }

            return surveyNames;
        }

        //Method for deleting survey by using the name of existing survey
        public void DeleteSurvey(string name)
        {
            try
            {
                string path = Path.Combine(folder, name.ToLower());
                File.Delete(path);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found");
            }
        }

        //Method for getting the survey 
        public Survey GetSurvey()
        {
            return survey;
        }

        //Method for resetting the survey for starting with a blank survey
        public void ResetSurvey()
        {
            survey = new Survey();
        }

        //method for getting an instance of surveymanager so there isn't more than one surveymanager object
        public static SurveyManager GetInstance() {
            if (instance != null) {
                return instance;
			}
            instance = new SurveyManager();
            return instance;
		}
    }
}