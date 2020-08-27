using AzureDevOps.DataIngestor.Sdk.Util;
using System;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    public class VssBaseEntity
    {
        public string Organization { get; set; }
        public DateTime RowUpdatedDate { get; set; } = Helper.UtcNow;
    }
}
