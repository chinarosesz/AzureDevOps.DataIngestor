﻿using CommandLine;
using System.Collections.Generic;

namespace AzureDevOpsDataCollector.Console
{
    [Verb("project", HelpText = "Collect all projects from an Azure DevOps organization account")]
    public class ProjectCommandOptions : CommandOptionsBase
    {
    }

    [Verb("repository", HelpText = "Collect Azure DevOps repository data from an organization account")]
    public class RepositoryCommandOptions : ProjectCommandOptionsBase
    {
    }

    public class CommandOptionsBase
    {
        [Option("account", Required = true, HelpText = "The name of Azure DevOps account, for example: https://dev.azure.com/lilatran where lilatran is the account name")]
        public string Account { get; set; }

        [Option("pat", Required = true, HelpText = "Azure DevOps Personal Access Token")]
        public string PersonalAccessToken { get; set; }
    }

    public class ProjectCommandOptionsBase : CommandOptionsBase
    {
        [Option("projects", Required = false, Default = null, Separator = ':', HelpText = "A list of project names, default is to collect all projects if not specified. For example: 'project1:project2:project3'")]
        public IEnumerable<string> Projects { get; set; }
    }
}
