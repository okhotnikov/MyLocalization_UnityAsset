namespace MLoc.Data
{
    using System;
    using UnityEngine;

    [Serializable]
    public partial class MyTranslatePair : IEquatable<MyTranslatePair>
    {
        [SerializeField] private string _localizedTag;
        [SerializeField] private string _localizedText;

        public string LocalizedTag => _localizedTag;

        public string LocalizedText
        {
            get => _localizedText;
            set => _localizedText = value;
        }

        public MyTranslatePair(string localizedTag, string localizedText)
        {
            _localizedTag = localizedTag;
            _localizedText = localizedText;
        }

        public override string ToString() => $"Tag_{_localizedTag}_{_localizedText}";

        public override bool Equals(object obj) => obj is MyTranslatePair data && _localizedTag == data._localizedTag && _localizedText == data._localizedText;

        public bool Equals(MyTranslatePair other)
        {
            if (other == null)
                return false;

            return _localizedTag == other._localizedTag && _localizedText == other._localizedText;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_localizedTag?.GetHashCode(), _localizedText?.GetHashCode());
        }
    }
}
