namespace MLoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MLoc.Data;
    using UnityEngine;

    [Serializable]
    public partial class MyLocalization
    {
        private static readonly Dictionary<string, Func<double, double, double>> Operations = new()
        {
            { "+", (x, y) => x + y },
            { "-", (x, y) => x - y },
            { "*", (x, y) => x * y },
            { "/", (x, y) => x / y },
            { "%", (x, y) => (x / y) * 100 }
        };

        [SerializeField] private List<MyLocalizationLink> _localizationLinks = new ();
        [SerializeField] private List<LanguageCode> _dictionaryLangCodes = new ();
        [SerializeField] private MyLocalizationDictionary _dictionary = new (LanguageCode.xx);

        [SerializeField] private string _credentialFilePath;
        [SerializeField] private bool _expandExternalSettings;
        [SerializeField] private bool _logsEnabled;
        [SerializeField] private bool _inverseSorting;

        private LanguageCode _runtimeLangCode = LanguageCode.xx;
        private Dictionary<string, string> _runtimeDictionary = new ();

        private bool _isLoaded = false;
        private bool _isSynced = false;

        public event Action<MyLocalizationDictionary> OnDictionaryChanged;
        public event Action<MyLocalizationDictionary> OnLanguageChanged;

        private static MyLocalization _instance;

        public static MyLocalization Instance
        {
            get
            {
                _instance ??= CreateEmptyLocalization();

                if (!_instance.IsLoaded)
                {
                    var loc = Load();
                    if (loc != null)
                    {
                        _instance = loc;
                    }
                }

                if (_instance.IsLoaded && !_instance.IsSynced)
                {
                    _instance.Sync();

                    if (Application.isPlaying)
                    {
                        _instance.ConvertToRuntimeMode();
                    }
                }

                if (_instance == null)
                {
                    Debug.LogWarning("Localization instance is null");
                }

                return _instance;
            }
        }

        public bool IsLoaded => _isLoaded;
        public bool IsSynced => _isSynced;

        public LanguageCode ActiveDictionaryLangCode
        {
            get => ActiveDictionary?.LangCode ?? LanguageCode.xx;
        //     set => ActiveDictionaryIndex = _dictionaryLangCodes.FindIndex(p => p == value);
        }

        public MyLocalizationDictionary ActiveDictionary => _dictionary;

        public string CredentialFilePath
        {
            get => _credentialFilePath;
            set => _credentialFilePath = value;
        }

        public bool ExpandExternalSettings
        {
            get => _expandExternalSettings;
            set => _expandExternalSettings = value;
        }

        public bool LogsEnabled
        {
            get => _logsEnabled;
            set => _logsEnabled = value;
        }

        public MyLocalizationDictionary Dictionary => _dictionary;

        public IEnumerable<LanguageCode> DictionaryLangCodes => _dictionaryLangCodes ?? new List<LanguageCode>();

        public IEnumerable<string> DictionaryNames => _dictionaryLangCodes == null ? new List<string>() : _dictionaryLangCodes.Select(p => p.ToString());

        public IEnumerable<string> DictionarySystemNames => _dictionaryLangCodes == null ? new List<string>() : _dictionaryLangCodes.Select(p => Settings.GetSystemLanguage(p).ToString());

        public struct ReplaceNumberParamsArg
        {
            public string key;
            public object value;
            public string customFormat;
            public object result;
        }
    }
}
