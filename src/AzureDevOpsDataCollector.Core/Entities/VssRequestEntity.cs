using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("VssRequest")]
    public class VssRequestEntity : VssBaseEntity
    {
        [Key]
        public string RequestUrl { get; set; }

        public string ResponseContent { get; set; }
    }
}
