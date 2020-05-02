using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("Repository")]
    public class RepositoryEntity
    {
        [Key, Column(Order = 0)]
        public Guid Id { get; set; }

        public string OrganizationName { get; set; }

        public string RepoName { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string DefaultBranch { get; set; }

        public string RepoUrl { get; set; }

        public string RemoteUrl { get; set; }

        public DateTime RowUpdatedDate { get; set; }
    }
}
