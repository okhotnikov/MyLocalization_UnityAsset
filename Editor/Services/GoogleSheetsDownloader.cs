namespace MLoc.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Newtonsoft.Json.Linq;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class GoogleSheetsDownloader
    {
        private SheetsService _sheetsService;

        public GoogleSheetsDownloader()
        {
            Authenticate();
        }

        private void Authenticate()
        {
            try
            {
                var credentialsPath = Path.Combine(Application.dataPath, MyLocalization.Instance.CredentialFilePath);
                GoogleCredential credential;

                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(new[] { SheetsService.Scope.SpreadsheetsReadonly });
                }

                _sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Sheets Downloader"
                });

                Debug.Log("<b>Google Sheets:</b> Authentication successful!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"<b>Google Sheets:</b> Authentication failed: {ex.Message}");
                throw;
            }
        }

        private List<List<object>> GetSheetData(string targetSheetName)
        {
            try
            {
                var documentId = string.Empty;
                var credentialsPath = Path.Combine(Application.dataPath, MyLocalization.Instance.CredentialFilePath);
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        JObject jsonObject = JObject.Parse(jsonContent);
                        documentId = jsonObject.GetValue("document_id")?.ToString();
                    }
                }

                if (string.IsNullOrEmpty(documentId))
                    throw new Exception("Document ID is not found in credentials.json");

                var request = _sheetsService.Spreadsheets.Get(documentId);
                var response = request.Execute();
                var sheets = response.Sheets;
                var resultValueRange = new List<List<object>>();

                foreach (var sheet in sheets)
                {
                    var sheetName = sheet.Properties.Title;
                    if (!sheetName.Equals(targetSheetName))
                        continue;

                    var range = $"{sheetName}";
                    var dataRequest = _sheetsService.Spreadsheets.Values.Get(documentId, range);
                    var dataResponse = dataRequest.Execute();

                    resultValueRange.AddRange(dataResponse.Values.Select(innerList => innerList.ToList()));
                }

                Debug.Log("<b>Google Sheets:</b> Data retrieved successfully!");
                return resultValueRange;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<b>Google Sheets:</b> Failed to get data: {ex.Message}");
                throw;
            }
        }

        public List<List<object>> DownloadSheet(string targetSheetName = "Localization")
        {
            return GetSheetData(targetSheetName);
        }
    }
}
