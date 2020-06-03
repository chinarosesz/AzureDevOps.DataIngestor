using CommandLine;
using System.Collections.Generic;

namespace AzureDevOpsDataCollector.Console
{
    public class CommandOptions
    {
        [Option("account", Required = true, HelpText = "The name of Azure DevOps account, for example: https://dev.azure.com/lilatran where lilatran is the account name")]
        public string Account { get; set; }

        [Option("pat", Required = true, HelpText = "Azure DevOps Personal Access Token")]
        public string PersonalAccessToken { get; set; }

        [Option("collector", Required = false, Default = CollectorType.Project, HelpText = "The name of the collector to run.")]
        public CollectorType CollectorType { get; set; }

        [Option("projects", Required = false, Default = null, Separator = ':', HelpText = "A list of project names, default is to collect all projects if not specified. For example: 'project1:project2:project3'")]
        public IEnumerable<string> Projects { get; set; }
    }
}
