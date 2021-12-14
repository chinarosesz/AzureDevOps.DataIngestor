using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    /// <summary>
    /// Azure DevOps (ADO) class for GroupMembership.
    /// </summary>
    public class IdentityGroupMembership
    {
        /// <summary>
        /// Count of Members.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Membership information.
        /// </summary>
        public List<IdentityMember> Value { get; set; }
    }

    /// <summary>
    /// ADO class for the specific identity.
    /// </summary>
    public class IdentityMember
    {
        /// <summary>
        /// Container Identity descriptor
        /// </summary>
        public string RootDescriptor { get; set; }

        [JsonPropertyName("containerDescriptor")]
        public string Descriptor { get; set; }
        public string DescriptorDisplayName { get; set; }
        /// <summary>
        /// Depth level of identity from Root
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// Container SchemaClassName
        /// </summary>
        public string SchemaClassName { get; set; }
        [JsonPropertyName("memberDescriptor")] 
        public string MemberDescriptor { get; set; }
        /// <summary>
        /// Member Display Name
        /// </summary>
        public string MemberDisplayName { get; set; }
        /// <summary>
        /// Whether MemberDescriptor is a container
        /// </summary>
        public bool? IsMemberDescriptorContainer { get; set; }
    }
}