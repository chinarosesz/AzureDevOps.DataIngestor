using System;

namespace AzureDevOpsDataCollector.Core.Entities
{
    public class VssBaseEntity
    {
        public string OrganizationName { get; set; }

        public string ProjectName { get; set; }

        public DateTime RowUpdatedDate { get; set; }
    }
}
