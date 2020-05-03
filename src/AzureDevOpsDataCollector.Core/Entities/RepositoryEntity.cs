using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("Repository")]
    public class RepositoryEntity : BaseEntity
    {
        [Key]
        public Guid RepoId { get; set; }

        public Guid ProjectId { get; set; }

        public string RepoName { get; set; }

        public string DefaultBranch { get; set; }

        public string WebUrl { get; set; }

        public string RequestUrl { get; set; }
    }
}
