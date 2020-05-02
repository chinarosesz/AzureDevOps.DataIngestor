using CommandLine;
using System.Collections.Generic;

namespace AzureDevOpsDataCollector.Console
{
    public class CommandOptions
    {
        [Option("account", Required = true, HelpText = "The name of Azure DevOps account")]
        public string Account { get; set; }

        [Option("pat", Required = false, Default = null, HelpText = "Personal access token that is generated from Azure DevOps. If not passed in, make sure you set environment variable PersonalAccessToken")]
        public string PersonalAccessToken { get; set; }

        [Option("projects", Required = false, Default = null, Separator = ':', HelpText = "A list of project names, default is to collect all projects if not specified")]
        public IEnumerable<string> Projects { get; set; }
    }
}
