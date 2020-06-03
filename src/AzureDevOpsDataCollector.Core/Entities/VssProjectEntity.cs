using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("VssProject")]
    public class VssProjectEntity : VssBaseEntity
    {
        [Key]
        public Guid ProjectId { get; set; }

        public string Name { get; set; }

        public string State { get; set; }

        public long Revision { get; set; }

        public string Visibility { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string Url { get; set; }
    }
}
