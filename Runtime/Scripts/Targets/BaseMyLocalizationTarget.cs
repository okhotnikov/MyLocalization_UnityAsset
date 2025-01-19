namespace MLoc.Targets
{
    using System;
    using MLoc.Data;
    using UnityEngine;

    [Serializable, ExecuteAlways, ExecuteInEditMode]
    public abstract class BaseMyLocalizationTarget : MonoBehaviour
    {
        [SerializeField] protected MyLocalizationTarget _data = new();
        protected virtual UnityEngine.Object Target { get; } = null;

        protected MyLocalizationDictionary ActiveDictionary => MyLocalization.Instance.ActiveDictionary;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            TryUpdateLocalization(out _);
            SetDirty();
#endif
        }

        protected virtual void Start()
        {
#if UNITY_EDITOR
            TryUpdateLocalization(out _);
#endif
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                EnableInternal();
            }
            else
            {
                UnityEditor.EditorApplication.delayCall += EnableInternal;
            }
#else
            EnableInternal();
#endif
        }

        protected virtual void OnDisable()
        {
            DisableInternal();
        }

        protected virtual void EnableInternal()
        {
            TryUpdateLocalization(out _);
            SetDirty();
            RecordPrefabInstancePropertyModifications();

            MyLocalization.Instance.OnDictionaryChanged += UpdateLocalization;
            MyLocalization.Instance.OnLanguageChanged += UpdateLocalization;
        }

        protected virtual void DisableInternal()
        {
            MyLocalization.Instance.OnDictionaryChanged -= UpdateLocalization;
            MyLocalization.Instance.OnLanguageChanged -= UpdateLocalization;
        }

        protected virtual void UpdateLocalization(MyLocalizationDictionary dictionary)
        {
            TryUpdateLocalization(out _);
        }

        protected virtual bool TryUpdateLocalization(out string newLocalizedText)
        {
            newLocalizedText = string.Empty;

            var dictionary = ActiveDictionary;
            if (dictionary == null)
            {
                Debug.LogError("Main dictionary is null", this);
                return false;
            }

            if (string.IsNullOrEmpty(_data.TagGuid))
            {
                return false;
            }

            var tag = dictionary.GetTagByGuid(_data.TagGuid);
            if (tag == null)
            {
                Debug.LogError($"Localized text with GUID='{_data.TagGuid}' not found in database", this);
                return false;
            }

            newLocalizedText = tag.LocalizedText;
            SetLocalizedText(newLocalizedText);

            return true;
        }

        protected abstract string GetBaseText();

        protected abstract void SetLocalizedText(string value);

        private void SetDirty()
        {
            if (Application.isPlaying)
                return;

            SetDirty(this);
            SetDirty(Target);
        }

        private void RecordPrefabInstancePropertyModifications()
        {
            if (Application.isPlaying)
                return;

            RecordPrefabInstancePropertyModifications(this);
            RecordPrefabInstancePropertyModifications(Target);
        }

        private void SetDirty(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }

        private void RecordPrefabInstancePropertyModifications(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
#endif
        }
    }
}
