using System;

namespace AzureDevOpsDataCollector.Core.Entities
{
    public class VssBaseEntity
    {
        public string Organization { get; set; }

        public string Data { get; set; }

        public DateTime RowUpdatedDate { get; set; } = Helper.UtcNow;
    }
}
