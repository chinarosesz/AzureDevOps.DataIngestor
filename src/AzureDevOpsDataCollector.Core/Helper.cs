using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace AzureDevOpsDataCollector.Core
{
    public class Helper
    {
        public static DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }

        public static string SerializeObject(object obj)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string data = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            return data;
        }

        public static void DisplayProjectHeader(object obj, string project, ILogger logger)
        {
            logger.LogInformation($"{obj.GetType().Name}:Collecting data for project {project}".ToUpper());
        }
    }
}
