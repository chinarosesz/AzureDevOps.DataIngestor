using CommandLine;
using System.Collections.Generic;

namespace AzureDevOps.DataIngestor
{
    [Verb("project", HelpText = "Collect projects data given a specific project or all projects by default")]
    public class ProjectCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("repository", HelpText = "Collect repository data given a specific project or all projects by default")]
    public class RepositoryCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("repositoryacl", HelpText = "Collect repository permissions data given a specific project or all projects by default")]
    public class RepositoryACLCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("pullrequest", HelpText = "Collect pull request data given a specific project or all projects by default")]
    public class PullRequestCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("commit", HelpText = "Collect commit data given a specific project or all projects by default")]
    public class CommitCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("builddefinition", HelpText = "Collect bulid definition data given a specific project or all projects by default")]
    public class BuildDefinitionCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("build", HelpText = "Collect build data given a specific project or all projects by default")]
    public class BuildCommandOptions : ProjectCommandOptionsBase
    {
    }

    public class CommandOptions
    {
        [Option("organization", Required = true, HelpText = "The name of Azure DevOps account, for example: https://dev.azure.com/lilatran where lilatran is the account name")]
        public string Organization { get; set; }

        [Option("pat", HelpText = "Provide an Azure DevOps Personal Access Token. If not provided, the program looks for environment variable VssPersonalAccessToken. If not set as environment variable, program uses current logged in domain user to connect as Oauth")]
        public string PersonalAccessToken { get; set; }

        [Option("sqlserverconnectionstring", HelpText = "If not specified from command line or as an environment variable VssSqlServerConnectionString, then use local database VssAzureDevOps")]
        public string SqlServerConnectionString { get; set; }
    }

    public class ProjectCommandOptionsBase : CommandOptions
    {
        private const string helpText = 
            "By default if not specified all projects are collected\r\n" +
            "To collect data for a list of projects use ':' to separate each project name \r\n" +
            "Example: AzureDevOpsDataCollector.Console.exe pullrequest --projects project1:project2:project3 \r\n" +
            "Example: AzureDevOpsDataCollector.Console.exe pullrequest";

        [Option("projects", Separator = ':', HelpText = helpText)]
        public IEnumerable<string> Projects { get; set; }
    }
}
