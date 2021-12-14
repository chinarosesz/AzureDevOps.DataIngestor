﻿using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Entities;
using AzureDevOps.DataIngestor.Sdk.Util;
using CsvHelper;
using CsvHelper.Configuration;
using EntityFrameworkCore.BulkOperations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
//using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Ingestors
{

    public class TeamProjectReference2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string State { get; set; }
        public long Revision { get; set; }
        public string ExtraShut { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class Rootobject<T>
    {
        public T[] value { get; set; }
        public int count { get; set; }
    }

    public class RepositoryACLIngestor : BaseIngestor
    {
        private readonly VssClient vssClient;
        private readonly string sqlConnectionString;
        private readonly IEnumerable<string> projectNames;
        private readonly ILogger logger;

        public RepositoryACLIngestor(VssClient vssClient, string sqlConnectionString, IEnumerable<string> projectNames, ILogger logger)
        {
            this.vssClient = vssClient;
            this.sqlConnectionString = sqlConnectionString;
            this.projectNames = projectNames;
            this.logger = logger;
        }

        public override async Task RunAsync()
        {

            SecurityNamespaceDescription repositorySecurityNamespaceDescription = await this.vssClient.SecurityClient.GetSecurityNamespaceDescriptionAsync("Git Repositories");

            // SAMPLE:CLAUDIO: Just show how to get the repo ACL for each repo using repo specific token
            // List<AccessControlList> repoAccessControlList = (List<AccessControlList>)await this.vssClient.SecurityClient.GetQueryAccessControlListsAsync(repositorySecurityNamespaceDescription.NamespaceId, null, null, true, true);
            // foreach (AccessControlList  acl in repoAccessControlList)
            // {
            //     List<AccessControlList> aclCurrent = (List<AccessControlList>)await this.vssClient.SecurityClient.QueryAccessControlListsAsync(repositorySecurityNamespaceDescription.NamespaceId, acl.Token, null, true, false);
            // }

            // Get projects
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync(this.projectNames);

            // Get repos for all projects
            List<GitRepository> repositories = new List<GitRepository>();

            List<IdentityDescriptorMap> descriptorMapList = new List<IdentityDescriptorMap>();

            foreach (TeamProjectReference project in projects)
            {
                this.DisplayProjectHeader(this.logger, project.Name);
                List<GitRepository> reposFromProject = await this.vssClient.GitClient.GetRepositoriesWithRetryAsync(project.Name);

                foreach (GitRepository repo in reposFromProject)
                {
                    // TODO:REMOVE
                    if (repo.Id.ToString() == "REPO-ID")
                    {
                        Console.WriteLine("debug");
                    }

                    if (false)
                    {
                        // EXAMPLE CHECK CLAUDIOM Permissions on EAch Repo
#pragma warning disable CS0162 // Unreachable code detected
                        List<IdentityDescriptor> descriptors = new List<IdentityDescriptor>();
                        descriptors.Add(new IdentityDescriptor()
                        {
                            // TODO:  Make Tenant ID configurable.
                            Data = null,
                            Identifier = $@"72f988bf-86f1-41af-91ab-2d7cd011db47\claudiom@microsoft.com",
                            IdentityType = "Microsoft.IdentityModel.Claims.ClaimsIdentity"
                        });

                        List<AccessControlList> acl3 = (List<AccessControlList>)await this.vssClient.SecurityClient.QueryAccessControlListsAsync(repositorySecurityNamespaceDescription.NamespaceId,
                               CreateRepositoryToken(repo.ProjectReference.Id.ToString(), repo.Id.ToString()), descriptors, true, false);
                        // END EXAMPLE
#pragma warning restore CS0162 // Unreachable code detected                    
                    }

                    try
                    {
                        // Get Branches
                        List<GitRef> repRefs = await this.vssClient.GitClient.GetRefsAsync(project.Name, repo.Id, includeStatuses: true, latestStatusesOnly: true);

                        List<VssRepositoryACLEntity> vssRepositoryACLEntityList = new List<VssRepositoryACLEntity>();

                        if (repRefs.Count == 0)
                        {
                            logger.LogWarning($"Repo {repo.Id} does not have any branch information");
                        }

                        foreach (GitRef branch in repRefs)
                        {
                            // TODO: Remove this as it limits to default repo only
                            if (String.Compare(repo.DefaultBranch, branch.Name) != 0)
                            {
                                continue;
                            }

                            this.logger.LogInformation($"Retrieving exporting Permissions for repository [{repo.Name}], Branch [{branch.Name}] ...");

                            string token = this.CreateBranchToken(repo.ProjectReference.Id.ToString(), repo.Id.ToString(), branch.Name);
                            List<AccessControlList> branchAccessControlList = (List<AccessControlList>)await this.vssClient.SecurityClient.GetQueryAccessControlListsAsync(repositorySecurityNamespaceDescription.NamespaceId, token);//, identityDescriptors, true, false);

                            if (branchAccessControlList.Count == 0)
                            {
                                logger.LogWarning($"Repo {repo.Id}, Branch {branch.Name} does not have any ACL information");
                            }

                            foreach (AccessControlList branchAccessControl in branchAccessControlList)
                            {
                                foreach (var dict in branchAccessControl.AcesDictionary)
                                {
                                    string permsAllow = this.GetPermisions(dict.Value.ExtendedInfo.EffectiveAllow);
                                    string permsDenied = this.GetPermisions(dict.Value.ExtendedInfo.EffectiveDeny);

                                    VssRepositoryACLEntity vssRepositoryACLEntity = new VssRepositoryACLEntity()
                                    {
                                        Organization = this.vssClient.OrganizationName,
                                        ProjectId = project.Id,
                                        ProjectName = project.Name,
                                        RepoId = repo.Id,
                                        RepositoryName = repo.Name,
                                        DefaultBranch = repo.DefaultBranch,
                                        BranchName = branch.Name,
                                        Descriptor = dict.Key.ToString(),
                                        PermissionsAllow = permsAllow,
                                        PermissionsDeny = permsDenied
                                    };

                                    // TODO: Need to cache these so we don't have to retrieve the descriptors that we have seen before
                                    IdentityDescriptorMap identityMap = this.GetIdentityDetails(dict.Key.ToString());
                                    vssRepositoryACLEntity.DescriptorName = identityMap.DisplayName;
                                    vssRepositoryACLEntity.DescriptorType = identityMap.SchemaClassName;
                                    vssRepositoryACLEntity.SubjectDescriptor = identityMap.SubjectDescriptor;

                                    descriptorMapList.Add(identityMap);

                                    vssRepositoryACLEntityList.Add(vssRepositoryACLEntity);
                                }
                            }
                        }

                        // Export/Ingest RepositoryACL list
                        if (Helper.ExtractToCSV)
                        {
                            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                            {
                                // Don't write the header again.
                                HasHeaderRecord = Helper.ExtractToCSVExportHeader,
                                SanitizeForInjection = true
                            };

                            using (Stream stream = File.Open(@".\csv\RepositoryACL.csv", Helper.ExtractToCSVExportHeader ? FileMode.Create : FileMode.Append))
                            using (CsvWriter csv = new CsvWriter(new StreamWriter(stream), config))
                            {
                                csv.WriteRecords(vssRepositoryACLEntityList);
                                this.logger.LogInformation($"Done exporting Permissions for repository [{repo.Name}] CSV file");
                                Helper.ExtractToCSVExportHeader = false;
                            }
                        }
                    }
                    catch (VssException ex) 
                    {
                        Console.WriteLine($"Failed to get info for https://piemini.visualstudio.com/{project.Name}/_git/{repo.Name}");
                        Console.WriteLine(ex.Message);
                    }
                }

                repositories.AddRange(reposFromProject);
            }

            this.logger.LogInformation($"Getting repository user and group memberships");
            List<IdentityMember> groupMembership = GetGroupMembership(descriptorMapList);

            if (Helper.ExtractToCSV)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    // Don't write the header again.
                    HasHeaderRecord = true,
                    SanitizeForInjection = true
                };

                using (Stream stream = File.Open(@".\csv\IdentityList.csv", FileMode.Create))
                using (CsvWriter csv = new CsvWriter(new StreamWriter(stream), config))
                {
                    csv.WriteRecords(groupMembership);
                    this.logger.LogInformation($"Done exporting identities to CSV file");
                    Helper.ExtractToCSVExportHeader = false;
                }
            }

        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="securityDescriptor"></param>
        /// <returns></returns>
        private IdentityDescriptorMap GetIdentityDetails(string securityDescriptor)
        {
            IdentityDescriptorMap descriptor = new IdentityDescriptorMap();
            descriptor.Descriptor = securityDescriptor;

            try
            {
                if (securityDescriptor.StartsWith("System:ServicePrincipal"))
                {
                    TeamFoundationIdentities identity =
                        vssClient.VSSHttpClient.HttpGetAsync<TeamFoundationIdentities>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?descriptors={securityDescriptor}&api-version=5.0")
                        .GetAwaiter().GetResult();
                    descriptor.DisplayName = identity.Count > 0 ? identity.Value[0].ProviderDisplayName : securityDescriptor;
                    descriptor.SubjectDescriptor = identity.Count > 0 ? identity.Value[0].SubjectDescriptor : string.Empty;
                    descriptor.SchemaClassName = identity.Count > 0 ? identity.Value[0].Properties.SchemaClassName.Value : string.Empty;
                    descriptor.IsContainer = false;
                }
                else if (securityDescriptor.Contains("@") && !securityDescriptor.StartsWith("System:ServicePrincipal"))
                {
                    TeamFoundationIdentities identity =
                        vssClient.VSSHttpClient.HttpGetAsync<TeamFoundationIdentities>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?descriptors={securityDescriptor}&api-version=5.0")
                        .GetAwaiter().GetResult();
                    descriptor.DisplayName = identity.Count > 0 ? identity.Value[0].ProviderDisplayName : securityDescriptor;
                    descriptor.SubjectDescriptor = identity.Count > 0 ? identity.Value[0].SubjectDescriptor : string.Empty;
                    descriptor.SchemaClassName = identity.Count > 0 ? identity.Value[0].Properties.SchemaClassName.Value : string.Empty;
                    descriptor.IsContainer = false;
                }
                else if (securityDescriptor.StartsWith("Microsoft.TeamFoundation.ServiceIdentity"))
                {
                    TeamFoundationIdentities identity =
                        vssClient.VSSHttpClient.HttpGetAsync<TeamFoundationIdentities>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?descriptors={securityDescriptor}&api-version=5.0")
                        .GetAwaiter().GetResult();
                    descriptor.DisplayName = identity.Count > 0 ? identity.Value[0].CustomDisplayName : securityDescriptor;
                    descriptor.SubjectDescriptor = identity.Count > 0 ? identity.Value[0].SubjectDescriptor : string.Empty;
                    descriptor.SchemaClassName = identity.Count > 0 ? "ServiceAccount" : string.Empty;
                    descriptor.IsContainer = false;
                }
                else if (securityDescriptor.StartsWith("Microsoft.TeamFoundation.Identity"))
                {
                    descriptor.SchemaClassName = "Group";
                    TeamFoundationIdentities group =
                        vssClient.VSSHttpClient.HttpGetAsync<TeamFoundationIdentities>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?descriptors={securityDescriptor}&api-version=6.0")
                        .GetAwaiter().GetResult();
                    Debug.Assert(group.Count <= 1, "Group lookup should return at most 1 member");
                    descriptor.DisplayName = group.Count > 0 ? group.Value[0].ProviderDisplayName : securityDescriptor;
                    descriptor.SubjectDescriptor = group.Count > 0 ? group.Value[0].SubjectDescriptor : string.Empty;
                    descriptor.SchemaClassName = group.Count > 0 ? group.Value[0].Properties.SchemaClassName.Value : string.Empty;
                    descriptor.IsContainer = (bool)group.Value[0].IsContainer;
                }
                else
                {
                    throw new InvalidDataException($"Descriptor type not expected [{securityDescriptor}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"Failed to retrieve identity informaton for descritor [{securityDescriptor}]");
            }
            return descriptor;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptorMapList"></param>
        /// <returns></returns>
        List<IdentityMember> GetGroupMembership(List<IdentityDescriptorMap> descriptorMapList)
        {
            // Elininate duplicated values
            List<IdentityDescriptorMap> uniqueIdentities = descriptorMapList
                .Select(m => new { m.Descriptor, m.DisplayName, m.SchemaClassName, m.SubjectDescriptor, m.IsContainer })
                .Distinct()
                .Select(m => new IdentityDescriptorMap
                {
                    Descriptor = m.Descriptor,
                    DisplayName = m.DisplayName,
                    SchemaClassName = m.SchemaClassName,
                    SubjectDescriptor = m.SubjectDescriptor,
                    IsContainer = m.IsContainer
                })
                .ToList();

            Dictionary<string, List<IdentityMember>> identityMembershipList = new Dictionary<string, List<IdentityMember>>();

            // Loops thru list of Groups/Users that have been given permissions to repos. 
            // If Group, it will iterate thru them and so forth until there are no more groups to itereate.
            foreach (IdentityDescriptorMap s in uniqueIdentities)
            {
                string rootSubscriptor = s.SubjectDescriptor;

                if (!identityMembershipList.ContainsKey(rootSubscriptor + s.SubjectDescriptor))
                {
                    if (s.SubjectDescriptor == "bnd.dXBuOjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0N1wxYnBpcGVjb3JlLWRldkBtaWNyb3NvZnQuY29t")
                    {
                        Console.WriteLine("Stop");
                    }

                    if (s.IsContainer == true)
                    {
                        this.logger.LogInformation($"Getting membership for group {s.DisplayName}...");
                        GetUsersRecursively(rootDescriptor: s.SubjectDescriptor, depth: 1, subjectDescriptor: s.SubjectDescriptor, ref identityMembershipList);
                    }
                    else
                    {
                        // Add User/Service Account to dictionary so we don't have to add then again
                        if (!identityMembershipList.ContainsKey(rootSubscriptor + s.SubjectDescriptor))
                        {
                            List<IdentityMember> m = new List<IdentityMember>() {new IdentityMember
                                {
                                    RootDescriptor = rootSubscriptor, // TODO: REMOVE COMMENT OR FIX s.SubjectDescriptor,
                                    Descriptor = s.SubjectDescriptor,
                                    Depth = 1,
                                    DescriptorDisplayName = s.DisplayName,
                                    SchemaClassName = s.SchemaClassName,
                                    MemberDescriptor = s.SubjectDescriptor,
                                    MemberDisplayName = s.DisplayName,
                                    IsMemberDescriptorContainer = false,
                                }};

                            identityMembershipList.Add(rootSubscriptor + s.SubjectDescriptor, m);
                        }
                    }
                }
            }

            // FLatten list of Users and Groups
            List<IdentityMember> flatMembershipList = identityMembershipList.SelectMany(m => m.Value).ToList();

            return flatMembershipList;
        }

        void GetUsersRecursively(string rootDescriptor, int depth, string  subjectDescriptor, ref Dictionary<string, List<IdentityMember>> identityMembershipList)
        {
            if (!string.IsNullOrEmpty(subjectDescriptor) && !identityMembershipList.ContainsKey(rootDescriptor + subjectDescriptor))
            {
                //if (subjectDescriptor == "svc.MGJkNThiMDItZDUyZi00ZDMzLWE1ZmQtNGRkMDViMTkyNzY1OkJ1aWxkOjEyNmM4MmYwLThkNTgtNGZlOC05ZTc4LTI2MDdiYzMxNDM2OA")
                //{
                //    Console.WriteLine("debug");
                //}

                List<IdentityMember> members = GetMembers(rootDescriptor, depth, subjectDescriptor, ref identityMembershipList);
                foreach (IdentityMember member in members)
                {
                    // TODO:REMOVE
                    if (member.MemberDescriptor == "bnd.XXX")
                    {
                        Console.WriteLine("debug");
                    }

                    // Check if we have seen this member before, otherwise proceed to get further groups or add member
                    if (!string.IsNullOrEmpty(member.MemberDescriptor) && !identityMembershipList.ContainsKey(rootDescriptor + member.MemberDescriptor))
                    {
                        if (member.IsMemberDescriptorContainer == true)
                        {
                            GetUsersRecursively(rootDescriptor, depth + 1, member.MemberDescriptor, ref identityMembershipList);
                        }
                        else
                        {
                            identityMembershipList.Add(rootDescriptor + member.MemberDescriptor,
                                new List<IdentityMember>
                                { new IdentityMember {
                                RootDescriptor = rootDescriptor,
                                Descriptor = member.MemberDescriptor,
                                DescriptorDisplayName = member.MemberDisplayName,
                                SchemaClassName = member.MemberDescriptor.StartsWith("svc.", StringComparison.OrdinalIgnoreCase) ? "ServiceAccount" : "User",
                                MemberDescriptor = member.MemberDescriptor,
                                MemberDisplayName = member.MemberDisplayName,
                                IsMemberDescriptorContainer = false,
                            }});
                        }
                    }
                }
            }
        }

        List<IdentityMember> GetMembers (string rootDescriptor, int depth, string subjectDescriptor, ref Dictionary<string, List<IdentityMember>> identityMembershipList)
        {
            // Get Group Information
            TeamFoundationIdentities group =
                vssClient.VSSHttpClient.HttpGetAsync<TeamFoundationIdentities>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?subjectDescriptors={subjectDescriptor}&api-version=6.0")
                .GetAwaiter().GetResult();

            // Get Members
            IdentityGroupMembership groupMembers =
                vssClient.VSSHttpClient.HttpGetAsync<IdentityGroupMembership>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/graph/memberships/{subjectDescriptor}?direction=down&api-version=6.0-preview.1")
                .GetAwaiter().GetResult();

            List<IdentityMember> list = new List<IdentityMember>();
            string subjectDescriptorsToQuery = String.Empty; // TODO: remove this once it's moved into the do/while

            // If group has no members, add itself to the list with no members and to the dictioanry if not already and ad
            if (groupMembers.Count == 0 && !identityMembershipList.ContainsKey(rootDescriptor + subjectDescriptor))
            {
                list.Add(new IdentityMember()
                {
                    RootDescriptor = rootDescriptor,
                    Descriptor = subjectDescriptor,
                    Depth = depth,
                    DescriptorDisplayName = group.Value[0].ProviderDisplayName,
                    SchemaClassName = group.Value[0].Properties.SchemaClassName.Value,
                    MemberDescriptor = string.Empty,
                    MemberDisplayName = string.Empty,
                    //IsMemberDescriptorContainer = false,
                });
            }

            // Due to URL limit size of 2048 chars, we have to batch calls into chuncks of no more than 15 descriptors at a time.
            // On average, User descriptors at 145 chars long up to 186 for groups. Most of large memberships have users or service accounts
            for (int i = 0; i < groupMembers.Value.Count; i = i + 15)
            {
                subjectDescriptorsToQuery = string.Join(",", groupMembers.Value.Skip(i).Take(15).Select(i => i.MemberDescriptor.ToString()).ToArray());

                // Get All Identities of the members returned above.
                // string subjectDescriptorsToQuery = string.Join(",", groupMembers.Value.Take(25).Select(i => i.MemberDescriptor.ToString()).ToArray());
                if (!string.IsNullOrEmpty(subjectDescriptorsToQuery))
                {
                    TeamFoundationIdentities identities =
                        vssClient.VSSHttpClient.HttpGetAsync<TeamFoundationIdentities>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?subjectDescriptors={subjectDescriptorsToQuery}&queryMembership=Direct&api-version=6.0")
                        .GetAwaiter().GetResult();

                    if (!identityMembershipList.ContainsKey(rootDescriptor + subjectDescriptor))
                    {
                        foreach (TeamFoundationIdentity id in identities.Value)
                        {
                            if (id.IsActive == false)
                            {
                                Console.WriteLine("stop");
                            }

                            // TODO:REMOVE
                            if (id.SubjectDescriptor == "GROUP/USER ID")
                            {
                                Console.WriteLine("debug");
                            }

                            list.Add(new IdentityMember()
                            {
                                RootDescriptor = rootDescriptor,
                                Descriptor = subjectDescriptor,
                                Depth = depth,
                                DescriptorDisplayName = group.Value[0].ProviderDisplayName,
                                SchemaClassName = group.Value[0].Properties.SchemaClassName.Value,
                                MemberDescriptor = id.SubjectDescriptor,
                                MemberDisplayName = id.Descriptor.EndsWith(id.ProviderDisplayName) && !string.IsNullOrEmpty(id.CustomDisplayName)
                                    ? id.CustomDisplayName : id.ProviderDisplayName,
                                IsMemberDescriptorContainer = id.IsContainer ?? false,
                            });
                        }
                    }
                }
            }

            // Add group itself to the dictionary so we don't attempt to get the data again
            identityMembershipList.Add(rootDescriptor + subjectDescriptor, list);

            return list;
        }

        private string GetPermisions(int mask)
        {
            GitRepositoryPermissions permissions = (GitRepositoryPermissions)mask;
            return permissions.ToString().Replace(",", "|");
        }

        private void IngestData(List<GitRepository> repositories)
        {
            List<VssRepositoryEntity> entities = new List<VssRepositoryEntity>();

            foreach (GitRepository repo in repositories)
            {
                VssRepositoryEntity repoEntity = new VssRepositoryEntity
                {
                    Organization = this.vssClient.OrganizationName,
                    RepoId = repo.Id,
                    Name = repo.Name,
                    DefaultBranch = repo.DefaultBranch,
                    ProjectId = repo.ProjectReference.Id,
                    ProjectName = repo.ProjectReference.Name,
                    WebUrl = repo.RemoteUrl,
                };
                entities.Add(repoEntity);
            }

            if (Helper.ExtractToCSV)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    // Don't write the header again.
                    HasHeaderRecord = Helper.ExtractToCSVExportHeader,
                    SanitizeForInjection = true
                };

                using (Stream stream = File.Open(@".\csv\Repository.csv", Helper.ExtractToCSVExportHeader ? FileMode.Create : FileMode.Append))
                using (CsvWriter csv = new CsvWriter(new StreamWriter(stream), config))
                {
                    csv.WriteRecords(entities);
                    this.logger.LogInformation($"Done exporting Repositories to CSV file");
                    Helper.ExtractToCSVExportHeader = false;
                }
            }
            else
            {
                this.logger.LogInformation("Start ingesting repository permission data to database...");
                using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
                using IDbContextTransaction transaction = context.Database.BeginTransaction();
                context.BulkDelete(context.VssRepositoryACLEntities.Where(v => v.Organization == this.vssClient.OrganizationName || v.Organization == null));
                int insertResult = context.BulkInsert(entities);
                transaction.Commit();
                this.logger.LogInformation($"Done ingesting {insertResult} records");
            }
        }
        
        /// <summary>
        /// Takes a branch name and converts to token.  Tokenis in UTF-16 little endian format as described here:
        /// https://devblogs.microsoft.com/devops/git-repo-tokens-for-the-security-service/
        /// </summary>
        /// <param name="projectId">Project ID for the branch.</param>
        /// <param name="repositoryId">Repository ID for the branch.</param>
        /// <param name="branchName">Full branch name (for now, no check for friendly name).</param>
        /// <returns></returns>
        internal string CreateBranchToken(string projectId, string repositoryId, string branchName)
        {
            string baseToken = $"repoV2/{projectId}/{repositoryId}/refs/heads";
            string token = string.Empty;
            char[] branchNameArray = branchName.Substring("refs/heads/".Length).ToCharArray();
            for (int i = 0; i < branchNameArray.Length; i++)
            {
                if (branchNameArray[i].Equals('/'))
                {
                    token += branchNameArray[i];
                }
                else
                {
                    byte letterByte = (byte)branchNameArray[i];
                    if (letterByte != 0)
                    {
                        string temp = BitConverter.ToString(BitConverter.GetBytes(letterByte));
                        temp = temp.Replace("-", string.Empty);
                        token += temp;
                    }
                }
            }
            string branchToken = baseToken + token;

            return branchToken;
        }

        /// <summary>
        /// Takes a repository id and converts to token:
        /// https://devblogs.microsoft.com/devops/git-repo-tokens-for-the-security-service/
        /// </summary>
        /// <param name="projectId">Project ID for the branch.</param>
        /// <param name="repositoryId">Repository ID for the branch.</param>
        /// <returns></returns>
        internal string CreateRepositoryToken(string projectId, string repositoryId)
        {
            return $"repoV2/{projectId}/{repositoryId}";
        }
    }
}
