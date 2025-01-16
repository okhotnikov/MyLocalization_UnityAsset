namespace MLoc.Editor
{
    using System.Collections;
    using UnityEditor;
    using UnityEngine;

    public static class MyLocalizationEditorInspector
    {
        [InitializeOnLoadMethod]
        public static IEnumerator Initialize()
        {
            yield return new WaitForEndOfFrame();
            var myLoc = MyLocalization.Instance;
            Show();
        }

        [MenuItem("Assets/MyLocalization/Reset localization")]
        public static void ResetGallery()
        {
            var cg = MyLocalization.Instance;
            // cg.Clear();
            cg.InitByDefault();
            Show().Init();
        }

        [MenuItem("Window/MyLocalization")]
        public static MyLocalizationEditorWindow Show()
        {
            var w = EditorWindow.GetWindow<MyLocalizationEditorWindow>();
            w.titleContent = new GUIContent("MyLocalization");
            w.Focus();

            return w;
        }
    }
}
