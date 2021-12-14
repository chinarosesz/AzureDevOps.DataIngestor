using System.Collections.Generic;
using System.Text.Json.Serialization;

/// Tkane from ADO Microsoft/EngSys/code/Libraries/AzureDevOps/Core/Rest/Model/TeamFoundationIdentities.cs
/// 
namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    /// <summary>
    /// Azure DevOps (ADO) class for identities that start with Microsoft.TeamFoundation.  Undocumented API.
    /// </summary>
    public class TeamFoundationIdentities
    {
        /// <summary>
        /// Count of identities.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Identity information.
        /// </summary>
        public List<TeamFoundationIdentity> Value { get; set; }
    }

    /// <summary>
    /// ADO class for the specific identity.
    /// </summary>
    public class TeamFoundationIdentity
    {
        /// <summary>
        /// Internal ADO Id.  TODO:  Find out if this has any relationship to graph API information.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ADO descriptor.
        /// </summary>
        public string Descriptor { get; set; }

        /// <summary>
        /// ADO storage key.  TODO:  Find out for sure.
        /// </summary>
        public string SubjectDescriptor { get; set; }

        /// <summary>
        /// Either friendly display name (for team names) or GUID.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        /// Friendly display name for groups.  Does not populate for service principals.
        /// </summary>
        public string CustomDisplayName { get; set; }

        /// <summary>
        /// Whether or not the identity is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether or not the identity is a container (group).
        /// </summary>
        public bool? IsContainer { get; set; }

        /// <summary>
        /// Identity membership list if this is a group.
        /// </summary>
        public List<string> Members { get; set; }

        /// <summary>
        /// Identity membership of list if this is a group.
        /// </summary>
        public List<string> MemberOf { get; set; }

        /// <summary>
        /// Identity membership id list if this is a group.
        /// </summary>
        public List<string> MemberIds { get; set; }

        /// <summary>
        /// Extended identity properties.
        /// </summary>
        [JsonPropertyName("properties")] 
        public TeamFoundationIdentityProperties Properties { get; set; }

        /// <summary>
        /// Resource version.
        /// </summary>
        public int ResourceVersion { get; set; }

        /// <summary>
        /// MetaTypeId (unknown what this is used for).
        /// </summary>
        public int MetaTypeId { get; set; }
    }

    /// <summary>
    /// Extended properties for the identity.  Most of the usage for these is currently unknown.
    /// Note: one or more of these might be blank depending on the identity being returned.
    /// </summary>
    public class TeamFoundationIdentityProperties
    {
        /// <summary>
        /// What type of an identity this is (group, etc.)
        /// </summary>
        [JsonPropertyName("SchemaClassName")] 
        public TeamFoundationIdentityExtendedProperties SchemaClassName { get; set; }

        /// <summary>
        /// Text description of the group (if entered in).
        /// </summary>
        public TeamFoundationIdentityExtendedProperties Description { get; set; }

        /// <summary>
        /// Domain of the group (if ADO group, this will be an ADO Url).
        /// </summary>
        public TeamFoundationIdentityExtendedProperties Domain { get; set; }

        /// <summary>
        /// Account name.  TODO:  Find out how this differs between account groups, project groups and AAD groups.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties Account { get; set; }

        /// <summary>
        /// TODO:  Find out what this property does.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties DN { get; set; }

        /// <summary>
        /// Mail properties for the group (if present).
        /// </summary>
        public TeamFoundationIdentityExtendedProperties Mail { get; set; }

        /// <summary>
        /// Security group information.  TODO: find what values/significance of this property.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties SecurityGroup { get; set; }

        /// <summary>
        /// Special type of the group.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties SpecialType { get; set; }

        /// <summary>
        /// Scope ID of the group.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties ScopeId { get; set; }

        /// <summary>
        /// The type of scope.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties ScopeType { get; set; }

        /// <summary>
        /// Local scope ID of the group.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties LocalScopeId { get; set; }

        /// <summary>
        /// GUID of the securing host ID.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties SecuringHostId { get; set; }

        /// <summary>
        /// Scope name (account name, project name, etc).
        /// </summary>
        public TeamFoundationIdentityExtendedProperties ScopeName { get; set; }

        /// <summary>
        /// Whether or not this is global (account).
        /// </summary>
        public TeamFoundationIdentityExtendedProperties GlobalScope { get; set; }

        /// <summary>
        /// Last time the service principal was validated by the system.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties ComplianceValidated { get; set; }

        /// <summary>
        /// TODO:  Find out what this does.
        /// </summary>
        public TeamFoundationIdentityExtendedProperties VirtualPlugin { get; set; }
    }

    /// <summary>
    /// Generic propertly class for an identity.
    /// </summary>
    public class TeamFoundationIdentityExtendedProperties
    {
        /// <summary>
        /// Value type (string, DateTime, etc.).
        /// </summary>
        [JsonPropertyName("$type")]
        public string Type { get; set; }

        /// <summary>
        /// Value for the property.
        /// </summary>
        [JsonPropertyName("$value")]
        public string Value { get; set; }
    }
}
