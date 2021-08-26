using Microsoft.TeamFoundation.Build.WebApi;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    [Table("VssBuild")]
    public class VssBuildEntity : VssBaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Key]
        public Guid ProjectId { get; set; }

        public Guid RepositoryId { get; set; }

        public string BuildNumber { get; set; }

        public bool? KeepForever { get; set; }

        public bool? RetainedByRelease { get; set; }

        public BuildStatus? Status { get; set; }

        public BuildResult? Result { get; set; }

        public DateTime? QueueTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? FinishTime { get; set; }

        public string Url { get; set; }

        public int DefinitionId { get; set; }

        public string SourceBranch { get; set; }

        public string SourceVersion { get; set; }

        public int QueueId { get; set; }

        public string QueueName { get; set; }

    }
}
