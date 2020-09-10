using System.ComponentModel.DataAnnotations;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    internal class VssDataEntity
    {
        [Key]
        public string Id { get; set; }
        public string Data { get; set; }
    }
}
