namespace MLoc.Data
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public partial class MyLocalizationDictionary
    {
        [SerializeField] private LanguageCode _langCode;

        [SerializeField] private List<MyTranslatePair> _translatePairs = new ();

        public LanguageCode LangCode => _langCode;
        public string LangCodeName => _langCode.ToString();

        public IReadOnlyList<MyTranslatePair> TranslatePairs => _translatePairs;

        public MyLocalizationDictionary(LanguageCode langCode)
        {
            _langCode = langCode;
        }
    }
}
