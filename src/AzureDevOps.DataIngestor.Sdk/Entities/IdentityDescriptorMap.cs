using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{

    /// <summary>
    /// Common Class that stores the legacy identity information and new Graph information.
    /// </summary>
    public class IdentityDescriptorMap
    {
        public string Identity { get; set; }
        public string SchemaClassName { get; set; }
        public bool ?IsContainer { get; set; }
        public string DisplayName { get; set; }
        public string Descriptor { get; set; }
        public string SubjectDescriptor { get; set; }
        public List<IdentityMember> Members { get; set; }
    }
}
