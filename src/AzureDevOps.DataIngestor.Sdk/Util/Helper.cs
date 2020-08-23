using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace AzureDevOps.DataIngestor.Sdk.Util
{
    public class Helper
    {
        public static DateTime UtcNow { get; } = DateTime.UtcNow;

        public static string SerializeObject(object obj)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string data = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            return data;
        }


    }
}
