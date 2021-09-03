using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    [Table("VssCommitWatermark")]
    public class VssCommitWatermarkEntity
    {
        [Key]
        public Guid RepositoryId { get; set; }
        public string Organization { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime RowUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
