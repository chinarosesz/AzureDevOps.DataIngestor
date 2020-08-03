using CommandLine;
using System.Collections.Generic;

namespace AzureDevOpsDataCollector.Console
{
    [Verb("project", HelpText = "Collect projects data given a specific project or all projects by default")]
    public class ProjectCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("repository", HelpText = "Collect repository data given a specific project or all projects by default")]
    public class RepositoryCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("pullrequest", HelpText = "Collect pull request data given a specific project or all projects by default")]
    public class PullRequestCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("builddefinition", HelpText = "Collect bulid definition data given a specific project or all projects by default")]
    public class BuildDefinitionCommandOptions : ProjectCommandOptionsBase
    {
    }

    public class CommandOptions
    {
        [Option("account", Required = true, HelpText = "The name of Azure DevOps account, for example: https://dev.azure.com/lilatran where lilatran is the account name")]
        public string Account { get; set; }

        [Option("pat", HelpText = "Azure DevOps Personal Access Token. If not provided, will look for environment variable VssPersonalAccessToken. If not set as environment variable, will use current logged in domain user to connect as Oauth")]
        public string PersonalAccessToken { get; set; }

        [Option("connection", Default = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AzureDevOps", HelpText = "SQL Server database connection string. If not specfied, data will be in local database")]
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
