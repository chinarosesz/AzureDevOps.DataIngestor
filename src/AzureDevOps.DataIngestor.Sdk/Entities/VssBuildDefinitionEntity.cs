using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    [Table("VssBuildDefinition")]
    public class VssBuildDefinitionEntity : VssBaseEntity
    {
        [Key]
        public int Id { get; internal set; } // TODO BUG: this needs to include project ID as a double Key to be unique across projects/orgs
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public string ProjectName { get; internal set; }
        public string PoolName { get; internal set; }
        public DateTime CreatedDate { get; internal set; }
        public string UniqueName { get; internal set; }
        public Guid ProjectId { get; internal set; }
        public string Process { get; internal set; }
        public int? PoolId { get; internal set; }
        public bool? IsHosted { get; internal set; }
        public string QueueName { get; internal set; }
        public int? QueueId { get; internal set; }
        public string WebLink { get; internal set; }
        public string RepositoryName { get; internal set; }
        public string RepositoryId { get; internal set; }
        public byte[] GZipCompressedJsonData { get; set; }
    }
}
