using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    [Table("VssCommit")]
    public class VssCommitEntity : VssBaseEntity
    {
        [Key]
        [Column(Order = 0)]
        public string CommitId { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid RepositoryId { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        [StringLength(300)]
        public string AuthorEmail { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CommitTime { get; set; }

        [StringLength(500)]
        public string Comment { get; set; }

        [StringLength(2083)]
        public string RemoteUrl { get; set; }
    }
}
