namespace MLoc.Editor
{
    using MLoc.Data;
    using MLoc.Targets;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(BaseMyLocalizationTarget), true)]
    public class MyLocalizationTargetInspector : Editor
    {
        [SerializeField] VisualTreeAsset _mainInspectorAsset;

        private VisualElement _dataViewContainer;
        private Label _labelActiveDictionary;
        private PropertyField _dataField;

        private void OnEnable()
        {
            var cg = MyLocalization.Instance;
            if (cg == null)
                return;

            cg.OnDictionaryChanged += OnDictionaryChanged;
            cg.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            var cg = MyLocalization.Instance;
            if (cg == null)
                return;

            cg.OnDictionaryChanged -= OnDictionaryChanged;
            cg.OnLanguageChanged -= OnLanguageChanged;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = _mainInspectorAsset.Instantiate();

            _labelActiveDictionary = root.Query<Label>("activeDictionaryLabel").First();
            _labelActiveDictionary.text = $"Active dictionary: {Settings.GetSystemLanguage(MyLocalization.Instance.ActiveDictionaryLangCode).ToString()}";

            var targetProp = serializedObject.FindProperty("_target");
            if (targetProp != null)
            {
                var imageField = new IMGUIContainer(() =>
                {
                    EditorGUILayout.PropertyField(targetProp, true);
                    serializedObject.ApplyModifiedProperties();
                });

                root.Q<VisualElement>("targetViewContainer").Add(imageField);
            }

            _dataViewContainer = root.Q<VisualElement>("dataViewContainer");

            var dataProp = serializedObject.FindProperty("_data");
            if (dataProp != null)
            {
                _dataField = new PropertyField(dataProp);
                _dataField.Bind(serializedObject);
                _dataViewContainer.Add(_dataField);
            }

            return root;
        }

        private void OnDictionaryChanged(MyLocalizationDictionary dictionary)
        {
            UpdateData();
        }

        private void OnLanguageChanged(MyLocalizationDictionary dictionary)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            var dataProp = serializedObject.FindProperty("_data");
            if (dataProp != null)
            {
                _dataViewContainer.Remove(_dataField);

                _dataField = new PropertyField(dataProp);
                _dataField.Bind(serializedObject);
                _dataViewContainer.Add(_dataField);
            }
            Repaint();
            ResetTarget();
        }
    }
}
