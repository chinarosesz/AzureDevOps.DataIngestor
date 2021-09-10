using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    /// <summary>
    /// Class that stores the group identity information.  This is taken from the undocumented Identity APIs.
    /// </summary>
    public class GroupIdentity
    {
        // Number of groups.
        public int Count { get; set; }

        // List that stores the group information.
        [JsonPropertyName("value")]
        public List<GroupInformation> GroupInformation { get; set; }
    }

    /// <summary>
    /// Contains information on the type of container (group or team).
    /// </summary>
    public class SchemaClassName
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains information on the description of the group inside ADO.
    /// </summary>
    public class Description
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Not sure again what this contains - might be legacy TFS.
    /// </summary>
    public class Domain
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains friendly information on the name of the group
    /// </summary>
    public class Account
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains information the security group, think this is related to scope.
    /// </summary>
    public class SecurityGroup
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains friendly information of the special type.  Not sure what this means, most are generic.
    /// </summary>
    public class SpecialType
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains the ID of the scope.
    /// </summary>
    public class ScopeId
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains friendly information on the scope level.
    /// </summary>
    public class ScopeType
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains information on the project.
    /// </summary>
    public class LocalScopeId
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Not entirely sure what this class contains.
    /// </summary>
    public class SecuringHostId
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Contains information on the scope of the container (project, etc.).
    /// </summary>
    public class ScopeName
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Per comments, not sure how this is used.
    /// </summary>
    public class VirtualPlugin
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Group properties.  Again, as this API is undocumented our understanding only goes so far.
    /// </summary>
    public class GroupProperties
    {
        // What type of ADO group this is (Team or Group).
        public SchemaClassName SchemaClassName { get; set; }

        // Description of the group.
        public Description Description { get; set; }

        // Domain (if applicable).
        public Domain Domain { get; set; }

        // Account name.
        public Account Account { get; set; }

        // Security group membership.
        public SecurityGroup SecurityGroup { get; set; }

        // What type this is (typically generic).
        public SpecialType SpecialType { get; set; }

        // ID based off the scope (Team Project, etc.).
        public ScopeId ScopeId { get; set; }

        // What type it is (Team project, etc.).
        public ScopeType ScopeType { get; set; }

        // ID of the local scope (project ID, account ID, etc.).
        public LocalScopeId LocalScopeId { get; set; }

        // ID of the securing host ID.  Not terribly sure of the context.
        public SecuringHostId SecuringHostId { get; set; }

        // Friendly name of the scope
        public ScopeName ScopeName { get; set; }

        // Don't know what this is used for frankly.
        public VirtualPlugin VirtualPlugin { get; set; }
    }

    /// <summary>
    /// Class that stores most of the ID/descriptor information for the group.
    /// </summary>
    public class GroupInformation
    {
        // Group ID.  Will need to leverage this to get the group membership information.
        public string Id { get; set; }

        // Group descriptor - from access control list.
        public string Descriptor { get; set; }

        // Subject descriptor - not sure what this is used for at the moment.
        public string SubjectDescriptor { get; set; }

        // Friendly display name inside ADO.
        public string ProviderDisplayName { get; set; }

        // Whether or not the group is active.
        public bool IsActive { get; set; }

        // Whether or not the group can contain members.  I think - note these are undocumented.
        public bool IsContainer { get; set; }

        // Group membership, if any (so far all I've seen are blank arrays).
        public List<object> Members { get; set; }

        // What groups this group is nested in (again, so far all are blank arrays).
        public List<object> MemberOf { get; set; }

        // Member IDs - above comments apply, this is usually blank.
        public List<object> MemberIds { get; set; }

        // Properties of the group.
        public GroupProperties Properties { get; set; }

        // Resource version.
        public int ResourceVersion { get; set; }

        // Meta Type Id.
        public int MetaTypeId { get; set; }
    }
}
