namespace MLoc.Editor
{
    using MLoc.Data;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(MyLocalizationTarget), true)]
    public class MyLocalizationTargetDataPropertyDrawer : PropertyDrawer
    {
        [SerializeField] VisualTreeAsset _mainTargetDataPropertyDrawerAsset;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = _mainTargetDataPropertyDrawerAsset.Instantiate();

            var dropdownTags = root.Query<CustomDropDown>("selectTagCustomDropdown").First();

            var myLoc = MyLocalization.Instance;
            if (myLoc != null)
            {
                foreach (var cGuid in myLoc.ActiveDictionary.TranslatePairs)
                {
                    var localizedTag = cGuid.LocalizedTag;
                    var localizedText = cGuid.LocalizedText;

                    dropdownTags.AddItem(localizedTag, localizedText);
                }
            }

            dropdownTags.RegisterValueChangedCallback(chEvent =>
            {
                if (!chEvent.target.Equals(chEvent.currentTarget))
                    return;

                property.FindPropertyRelative("_tagGuid").stringValue = chEvent.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            var selectedTag = property.FindPropertyRelative("_tagGuid").stringValue;
            dropdownTags.SetValue(selectedTag);

            return root;
        }
    }
}
