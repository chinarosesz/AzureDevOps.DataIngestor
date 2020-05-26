using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("VssRepository")]
    public class VssRepositoryEntity : VssBaseEntity
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
