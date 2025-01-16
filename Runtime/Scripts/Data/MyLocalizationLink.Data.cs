namespace MLoc.Data
{
    using System;
    using UnityEngine;

    [Serializable]
    public partial class MyLocalizationLink
    {
        [SerializeField] private string _localizedTag;

        public string LocalizedTag => _localizedTag;

        public MyLocalizationLink(string localizedTag)
        {
            _localizedTag = localizedTag;
        }
    }
}
