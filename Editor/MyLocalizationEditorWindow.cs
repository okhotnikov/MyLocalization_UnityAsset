namespace MLoc.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MLoc.Editor.Data;
    using MLoc.Services;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class MyLocalizationEditorWindow : EditorWindow
    {
        private readonly string _uiName_settingsViewContainer = "settingsViewContainer";
        private readonly string _uiName_dictionaryTabsContainer = "dictionaryTabsContainer";
        private readonly string _uiName_dropdownDictionarySelector = "dropdownActiveDictionary";
        private readonly string _uiName_externalSettingsFoldout = "externalSettingsFoldout";
        private readonly string _uiName_externalSettingsFoldoutView = "externalSettingsFoldoutView";

        [SerializeField] VisualTreeAsset _mainViewContainerAsset;
        [SerializeField] VisualTreeAsset _settingsContainerAsset;
        [SerializeField] VisualTreeAsset _shortSettingsContainerAsset;
        [SerializeField] VisualTreeAsset _tabDictionaryViewAsset;
        [SerializeField] VisualTreeAsset _localizationTagViewAsset;

        private VisualElement _settingsPanel;

        private DropdownField _ddDictionarySelector;
        private Foldout _externalSettingsFoldout;
        private VisualElement _externalSettingsFoldoutView;

        private Dictionary<string, MyLocalizationDictionaryEditorData> _editorDictionariesData = new ();
        private Dictionary<LanguageCode, Dictionary<string, string>> _editorDictionariesDataCache = new ();

        private bool _logsEnabled;
        private Vector2 _previousSize;

        public void Init() => CreateGUI();

        private void OnValidate()
        {
            if (!MyLocalization.Instance.IsSynced)
                return;

            GetWindow<MyLocalizationEditorWindow>().Init();

            MyLocalization.Instance.ResolveCollisions();
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            var localization = MyLocalization.Instance;
            var mainContainer = _mainViewContainerAsset.Instantiate();

            var baseScroll = new ScrollView();
            baseScroll.Add(mainContainer);

            rootVisualElement.Add(baseScroll);

            if (localization is not { IsSynced: true })
            {
                CreateShortSettingsGUI(localization, mainContainer);
                Debug.LogWarning("Localization is not loaded");
                return;
            }

            var dictionaryJObject = MyLocalization.LoadDictionaryJson();
            if (dictionaryJObject == null)
            {
                CreateShortSettingsGUI(localization, mainContainer);
                Debug.LogWarning("Dictionary is not loaded");
                return;
            }

            var rawDicts = MyLocalization.ParseJObject(dictionaryJObject);
            _editorDictionariesDataCache = new Dictionary<LanguageCode, Dictionary<string, string>>();
            foreach (var (langCodeStr, dictionary) in rawDicts)
            {
                if (Enum.TryParse(langCodeStr, out LanguageCode langCode))
                    _editorDictionariesDataCache.Add(langCode, dictionary);
            }

            CreateSettingsGUI(localization, mainContainer);
            UpdateDictionarySelector(localization);
            CreateDictionaryTabsGUI(localization, mainContainer);
        }

        private void CreateShortSettingsGUI(MyLocalization localization, TemplateContainer panel)
        {
            _settingsPanel = _shortSettingsContainerAsset.Instantiate();
            var settingsContainer = panel.Query<VisualElement>(_uiName_settingsViewContainer).First();
            settingsContainer.Add(_settingsPanel);

            CreateCommonSettingsGUI(localization, panel);
        }

        private void CreateSettingsGUI(MyLocalization localization, TemplateContainer panel)
        {
            _settingsPanel = _settingsContainerAsset.Instantiate();
            var settingsContainer = panel.Query<VisualElement>(_uiName_settingsViewContainer).First();
            settingsContainer.Add(_settingsPanel);

            _ddDictionarySelector = _settingsPanel.Query<DropdownField>(_uiName_dropdownDictionarySelector).First();
            _ddDictionarySelector.RegisterValueChangedCallback(chEvent =>
            {
                // MyLocalization.Instance.ApplyLangCodes();
                var msg = $"Dictionary Changed: {chEvent.newValue}";
                SaveChanges(msg);
            });

            _externalSettingsFoldout = _settingsPanel.Query<Foldout>(_uiName_externalSettingsFoldout).First();
            _externalSettingsFoldoutView = _settingsPanel.Query<VisualElement>(_uiName_externalSettingsFoldoutView).First();
            _externalSettingsFoldout.value = localization.ExpandExternalSettings;
            _externalSettingsFoldout.RegisterValueChangedCallback(chEvent =>
            {
                var newValue = chEvent.newValue;
                if (localization.ExpandExternalSettings == newValue)
                    return;

                localization.ExpandExternalSettings = chEvent.newValue;

                var msg = $"Settings foldout state change: {chEvent.newValue}";
                SaveChanges(msg);
            });

            CreateCommonSettingsGUI(localization, panel);
        }

        private void CreateCommonSettingsGUI(MyLocalization localization, TemplateContainer panel)
        {
            var credentialTextFields = _settingsPanel.Query<TextField>("credentialFilePath").First();
            var credentialsPath = string.Empty;

            if (MyLocalization.Instance.IsLoaded)
            {
                credentialsPath = localization.CredentialFilePath;
            }
            else
            {
                credentialsPath = Settings.BaseCredentialFilePath;
                var msg = $"Credentials path changed: {MyLocalization.Instance.CredentialFilePath}";
                SaveChanges(msg);
            }

            credentialTextFields.value = credentialsPath;
            credentialTextFields.RegisterValueChangedCallback(chEvent =>
            {
                MyLocalization.Instance.CredentialFilePath = chEvent.newValue;

                var msg = $"Credentials path changed: {MyLocalization.Instance.CredentialFilePath}";
                SaveChanges(msg);
            });

            var refreshBtn = _settingsPanel.Query<Button>("refreshButton").First();
            refreshBtn.RegisterCallback<ClickEvent>(_ =>
            {
                var downloader = new GoogleSheetsDownloader();
                var output = downloader.DownloadSheet();
                var parser = new GoogleSheetParser();
                parser.ParseAndSave(output);

                MyLocalization.Instance.ForceReload();
                GetWindow<MyLocalizationEditorWindow>().Init();
                MyLocalization.Instance.ResolveCollisions();
            });

            var logsEnableToggle = _settingsPanel.Query<Toggle>("logsEnableToggle").First();
            logsEnableToggle.value = _logsEnabled = localization.LogsEnabled;
            logsEnableToggle.RegisterValueChangedCallback(chEvent =>
            {
                localization.LogsEnabled = _logsEnabled = chEvent.newValue;

                var msg = $"LogsEnabled state change: {localization.LogsEnabled}";
                SaveChanges(msg);
            });
        }

        private void CreateDictionaryTabsGUI(MyLocalization localization, TemplateContainer container)
        {
            foreach (var langCode in localization.DictionaryLangCodes)
                AddDictionaryInGUI(container, langCode);
        }

        private void AddDictionaryInGUI(TemplateContainer container, LanguageCode langCode)
        {
            var dictionaryTabsContainer = container.Query<TabView>(_uiName_dictionaryTabsContainer).First();
            var tabDictionaryView = _tabDictionaryViewAsset.Instantiate();

            var newTab = new Tab();
            newTab.Add(tabDictionaryView);
            newTab.name = $"tab_{langCode}";
            newTab.label = Settings.GetSystemLanguage(langCode).ToString();
            dictionaryTabsContainer.Add(newTab);

            var dictionaryDataEditor = _editorDictionariesData[langCode.ToString()] = InitTabComponents(tabDictionaryView, langCode);
            foreach (var translatePairs in _editorDictionariesDataCache[langCode])
                AddTranslatePairInGUI(dictionaryDataEditor, (translatePairs.Key, translatePairs.Value));

            if (MyLocalization.Instance.ActiveDictionary.LangCode == langCode)
                dictionaryTabsContainer.activeTab = newTab;
        }

        private MyLocalizationDictionaryEditorData InitTabComponents(TemplateContainer tabDictionaryView, LanguageCode langCode)
        {
            var sortBtn = tabDictionaryView.Query<Button>("sortButton").First();
            sortBtn.RegisterCallback<ClickEvent>(_ =>
            {
                _editorDictionariesDataCache[langCode] = new Dictionary<string, string>(_editorDictionariesDataCache[langCode].OrderBy(p => p.Key));

                foreach (var editorData in _editorDictionariesData.Values)
                {
                    var pairs = editorData.TranslatePairsData.Values.ToList();
                    foreach (var pair in pairs)
                        pair.PanelView.RemoveFromHierarchy();

                    foreach (var translatePairs in _editorDictionariesDataCache[langCode])
                        AddTranslatePairInGUI(editorData, (translatePairs.Key, translatePairs.Value));
                }

                var msg = $"Tags sorted";
                SaveChanges(msg);
            });

            var dictionaryDataEditor = new MyLocalizationDictionaryEditorData
            {
                TranslatePairsViewContainer = tabDictionaryView.Query<VisualElement>("translatePairsViewContainer").First(),
                TranslatePairsData = new Dictionary<string, MyLocalizationTranslatePairsEditorData>()
            };

            return dictionaryDataEditor;
        }

        private void AddTranslatePairInGUI(MyLocalizationDictionaryEditorData dictionaryDataEditor, (string key, string value) translatePair)
        {
            var localizationTagView = _localizationTagViewAsset.Instantiate();
            dictionaryDataEditor.TranslatePairsViewContainer.Add(localizationTagView);
            dictionaryDataEditor.TranslatePairsData[translatePair.key] = InitTranslatePairComponent(localizationTagView, translatePair);
        }

        private MyLocalizationTranslatePairsEditorData InitTranslatePairComponent(TemplateContainer localizationTagView, (string key, string value) translatePair)
        {
            var cDataEditor = new MyLocalizationTranslatePairsEditorData
            {
                PanelView = localizationTagView,
                LocalizationTagLabel = localizationTagView.Query<Label>("localizationTagLabel").First(),
                LocalizationTextLabel = localizationTagView.Query<Label>("localizationTextLabel").First(),
            };

            cDataEditor.LocalizationTagLabel.text = translatePair.key;
            cDataEditor.LocalizationTextLabel.text = translatePair.value;

            return cDataEditor;
        }

        private void UpdateDictionarySelector(MyLocalization localization)
        {
            var names = localization.DictionarySystemNames.ToList();
            _ddDictionarySelector.choices = names;

            var activeDictionaryName = Settings.GetSystemLanguage(localization.ActiveDictionary.LangCode).ToString();
            _ddDictionarySelector.value = activeDictionaryName;
        }

        private void SaveChanges(string msg)
        {
            saveChangesMessage = msg;

            base.SaveChanges();
            MyLocalization.Instance.Save();
            Undo.RecordObject(Settings.DataAssetFile, msg);

            if (_logsEnabled)
                Debug.Log(msg);
        }
    }
}
