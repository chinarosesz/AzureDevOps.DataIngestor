using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Entities;
using AzureDevOps.DataIngestor.Sdk.Util;
using CsvHelper;
using CsvHelper.Configuration;
using EntityFrameworkCore.BulkOperations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Security;
using System;
using System.Collections.Generic;
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

            foreach (TeamProjectReference project in projects)
            {
                this.DisplayProjectHeader(this.logger, project.Name);
                List<GitRepository> reposFromProject = await this.vssClient.GitClient.GetRepositoriesWithRetryAsync(project.Name);

                foreach (GitRepository repo in reposFromProject)
                {
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

                                    //try: TODO
                                    //{
                                    //    //GraphStorageKeyResult result1 = await this.vssClient.IdentityClient.GetDescriptorByIdAsync()
                                    //    // GraphStorageKeyResult result2 = await this.vssClient.GraphClient.GetStorageKeyAsync(dict.Key.ToString());
                                    //    await GetResultFromRandomAPI();
                                    //}
                                    //catch (Exception ex)
                                    //{
                                    //    Console.WriteLine(ex.Message);
                                    //}

                                    VssRepositoryACLEntity vssRepositoryACLEntity = new VssRepositoryACLEntity()
                                    {
                                        Organization = "piemini",
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
                                    vssRepositoryACLEntity.DescriptorName = this.GetDescriptorName(dict.Key.ToString());
                                    vssRepositoryACLEntity.DescriptorType = this.GetDescriptorType(dict.Key.ToString());

                                    vssRepositoryACLEntityList.Add(vssRepositoryACLEntity);
                                }
                            }
                        }

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
                        Console.WriteLine(ex.Message);
                    }
                }

                repositories.AddRange(reposFromProject);
            }
        }

        private string GetDescriptorName(string securityDescriptor)
        {
            string descriptorName;

            try
            {
                if (securityDescriptor.StartsWith("System:ServicePrincipal"))
                {
                    descriptorName = "ServicePrincipal";
                }
                else if (securityDescriptor.Contains("@") && !securityDescriptor.StartsWith("System:ServicePrincipal"))
                {
                    string[] splitDescriptor = securityDescriptor.Split("\\");
                    descriptorName = splitDescriptor[1];
                }
                else if (securityDescriptor.StartsWith("Microsoft.TeamFoundation.ServiceIdentity"))
                {
                    descriptorName = securityDescriptor;
                    ServiceIdentity svc =
                        vssClient.VSSHttpClient.HttpGetAsync<ServiceIdentity>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?descriptors={securityDescriptor}&api-version=5.0")
                        .GetAwaiter().GetResult();
                    descriptorName = svc.Count > 0 ? svc.ServiceInformation[0].CustomDisplayName : securityDescriptor;
                }
                else if (securityDescriptor.StartsWith("Microsoft.TeamFoundation.Identity"))
                {
                    GroupIdentity group =
                        vssClient.VSSHttpClient.HttpGetAsync<GroupIdentity>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?descriptors={securityDescriptor}&api-version=6.0")
                        .GetAwaiter().GetResult();
                    descriptorName = group.Count > 0 ? group.GroupInformation[0].ProviderDisplayName : securityDescriptor;
                }
                else
                {
                    throw new InvalidDataException($"Descriptor type not expected [{securityDescriptor}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"Failed to retrieve identity informaton for descritor [{securityDescriptor}]");
                descriptorName = securityDescriptor;
            }
            return descriptorName;
        }

        private string GetDescriptorType(string securityDescriptor)
        {
            string branchMember;
            if (securityDescriptor.StartsWith("System:ServicePrincipal"))
            {
                branchMember = "ServicePrincipal";
            }
            else if (securityDescriptor.Contains("@") && !securityDescriptor.StartsWith("System:ServicePrincipal"))
            {
                string[] splitDescriptor = securityDescriptor.Split("\\");
                //branchMember.Name = splitDescriptor[1];
                branchMember = "User";
            }
            else if (securityDescriptor.StartsWith("Microsoft.TeamFoundation.ServiceIdentity"))
            {
                branchMember = "ServiceAccount";
                //ServiceIdentity svc =
                //vssClient.VSSHttpClient.HttpGetAsync<ServiceIdentity>($"https://vssps.dev.azure.com/{this.vssClient.OrganizationName}/_apis/identities?descriptors={securityDescriptor}&api-version=5.0")
                //    .GetAwaiter().GetResult();

            }
            else if (securityDescriptor.StartsWith("Microsoft.TeamFoundation.Identity"))
            {
                //GroupIdentity groupIdentity = branchService.GetGroupIdentity(securityDescriptor.Descriptor);
                //if (groupIdentity.Count > 1)
                //{
                //    // TODO:  We are never expecting this, set a breakpoint in this case.
                //    Console.WriteLine("Not expecting anything else, breakpoint");
                //    Console.ReadLine();
                //}

                //CustomGroupSuccessors customGroup = this.GroupIdentityToJson(groupIdentity, branchMember.Allow, branchMember.Deny);
                //branchMember.CustomSuccessor = customGroup;
                branchMember = "Group";
                //branchMember.Name = groupIdentity.GroupInformation[0].ProviderDisplayName;
            }
            else
            {
                throw new InvalidDataException($"Descriptor type not expected [{securityDescriptor}");
            }
            return branchMember;
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
        public async Task GetResultFromRandomAPI(string apiUrl = "projects")
        {
             await vssClient.VSSHttpClient.HttpGetAsync<Rootobject<TeamProjectReference>>($"https://dev.azure.com/{this.vssClient.OrganizationName}/_apis/{apiUrl}");
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
