using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Security;
using Microsoft.VisualStudio.Services.Security.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Clients
{
    public class VssSecurityClient : SecurityHttpClient
    {
        private readonly ILogger logger;

        internal VssSecurityClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, ILogger logger) : base(baseUrl, credentials, settings)
        {
            this.logger = logger;
        }

        public async Task<SecurityNamespaceDescription> GetSecurityNamespaceDescriptionAsync(string securityNamespaceDescription)
        {
            IEnumerable<SecurityNamespaceDescription> allSecurityAcls = await this.QuerySecurityNamespacesAsync(Guid.Empty);
            SecurityNamespaceDescription returnSecurityDescription = allSecurityAcls.FirstOrDefault(acl => acl.Name.Equals(securityNamespaceDescription));

            return returnSecurityDescription;
        }

        public async Task<List<AccessControlList>> GetQueryAccessControlListsAsync(Guid securityNamespaceId, string token, List<IdentityDescriptor> identityDescriptors = null, bool includeExtendedInfo = true, bool recurse = false)
        {

            List<AccessControlList> accessControlList = new List<AccessControlList>();
            try
            {
            accessControlList = (List<AccessControlList>)await this.QueryAccessControlListsAsync(securityNamespaceId, token, identityDescriptors, includeExtendedInfo, recurse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return accessControlList;
        }
    }
}
