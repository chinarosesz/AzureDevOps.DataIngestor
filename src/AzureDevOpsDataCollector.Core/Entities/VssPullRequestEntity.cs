using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("VssPullRequest")]
    public class VssPullRequestEntity : VssBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PullRequestId { get; set; }

        [Key]
        public Guid RepositoryId { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string AuthorEmail { get; set; }

        public string Title { get; set; }

        [Required]
        public string Status { get; set; }

        public string LastMergeCommitID { get; set; }

        [Required]
        public string SourceBranch { get; set; }

        public string LastMergeTargetCommitId { get; set; }

        [Required]
        public string TargetBranch { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreationDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime ClosedDate { get; set; }
    }
}
