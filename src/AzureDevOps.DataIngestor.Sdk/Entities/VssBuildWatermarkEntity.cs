using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    public class VssBuildWatermarkEntity
    {
        [Key]
        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Organization { get; set; }

        public DateTime RowUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
