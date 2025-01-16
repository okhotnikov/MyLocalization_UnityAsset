namespace MLoc.Services
{
    using System;
    using System.Collections.Generic;
    using MLoc.Utils;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UnityEditor;
    using UnityEngine;

    public class GoogleSheetParser
    {
        public void ParseAndSave(List<List<object>> value)
        {
            var normalizedData = NormalizeSheetData(value);
            SaveLocalization(normalizedData);
        }

        private Dictionary<string, Dictionary<string, string>> NormalizeSheetData(List<List<object>> sheetData)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            if (sheetData == null || sheetData.Count < 2)
            {
                Debug.LogError("<b>Google Sheets:</b> Sheet data is empty or has insufficient rows.");
                return result;
            }

            // First row only for service lang name
            // Second row is keys
            var headers = sheetData[1];
            for (var i = 2; i < sheetData.Count; i++)
            {
                var row = sheetData[i];
                if (row.Count == 0) continue;

                var rowDict = new Dictionary<string, string>();

                for (int j = 1; j < headers.Count && j < row.Count; j++)
                {
                    string key = headers[j]?.ToString() ?? $"Column_{j}";
                    string value = row[j]?.ToString() ?? string.Empty;

                    rowDict[key] = value;
                }

                string id = row[0]?.ToString() ?? $"Row_{i}";
                result[id] = rowDict;
            }

            return result;
        }

        private void SaveLocalization(Dictionary<string, Dictionary<string, string>> data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                FileUtils.EnsureDirectoryExists(Settings.DictionaryAssetsFilePath);
                JsonUtils.WriteJson(Settings.DictionaryAssetsFilePath, json);

#if UNITY_EDITOR
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(Settings.DictionaryAssetFile);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"<b>Google Sheets:</b> Failed to save localization: {ex.Message}");
                throw;
            }
        }
    }
}
