using System;

namespace AzureDevOpsDataCollector.Core
{
    public class Logger
    {
        public static void WriteLine(object value)
        {
            Console.WriteLine($"{Logger.TimeStamp}: {value}");
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static string TimeStamp
        {
            get
            {
                string timeStamp = DateTime.Now.ToString("u");
                return timeStamp;
            }
        }
    }
}
