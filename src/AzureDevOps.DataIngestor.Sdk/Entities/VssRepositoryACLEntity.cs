using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevOps.DataIngestor.Sdk.Entities
{
    [Table("VssRepositoryACL")]
    public class VssRepositoryACLEntity : VssBaseEntity
    {
        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        [Key]
        public Guid RepoId { get; set; }

        public string RepositoryName { get; set; }

        public string DefaultBranch { get; set; }

        public string BranchName { get; set; }

        public string DescriptorName { get; set; }

        public string DescriptorType { get; set; }

        public string Descriptor { get; set; }

        public string SubjectDescriptor { get; set; }

        public string PermissionsAllow { get; set; }
        
        public string PermissionsDeny { get; set; }

        public string WebUrl { get; set; }
    }
}
