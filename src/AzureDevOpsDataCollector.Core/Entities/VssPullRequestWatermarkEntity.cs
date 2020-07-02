using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("VssPullRequestWatermark")]
    public class VssPullRequestWatermarkEntity
    {
        [Key]
        public Guid RepositoryId { get; set; }

        public string RepositoryName { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string PullRequestStatus { get; set; }

        public string Organization { get; set; }

        public DateTime RowUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
