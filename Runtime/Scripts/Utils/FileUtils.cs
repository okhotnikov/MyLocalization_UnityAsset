namespace MLoc.Utils
{
    using UnityEngine;
    using System.IO;

    public static class FileUtils
    {
        public static string LoadResourceAsString(string resourceFileName, string filePath)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return Resources.Load<TextAsset>(resourceFileName)?.text;

            return File.Exists(filePath) ? File.ReadAllText(filePath) : null;
#else
            return Resources.Load<TextAsset>(resourceFileName)?.text;
#endif
        }

        public static byte[] LoadResourceAsBytes(string resourceFileName, string filePath)
        {
            byte[] dictionaryBytes;
#if UNITY_EDITOR
            if (Application.isPlaying)
                dictionaryBytes = Resources.Load<TextAsset>(resourceFileName)?.bytes;

            dictionaryBytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
#else
            dictionaryBytes = Resources.Load<TextAsset>(resourceFileName)?.bytes;
#endif

            if (dictionaryBytes != null)
                return dictionaryBytes;

            Debug.LogWarning($"{resourceFileName} not found or empty.");
            return null;
        }

        public static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
