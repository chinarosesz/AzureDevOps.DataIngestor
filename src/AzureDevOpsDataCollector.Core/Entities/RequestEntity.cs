using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOpsDataCollector.Core.Entities
{
    [Table("Request")]
    public class RequestEntity : BaseEntity
    {
        [Key]
        public string RequestUrl { get; set; }

        public string ResponseContent { get; set; }
    }
}
