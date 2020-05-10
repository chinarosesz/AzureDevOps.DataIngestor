using AzureDevOpsDataCollector.Core.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Test.AzureDevOpsDataCollector.Core
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1Async()
        {
            AzureDevOpsClient client = new AzureDevOpsClient("litra");
            var token = await client.InteractiveLoginAsync("litra@microsoft.com");
            Console.WriteLine(token);
        }
    }
}
