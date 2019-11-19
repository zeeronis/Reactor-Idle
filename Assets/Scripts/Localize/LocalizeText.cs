using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizeText : MonoBehaviour
{
    private const string LOCALIZATION_FOLDER = "localization/";

    private static SystemLanguage currentLanguage;
    private static bool currentLanguageHasBeenSet = false;
    public static SystemLanguage[] supportedLangs = new SystemLanguage[] 
    {
        SystemLanguage.English, SystemLanguage.Russian
    };
    private static List<LocalizeText> localizeTexts = new List<LocalizeText>();
    public static Dictionary<string, string> CurrentLanguageStrings = new Dictionary<string, string>();
    private static TextAsset currentLocalizationText;

    public static void SetCurrentLocalization(SystemLanguage language)
    {
        if (currentLanguage == language) return;
        currentLanguage = language;
        currentLocalizationText = Resources.Load(LOCALIZATION_FOLDER + language.ToString(), typeof(TextAsset)) as TextAsset;
        if(currentLocalizationText == null)
        {
            Debug.LogFormat("Missing locale {0}", language.ToString());
            SetCurrentLocalization(SystemLanguage.English);
        }
        else
        {
            string[] lines = currentLocalizationText.text.Split(new string[] { "\r\n", "\n\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            CurrentLanguageStrings.Clear();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] pairs = lines[i].Split(new char[] { '\t', '=' }, 2);
                if(pairs.Length == 2)
                {
                    CurrentLanguageStrings.Add(pairs[0].Trim(), pairs[1].Trim());
                }
            }

            LocalizeText[] texts = localizeTexts.ToArray();
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].UpdateLocale();
            }   
        }
    }

    public string localizationKey;
    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
        localizeTexts.Add(this);
        if (!currentLanguageHasBeenSet)
        {
            currentLanguageHasBeenSet = true;
            SetCurrentLocalization(SystemLanguage.English);
        }
        UpdateLocale();
    }

    public void UpdateLocale()
    {
        if (!text) return;
        if (CurrentLanguageStrings.ContainsKey(localizationKey))
        {
            text.text = CurrentLanguageStrings[localizationKey].Replace(@"\n", "" + '\n');
        }
    }
}
