using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("VssBuildDefinition")]
    public class VssBuildDefinitionEntity : VssBaseEntity
    {
        [Key]
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public string ProjectName { get; internal set; }
        public string PoolName { get; internal set; }
        public DateTime CreatedDate { get; internal set; }
        public string UniqueName { get; internal set; }
        public Guid ProjectId { get; internal set; }
    }
}
