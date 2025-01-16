namespace MyLocalization.Editor
{
    using UnityEditor;

    public static class DialogHelper
    {
        public static void ShowError(string message)
        {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        public static bool ShowConfirmation(string title, string message)
        {
            return EditorUtility.DisplayDialog(title, message, "Yes", "No");
        }
    }
}
