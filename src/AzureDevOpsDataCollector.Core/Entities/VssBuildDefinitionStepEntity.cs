using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("VssBuildDefinitionStep")]
    public class VssBuildDefinitionStepEntity : VssBaseEntity
    {
        public int StepNumber { get; set; }
        public int BuildDefinitionId { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string PhaseType { get; set; }
        public string PhaseRefName { get; set; }
        public string PhaseName { get; set; }
        public int? PhaseQueueId { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; }
        public Guid TaskDefinitionId { get; set; }
        public string TaskVersionSpec { get; set; }
        public string Condition { get; set; }
    }
}