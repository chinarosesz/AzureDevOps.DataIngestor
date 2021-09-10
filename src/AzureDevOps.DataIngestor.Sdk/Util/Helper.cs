using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace AzureDevOps.DataIngestor.Sdk.Util
{
    public class Helper
    {
        public static DateTime UtcNow { get; } = DateTime.UtcNow;

        // TODO: Need to find another way to integrate CVS Export so adding as static here for now.
        public static bool ExtractToCSV { get; set; } = true;

        // TODO: Need to find another way to integrate CVS Export so adding as static here for now.
         public static bool ExtractToCSVExportHeader { get; set; } = true;

        // TODO: Need bettwer way to share token between VssClient and HttpClient so adding here for now.
        public static string AuthenticationHeader { get; set; } = string.Empty;

        public static byte[] Compress(Object obj)
        {
            string jsonString = Helper.SerializeObject(obj);
            byte[] jsonBytes = Helper.Compress(jsonString);
            return jsonBytes;
        }

        private static string SerializeObject(object obj)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string data = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            return data;
        }

        /// <summary>
        /// You can decompress data in SQL Server by calling DECOMPRESS function
        /// Example: CAST(DECOMPRESS([MyCompressedJsonString]) AS NVARCHAR(MAX)) AS JsonString
        /// </summary>
        private static byte[] Compress(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            using MemoryStream msi = new MemoryStream(bytes);
            using MemoryStream mso = new MemoryStream();
            using (GZipStream gs = new GZipStream(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }
            return mso.ToArray();
        }
    }
}
