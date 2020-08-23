using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    [Table("VssPullRequestWatermark")]
    public class VssPullRequestWatermarkEntity
    {
        [Key]
        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string PullRequestStatus { get; set; }

        public string Organization { get; set; }

        public DateTime RowUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
