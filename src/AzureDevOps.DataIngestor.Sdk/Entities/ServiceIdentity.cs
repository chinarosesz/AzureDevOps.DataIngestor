using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    /// <summary>
    /// ServiceIdentity - similar to, but not quite the same as, GroupIdentity so there is a new model for this too.
    /// This is taken from the undocumented APIs.
    /// </summary>
    public class ServiceIdentity
    {
        // Number of groups.
        public int Count { get; set; }

        // List that stores the group information.
        [JsonPropertyName("value")]
        public List<ServiceInformation> ServiceInformation { get; set; }
    }

    /// <summary>
    /// Class that stores most of the ID/descriptor information for the service identity.
    /// </summary>
    public class ServiceInformation
    {
        // Group ID.  Shouldn't be needed for this type.
        public string Id { get; set; }

        // Group descriptor - from access control list.
        public string Descriptor { get; set; }

        // Subject descriptor - not sure what this is used for at the moment.
        public string SubjectDescriptor { get; set; }

        // Not sure what this maps to
        public string ProviderDisplayName { get; set; }

        // Friendly display name
        public string CustomDisplayName { get; set; }

        // Whether or not the group is active.
        public bool IsActive { get; set; }

        // Group membership, if any (so far all I've seen are blank arrays).
        public List<object> Members { get; set; }

        // What groups this group is nested in (again, so far all are blank arrays).
        public List<object> MemberOf { get; set; }

        // Member IDs - above comments apply, this is usually blank.
        public List<object> MemberIds { get; set; }

        // Properties of the service identity.
        [JsonPropertyName("properties")]
        public ServiceProperties Properties { get; set; }

        // Resource version.
        public int ResourceVersion { get; set; }

        // Meta Type Id.
        public int MetaTypeId { get; set; }
    }

    /// <summary>
    /// Extended properties for the service identity.  Will not be used.
    /// </summary>
    public class ServiceProperties
    {
        // What type of ADO group this is (User).
        [JsonPropertyName("SchemaClassName")] 
        public SchemaClassNameProperty SchemaClassName { get; set; }

        // Description of the group.
        public string Description { get; set; }

        // Domain (if applicable).
        public string Domain { get; set; }

        // Account name.
        public string Account { get; set; }

        // I don't know what this.
        public DN DN { get; set; }

        // I'm guessing the mail information
        public Mail Mail { get; set; }

        // Whether the account has any special properties
        [JsonPropertyName("SpecialType")]
        public SecuritySpecialType SecuritySpecialType { get; set; }

        // Whether or not it's been vaidated and the time
        public ComplianceValidated ComplianceValidated { get; set; }
    }

    /// <summary>
    /// Contains information on the type of container (group or team).
    /// </summary>
    public class SchemaClassNameProperty
    {
        [JsonPropertyName("$type")]
        public string Type { get; set; }

        [JsonPropertyName("$value")] 
        public string Value { get; set; }
    }

    /// <summary>
    /// At this point I don't know what this could be
    /// </summary>
    public class DN
    {
        // The type, typically System.String
        [JsonPropertyName("$type")]
        public string Type { get; set; }

        // Value of the field
        [JsonPropertyName("$value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Based off the name I suppose this is the mail address.
    /// </summary>
    public class Mail
    {
        // The type, typically System.String
        [JsonPropertyName("$type")]
        public string Type { get; set; }

        // Value of the field
        [JsonPropertyName("$value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Whether or not the account is a special type.
    /// </summary>
    public class SecuritySpecialType
    {
        // The type, typically System.String
        [JsonPropertyName("$type")]
        public string Type { get; set; }

        // Value of the field
        [JsonPropertyName("$value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Whether or not has been validated.
    /// </summary>
    public class ComplianceValidated
    {
        // The type, in this case System.DateTime
        [JsonPropertyName("$type")]
        public string Type { get; set; }

        // Date of compliance
        [JsonPropertyName("$value")]
        public string Value { get; set; }
    }
}

