using CommandLine;
using System.Collections.Generic;

namespace AzureDevOpsDataCollector.Console
{
    [Verb("project", HelpText = "Collect all projects")]
    public class ProjectCommandOptions : CommandOptions
    {
    }

    [Verb("repository", HelpText = "Collect all repositories")]
    public class RepositoryCommandOptions : CommandOptions
    {
    }

    [Verb("pullrequest", HelpText = "Collect pull requests data given a specific project")]
    public class PullRequestCommandOptions : ProjectCommandOptionsBase
    {
    }

    [Verb("builddefinition", HelpText = "Collect build definitions given a specific project")]
    public class BuildDefinitionCommandOptions : ProjectCommandOptionsBase
    {
    }

    public class CommandOptions
    {
        [Option("account", Required = true, HelpText = "The name of Azure DevOps account, for example: https://dev.azure.com/lilatran where lilatran is the account name")]
        public string Account { get; set; }

        [Option("pat", HelpText = "Azure DevOps Personal Access Token")]
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
