using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;

namespace Grubbit
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }
        public List<Localization> translatedLanguages = new List<Localization>();
        public TextAsset localizationSheet;

        protected void Awake()
        {
            if (Instance != null) Destroy(this);
            Instance = this;
            DontDestroyOnLoad(this);
            GetLocalizationData();
        }

        /// <summary>
        /// Retrns a given key as it's translated version.
        /// </summary>
        public string GetTranslation(string key)
        {
            try
            {
                if ((LocalizationLanguage)GrubbitGlobals.CurrentLanguage == LocalizationLanguage.ENGLISH) return key;

                translatedLanguages[(int)(LocalizationLanguage)GrubbitGlobals.CurrentLanguage - 1].Translations.TryGetValue(key.ToLower().Trim(), out string value);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            catch (Exception e)
            {
                Debug.LogError("Could not translate correctly...\n" + e);
            }

            return key;
        }

        protected void GetLocalizationData()
        {
            var time = Time.time;
            // Create a Localization object for each language enum and add it to the desiredLanguages list
            try
            {
                for (var i = 0; i < Enum.GetNames(typeof(LocalizationLanguage)).Length; ++i)
                {
                    if ((LocalizationLanguage)i == LocalizationLanguage.ENGLISH) continue;

                    translatedLanguages.Add(new Localization()
                    {
                        language = (LocalizationLanguage)i,
                        columnNumber = i
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not correctly initialize the languages list...\n" + e);
                return;
            }

            // Split the file into lines by parsing for these line endings
            var lineSplitter = new string[] { "\r\n", "\r", "\n" };

            try
            {
                TextAsset sheet;

                // If the text asset has not been set in the inspector, set it here
                if (localizationSheet == null)
                    sheet = (TextAsset)Resources.Load("Localization_Sheet", typeof(TextAsset));
                else
                    sheet = localizationSheet;

                var fileData = sheet.text;

                // Split the file into lines
                var lines = fileData.Split(lineSplitter, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    Debug.LogWarning("The localization sheet could be read, but there is no relevant data inside...");
                    return;
                }

                var parsedLines = new List<string[]>();

                for (var i = 0; i < lines.Length; ++i)
                {
                    parsedLines.Add(Regex.Split(lines[i].Trim(), ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"));
                }

                var keyData = "";
                var valueData = "";

                // Iterate through each desired language, and create their dictionaries
                for (var i = 0; i < translatedLanguages.Count; ++i)
                {
                    if (translatedLanguages[i].language == LocalizationLanguage.ENGLISH) continue;

                    // Iterate through each line, parse it out and assign the correct key / value pair
                    for (var j = 1; j < parsedLines.Count; ++j)
                    {
                        keyData = "";
                        valueData = "";

                        // remove case sensitivity for the key
                        keyData = parsedLines[j][0].ToLower();
                        keyData = keyData.Replace(@"\", "").Replace("\"", "").Trim();

                        // Make sure that the column exists for this value
                        if (parsedLines.Count - 1 >= translatedLanguages[i].columnNumber)
                        {
                            valueData = parsedLines[j][translatedLanguages[i].columnNumber];
                            valueData = valueData.Replace(@"\", "").Replace("\"", "").Trim();
                        }

                        // Make sure the key and value are not empty, a comment, or are already in the dictionary
                        if (!string.IsNullOrWhiteSpace(keyData) && !string.IsNullOrWhiteSpace(valueData))
                            if (!translatedLanguages[i].Translations.ContainsKey(keyData) && !keyData.StartsWith("//"))
                                translatedLanguages[i].Translations.Add(keyData, valueData);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Found the localization sheet, but it's contents were invalid...\n" + e);
            }

            Debug.Log(string.Format("Translation dictionaries were created in {0} seconds.", Time.deltaTime - time));
        }
    }
}
