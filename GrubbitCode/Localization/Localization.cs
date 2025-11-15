using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grubbit
{
    public enum LocalizationLanguage
    {
        ENGLISH = 0,
        SPANISH = 1,
        RUSSIAN = 2,
        KOREAN = 3
    }

    public class Localization
    {
        public LocalizationLanguage language = LocalizationLanguage.ENGLISH;
        public int columnNumber;
        public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();

        /*
        * The only reason this exists here is because it makes more logical sense to outside programmers 
        * (and it's cleaner) to just call Translation.GetTranslation as opposed
        * to LocalizationManager.Instance.GetTranslation (which also works)
        */

        /// <summary>
        /// Retrns a given key as it's translated version.
        /// </summary>
        public static string GetTranslation(string key)
        {
            var translated = "";
            try
            {
                translated = LocalizationManager.Instance.GetTranslation(key);
                if (!string.IsNullOrWhiteSpace(translated))
                    return translated;
            }
            catch (Exception e)
            {
                Debug.LogError("The Localization Manager is not initialized, returning given key...\n" + e);
            }

            return key;
        }
    }
}
