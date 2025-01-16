namespace MLoc.Editor
{
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class CustomDropDown : VisualElement
    {
        private VisualElement _headerContainer;
        private VisualElement _dropDownContainer;
        private ToolbarSearchField _filterTextField;

        private Label _selectedTagNameLabel;
        private Label _selectedTagTextLabel;

        private Dictionary<string, (string, VisualElement)> _items = new ();

        private bool _isExpanded = false;
        private string _selectedLocalizationTag;

        public CustomDropDown()
        {
            AddToClassList("custom-dropdown");

            _headerContainer = new VisualElement();
            _headerContainer.AddToClassList("header-container");
            _headerContainer.style.flexDirection = FlexDirection.Row;
            _headerContainer.RegisterCallback<ClickEvent>(evt => ToggleDropDown());
            Add(_headerContainer);

            var header = new Label("Name");
            header.AddToClassList("dropdown-header");

            _selectedTagNameLabel = new Label("Tag");
            _selectedTagNameLabel.AddToClassList("dropdown-header");
            _selectedTagNameLabel.style.flexGrow = 1;
            _selectedTagNameLabel.pickingMode = PickingMode.Ignore;

            _selectedTagTextLabel = new Label("TagText");
            _selectedTagTextLabel.AddToClassList("dropdown-header");
            _selectedTagTextLabel.style.flexGrow = 1;
            _selectedTagTextLabel.pickingMode = PickingMode.Ignore;

            var button = new Button();
            button.AddToClassList("dropdown-button");
            button.text = "Select";

            _headerContainer.Add(header);
            _headerContainer.Add(_selectedTagNameLabel);
            _headerContainer.Add(_selectedTagTextLabel);
            _headerContainer.Add(button);

            _dropDownContainer = new VisualElement();
            _dropDownContainer.AddToClassList("dropdown-container");
            _dropDownContainer.style.display = DisplayStyle.None; // Скрыть по умолчанию
            Add(_dropDownContainer);

            _filterTextField = new ToolbarSearchField();
            _filterTextField.AddToClassList("filter-text-field");
            _filterTextField.placeholderText = "Search...";
            _filterTextField.value = "Search...";

            _dropDownContainer.Add(_filterTextField);

            _filterTextField.RegisterValueChangedCallback(chEvent =>
            {
                var searchValue = chEvent.newValue.ToLowerInvariant();
                foreach (var (locTag, (locText, container)) in _items)
                {
                    container.style.display = string.IsNullOrEmpty(searchValue) ||
                                              locText.ToLowerInvariant().Contains(searchValue) ||
                                              locTag.ToLowerInvariant().Contains(searchValue) ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });
        }

        private void ToggleDropDown()
        {
            _isExpanded = !_isExpanded;
            _dropDownContainer.style.display = _isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetValue(string locTag)
        {
            if (string.IsNullOrEmpty(locTag))
                return;

            if (!_items.ContainsKey(locTag) || _selectedLocalizationTag == locTag)
                return;

            var (locText, _) = _items[locTag];
            SetSelectedTag(locTag, locText);
            _selectedLocalizationTag = locTag;
        }

        public void AddItem(string locTag, string locText)
        {
            var itemContainer = new VisualElement();
            itemContainer.AddToClassList("dropdown-item");

            var locTagLabel = new Label(locTag);
            locTagLabel.AddToClassList("dropdown-tag");

            var locTextLabel = new Label(locText);
            locTextLabel.AddToClassList("dropdown-text");

            itemContainer.Add(locTagLabel);
            itemContainer.Add(locTextLabel);

            itemContainer.RegisterCallback<ClickEvent>(evt =>
            {
                SetSelectedTag(locTag, locText);
                NotifyValueChanged(locTag);
                _selectedLocalizationTag = locTag;

                Debug.Log($"Selected: {locTag}");
                ToggleDropDown();
            });

            _dropDownContainer.Add(itemContainer);

            _items.Add(locTag, (locText, itemContainer));
        }

        public void ClearItems()
        {
            _items.Clear();
            _dropDownContainer.Clear();
            _selectedLocalizationTag = null;
        }

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            RegisterCallback(callback);
        }

        private void NotifyValueChanged(string newValue)
        {
            using (var evt = ChangeEvent<string>.GetPooled(_selectedLocalizationTag, newValue))
            {
                evt.target = this;
                SendEvent(evt);
            }
        }

        private void SetSelectedTag(string tag, string text)
        {
            _selectedTagNameLabel.text = tag;
            _selectedTagTextLabel.text = text;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public new class UxmlFactory : UxmlFactory<CustomDropDown, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _headerText = new ()
                { name = "header-text", defaultValue = "Select an item" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is CustomDropDown customDropDown)
                {
                    customDropDown.Q<Label>().text = _headerText.GetValueFromBag(bag, cc);
                }
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
