namespace MLoc
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    public static class Settings
    {
        public static readonly string EnglishCodeString = LanguageCode.en.ToString();

        public static LanguageCode DefaultLanguageCode => GetDefaultLanguage();

        public static string DataFileName => "MyLocalizationData";
        public static string DictionaryFileName => "MyLocalizationDictionary";
        public static string AssetsPath => Path.Combine("Assets", "Resources");
        public static string DataAssetsFilePath => Path.Combine(AssetsPath, $"{DataFileName}.json");
        public static string DictionaryAssetsFilePath => Path.Combine(AssetsPath, $"{DictionaryFileName}.json");
        public static string BaseCredentialFilePath = Path.Combine("Resources", "credentials.json");

        public static int CountSupportedLanguages => SystemLanguageToCode.Count;

#if UNITY_EDITOR
        public static TextAsset DataAssetFile => UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(DataAssetsFilePath);
        public static TextAsset DictionaryAssetFile => UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(DictionaryAssetsFilePath);
#endif

        private static readonly Dictionary<SystemLanguage, LanguageCode> SystemLanguageToCode = new()
        {
            { SystemLanguage.Afrikaans,        LanguageCode.af },
            { SystemLanguage.Arabic,           LanguageCode.ar },
            { SystemLanguage.Basque,           LanguageCode.eu },
            { SystemLanguage.Belarusian,       LanguageCode.be },
            { SystemLanguage.Bulgarian,        LanguageCode.bg },
            { SystemLanguage.Catalan,          LanguageCode.ca },
            { SystemLanguage.Chinese,          LanguageCode.zh },
            { SystemLanguage.Czech,            LanguageCode.cs },
            { SystemLanguage.Danish,           LanguageCode.da },
            { SystemLanguage.Dutch,            LanguageCode.nl },
            { SystemLanguage.English,          LanguageCode.en },
            { SystemLanguage.Estonian,         LanguageCode.et },
            { SystemLanguage.Faroese,          LanguageCode.fo },
            { SystemLanguage.Finnish,          LanguageCode.fi },
            { SystemLanguage.French,           LanguageCode.fr },
            { SystemLanguage.German,           LanguageCode.de },
            { SystemLanguage.Greek,            LanguageCode.el },
            { SystemLanguage.Hebrew,           LanguageCode.he },
            { SystemLanguage.Hungarian,        LanguageCode.hu },
            { SystemLanguage.Icelandic,        LanguageCode.iс },
            { SystemLanguage.Indonesian,       LanguageCode.id },
            { SystemLanguage.Italian,          LanguageCode.it },
            { SystemLanguage.Japanese,         LanguageCode.ja },
            { SystemLanguage.Korean,           LanguageCode.ko },
            { SystemLanguage.Latvian,          LanguageCode.lv },
            { SystemLanguage.Lithuanian,       LanguageCode.lt },
            { SystemLanguage.Norwegian,        LanguageCode.no },
            { SystemLanguage.Polish,           LanguageCode.pl },
            { SystemLanguage.Portuguese,       LanguageCode.pt },
            { SystemLanguage.Romanian,         LanguageCode.ro },
            { SystemLanguage.Russian,          LanguageCode.ru },
            { SystemLanguage.SerboCroatian,    LanguageCode.sr },
            { SystemLanguage.Slovak,           LanguageCode.sk },
            { SystemLanguage.Slovenian,        LanguageCode.sl },
            { SystemLanguage.Spanish,          LanguageCode.es },
            { SystemLanguage.Swedish,          LanguageCode.sv },
            { SystemLanguage.Thai,             LanguageCode.th },
            { SystemLanguage.Turkish,          LanguageCode.tr },
            { SystemLanguage.Ukrainian,        LanguageCode.uk },
            { SystemLanguage.Vietnamese,       LanguageCode.vi },
            { SystemLanguage.ChineseSimplified, LanguageCode.zh_Hans },
            { SystemLanguage.ChineseTraditional, LanguageCode.zh_Hant },
            { SystemLanguage.Hindi,            LanguageCode.hi },
            { SystemLanguage.Unknown,          LanguageCode.xx }
        };

        public static LanguageCode GetLanguageCode(SystemLanguage language)
        {
            return SystemLanguageToCode.TryGetValue(language, out var code) ? code : DefaultLanguageCode;
        }

        public static SystemLanguage GetSystemLanguage(LanguageCode code)
        {
            foreach (var pair in SystemLanguageToCode.Where(pair => pair.Value == code))
                return pair.Key;

            return SystemLanguage.Unknown;
        }

        private static LanguageCode GetDefaultLanguage()
        {
#if UNITY_EDITOR
            return LanguageCode.en;
#else
            var lang = Application.systemLanguage;
            if (!SystemLanguageToCode.TryGetValue(lang, out var code))
                code = LanguageCode.en;

            return code;
#endif
        }
    }
}
