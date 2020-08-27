using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    [Table("VssRepository")]
    public class VssRepositoryEntity : VssBaseEntity
    {
        [Key]
        public Guid RepoId { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Name { get; set; }

        public string DefaultBranch { get; set; }

        public string WebUrl { get; set; }

        public string Data { get; internal set; }
    }
}
