namespace MLoc.Utils
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UnityEngine;

    public static class JsonUtils
    {
        public static void WriteJson(string filePath, string json)
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Saved at {filePath}");
        }

        public static JObject GetJsonFromBytes(byte[] bytes, bool isCrypted)
        {
            try
            {
                JObject json = null;
                using var ms = new MemoryStream(bytes);
                json = GetJsonFromStream(ms, isCrypted);

                return json;
            }
            catch (Exception e)
            {
                throw new Exception("GetJsonFromBytes error: " + e.Message);
            }
        }

        private static JObject GetJsonFromStream(Stream stream, bool isCrypted)
        {
            if (isCrypted)
                throw new NotImplementedException("Crypted config is not implemented");

            return GetJsonFromUncryptedStream(stream);
        }

        private static JObject GetJsonFromUncryptedStream(Stream uncryptedStream)
        {
            JObject json = null;
            try
            {
                using var textReader = new StreamReader(uncryptedStream, Encoding.UTF8);
                using JsonReader jsonReader = new JsonTextReader(textReader);
                json = (JObject)JToken.ReadFrom(jsonReader);
            }
            catch (Exception e)
            {
                throw new Exception("GetJsonFromUncryptedStream error: " + e.Message);
            }

            return json;
        }
    }
}
