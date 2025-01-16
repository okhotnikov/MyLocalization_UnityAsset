namespace MLoc.Data
{
    using System;
    using UnityEngine;

    [Serializable]
    public partial class MyLocalizationTarget
    {
        [SerializeField, HideInInspector] private string _tagGuid;

        public string TagGuid
        {
            get => _tagGuid;
            set => _tagGuid = value;
        }
    }
}
