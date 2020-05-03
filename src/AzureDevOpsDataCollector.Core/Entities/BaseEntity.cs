using System;

namespace AzureDevOpsDataCollector.Core.Entities
{
    public class BaseEntity
    {
        public string OrganizationName { get; set; }

        public string ProjectName { get; set; }

        public DateTime RowUpdatedDate { get; set; }
        
    }
}
