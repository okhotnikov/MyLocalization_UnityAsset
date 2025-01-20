namespace MLoc.Data
{
    using System;
    using UnityEngine;

    [Serializable]
    public partial class MyLocalizationLink : IEquatable<MyLocalizationLink>
    {
        [SerializeField] private string _localizedTag;

        public string LocalizedTag => _localizedTag;

        public MyLocalizationLink(string localizedTag)
        {
            _localizedTag = localizedTag;
        }

        public bool Equals(MyLocalizationLink other)
        {
            if (other == null)
                return false;

            return _localizedTag == other._localizedTag;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MyLocalizationLink);
        }

        public override int GetHashCode()
        {
            return _localizedTag?.GetHashCode() ?? 0;
        }
    }
}
