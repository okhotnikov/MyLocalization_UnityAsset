namespace MLoc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using MLoc.Data;
    using MLoc.Utils;
    using Newtonsoft.Json.Linq;
    using UnityEngine;

    public partial class MyLocalization
    {
        public static MyLocalization Load()
        {
            try
            {
                var dataJson = LoadDataJson();
                if (!string.IsNullOrEmpty(dataJson))
                {
                    var parserData = JsonUtility.FromJson<MyLocalization>(dataJson);
                    if (parserData != null)
                    {
                        var config = CreateEmptyLocalization();
                        config = parserData;
                        config.SetLoaded();
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Settings.DataFileName} is corrupted. Falling back to default configuration.");
                Debug.LogException(ex);
                return null;
            }

            return null;
        }

        public void Sync()
        {
            var dictionaryJObject = LoadDictionaryJson();
            if (dictionaryJObject == null)
            {
                Debug.LogWarning("Dictionary is null.");
                return;
            }

            var languageCodeCount = Enum.GetValues(typeof(LanguageCode)).Length;
            var countOfSupportedLanguages = Settings.CountSupportedLanguages;
            if (languageCodeCount != countOfSupportedLanguages)
            {
                Debug.LogWarning($"Count of supported languages in settings ({countOfSupportedLanguages}) doesn't match the count of LanguageCode enum values ({languageCodeCount}).");
                return;
            }

            var names = ReadLanguageNamesInDictionary(dictionaryJObject);
            if (names.Count > countOfSupportedLanguages)
            {
                Debug.LogWarning($"Count of language names in dictionary ({names.Count}) exceeds the count of supported languages ({countOfSupportedLanguages}).");
                return;
            }

            ApplyLangCodes(names);
            ApplyDictionary(dictionaryJObject);
        }

        public void ForceReload()
        {
            _instance = null;
            _instance = Load();
            Sync();
        }

        public void Save()
        {
#if UNITY_EDITOR
            try
            {
                ResolveCollisions();
                FileUtils.EnsureDirectoryExists(Settings.DataAssetsFilePath);
                JsonUtils.WriteJson(Settings.DataAssetsFilePath, JsonUtility.ToJson(this, true));
                UnityEditor.EditorUtility.SetDirty(Settings.DataAssetFile);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save {Settings.DataFileName} at {Settings.DataAssetsFilePath}");
                Debug.LogException(ex);
            }
#else
            Debug.LogWarning("Save is only available in the Unity Editor.");
#endif
        }

        public static string Get(int key)
        {
            return key.ToString();
        }

        public static string Get(string key)
        {
            return Instance.GetLocalized(key);
        }

        public void NotifyDictionaryChanged(MyLocalizationDictionary dictionary)
        {
            try
            {
                OnDictionaryChanged?.Invoke(dictionary);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void ApplyDictionary(JObject dictionaryJObject)
        {
            var activeLangCode = ActiveDictionary.LangCode;
            if (activeLangCode == LanguageCode.xx)
            {
                activeLangCode = Settings.DefaultLanguageCode;
            }

            var codeStr = activeLangCode.ToString();
            var isEnglish = activeLangCode == LanguageCode.en;

            _dictionary = new MyLocalizationDictionary(activeLangCode);
            foreach (var token in dictionaryJObject)
            {
                var translationKey = token.Key;
                var jValue = (JObject)token.Value;

                var translation = GetTranslationValueRecursive(codeStr, translationKey, jValue, isEnglish, dictionaryJObject);
                var pair = new MyTranslatePair(translationKey, translation);
                _dictionary.AddTranslatePair(pair);

                var link = new MyLocalizationLink(translationKey);
                if (!_localizationLinks.Contains(link))
                    _localizationLinks.Add(link);
            }

            NotifyAndRemoveDuplicates();

            _isSynced = true;
        }

        public void ApplyLangCodes(Dictionary<LanguageCode, string> languageNames)
        {
            if (languageNames == null)
            {
                Debug.LogWarning("Language names are null.");
                return;
            }

            foreach (var langCode in languageNames.Keys)
            {
                if (!_dictionaryLangCodes.Contains(langCode))
                {
                    _dictionaryLangCodes.Add(langCode);
                }
            }
        }

        private void NotifyAndRemoveDuplicates()
        {
            var duplicates = _localizationLinks
                .GroupBy(link => link)
                .Where(group => group.Count() > 1)
                .Select(group => new { Link = group.Key, Count = group.Count() })
                .ToList();

            if (duplicates.Any())
            {
                Console.WriteLine("Duplicates found:");

                foreach (var duplicate in duplicates)
                {
                    Console.WriteLine($"Tag: {duplicate.Link.LocalizedTag}, Count: {duplicate.Count}");
                }

                _localizationLinks = _localizationLinks.Distinct().ToList();
                Console.WriteLine("Duplicates removed.");
            }
            else
            {
                Console.WriteLine("Duplicates not found.");
            }
        }

        private string GetTranslationValueRecursive(string langCode, string key, JObject jvalue, bool isEnglish, JObject dict)
        {
            var enValue = GetTranslationValueExact(jvalue, Settings.EnglishCodeString);
            if (enValue.StartsWith("$"))
            {
                var nestedKey = enValue.Substring(1);
                var nestedValue = (JObject)dict[nestedKey];
                return GetTranslationValueRecursive(langCode, nestedKey, nestedValue, isEnglish, dict);
            }

            var stringValue = isEnglish ? enValue : GetTranslationValueExact(jvalue, langCode);
            if (!string.IsNullOrEmpty(stringValue))
                return stringValue;

            Debug.LogWarning($"Missing {langCode} translation, key <{key}>");
            return $"#{key}#";
        }

        private string GetTranslationValueExact(JObject valueObject, string code)
        {
            var translationToken = valueObject[code];
            return (string)((JValue)translationToken).Value;
        }

        private static string LoadDataJson()
        {
            return FileUtils.LoadResourceAsString(Settings.DataFileName, Settings.DataAssetsFilePath);
        }

        public static JObject LoadDictionaryJson()
        {
            var dictionaryBytes = FileUtils.LoadResourceAsBytes(Settings.DictionaryFileName, Settings.DictionaryAssetsFilePath);
            if (dictionaryBytes == null)
            {
                Debug.LogWarning($"{Settings.DictionaryFileName} not found or empty.");
                return null;
            }

            try
            {
                return JsonUtils.GetJsonFromBytes(dictionaryBytes, false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Settings.DictionaryFileName} is corrupted. Falling back to default dictionary.");
                Debug.LogException(ex);
                return null;
            }
        }

        public static Dictionary<string, Dictionary<string, string>> ParseJObject(JObject jObject)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var token in jObject)
            {
                var translationKey = token.Key;
                var jValue = (JObject)token.Value;

                foreach (var lang in jValue)
                {
                    if (!dict.ContainsKey(lang.Key))
                        dict[lang.Key] = new Dictionary<string, string>();

                    dict[lang.Key][translationKey] = lang.Value.ToString();
                }
            }

            return dict;
        }

        public static Dictionary<LanguageCode, string> ReadLanguageNamesInDictionary(JObject jobject)
        {
            Dictionary<LanguageCode, string> languageNames = new();
            var token = jobject["language.name"];
            if (token == null)
            {
                Debug.LogWarning("No language names found in dictionary.");
                return languageNames;
            }

            var dict = (IDictionary<string, JToken>)token;
            var keys = new List<string>(dict.Keys);
            keys.Sort();

            foreach (var key in keys)
            {
                if (!Enum.TryParse(key, out LanguageCode langCode))
                    continue;

                var value = dict[key];
                if (value.Type == JTokenType.String)
                {
                    languageNames[langCode] = value.ToString();
                }
            }

            return languageNames;
        }

        private static MyLocalization CreateEmptyLocalization()
        {
            var config = new MyLocalization();
            config.InitByDefault();
            return config;
        }

        public void InitByDefault()
        {
            _isSynced = false;
            _isLoaded = false;
        }

        public void SetLoaded()
        {
            _isLoaded = true;
        }

        public void ResolveCollisions()
        {
            var changed = false;
            if (_localizationLinks == null)
            {
                _localizationLinks = new List<MyLocalizationLink>();
                changed = true;
            }

            if (_dictionaryLangCodes == null)
            {
                _dictionaryLangCodes = new List<LanguageCode>();
                changed = true;
            }

            if (_dictionary == null)
            {
                _dictionary = new MyLocalizationDictionary(LanguageCode.xx);
                changed = true;
            }

            if (_dictionaryLangCodes.Count == 0)
            {
                TryNotify();
                return;
            }

            // if (!MyLocalizationService.IsAllPalettesHasGuid(_palettes))
            // {
            //     ColorPaletteService.UpdatePalettesGuid(_palettes);
            //     changed = true;
            // }

            // foreach (var paletteData in _palettes)
            // {
            //     if (paletteData.HasUnUsedColors(_colorLinks, out var guids))
            //     {
            //         paletteData.RemoveColors(guids);
            //         changed = true;
            //     }
            //
            //     if (paletteData.HasMissedColors(_colorLinks))
            //     {
            //         paletteData.FixMissedColors(_colorLinks);
            //         changed = true;
            //     }
            // }

            TryNotify();

            return;

            void TryNotify()
            {
                if (changed && ActiveDictionary != null)
                    NotifyDictionaryChanged(ActiveDictionary);
            }
        }

        public bool TryGet(string key, out string result)
        {
            var text = _dictionary?.TranslatePairs.FirstOrDefault(a => a.LocalizedTag.Equals(key));
            if (text != null)
            {
                result = text.LocalizedText;
                return true;
            }

            result = null;
            return false;
        }

        public string GetLocalized(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (TryGet(key, out var value))
                return value;

#if UNITY_EDITOR
            var disableLocalizationWarnings = false;
            disableLocalizationWarnings = UnityEditor.EditorPrefs.GetBool("DisableLocalizationWarnings_v2", true);
            if (!disableLocalizationWarnings)
                Debug.LogWarning($"translation not found for key: {key}");
#endif

            return $"#{key}#";
        }

        public string GetLocalizedFormat(string format, params object[] args)
        {
            return string.Format(GetLocalized(format), args);
        }

        private static readonly Dictionary<string, Func<double, double, double>> Operations = new()
        {
            { "+", (x, y) => x + y },
            { "-", (x, y) => x - y },
            { "*", (x, y) => x * y },
            { "/", (x, y) => x / y },
            { "%", (x, y) => (x / y) * 100 }
        };

        public string ReplaceNumberParams(string content, string returnFormat, params (string key, object value)[] args)
        {
            double ToDouble(object val)
            {
                return val switch
                {
                    int i => Convert.ToDouble(i),
                    double d => d,
                    _ => throw new InvalidCastException($"format {content}, invalid value: {val}")
                };
            }

            var builder = new StringBuilder(content);
            foreach (var arg in args)
                builder.Replace("{" + arg.key + "}", string.Format(returnFormat, arg.value));

            var expressionStr = builder.ToString();
            if (!Regex.IsMatch(expressionStr, @"\{[^{}]*\}")) // Contains open and closed { }
                return expressionStr;

            return Regex.Replace(expressionStr, @"\{([^{}]*)\}", match =>
            {
                var expression = match.Groups[1].Value;
                var parts = expression.Split(' ');

                if (parts.Length != 3)
                    throw new ArgumentException($"Incorrect expression: {expression}");

                if (!Operations.TryGetValue(parts[2], out var operation))
                    throw new ArgumentException($"Incorrect operation: {parts[2]}");

                var o1 = ParseOperand(parts[0], args);
                var o2 = ParseOperand(parts[1], args);
                return string.Format(returnFormat, operation(ToDouble(o1), ToDouble(o2)).ToString(CultureInfo.InvariantCulture));
            });
        }

        private object ParseOperand(string operand, params (string key, object value)[] args)
        {
            if (operand.EndsWith("%"))
                operand = operand.Remove(operand.Length - 2);

            if (double.TryParse(operand, out var constant))
                return constant;

            foreach (var arg in args)
                if (operand.Equals(arg.key))
                    return arg.value;

            throw new ArgumentException($"Localization: incorrect operand {operand}");
        }
    }
}
