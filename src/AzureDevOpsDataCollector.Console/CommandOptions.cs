using CommandLine;
using System.Collections.Generic;

namespace AzureDevOpsDataCollector.Console
{
    [Verb("project", HelpText = "Collect all projects from an Azure DevOps organization account")]
    public class ProjectCommandOptions : CommandOptions
    {
    }

    [Verb("repository", HelpText = "Collect Azure DevOps repository data from an organization account")]
    public class RepositoryCommandOptions : CommandOptions
    {
    }

    [Verb("pullrequest", HelpText = "Collect Azure DevOps repository data from an organization account")]
    public class PullRequestCommandOptions : ProjectCommandOptionsBase
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
        [Option("projects", Required = false, Default = null, Separator = ':', HelpText = "A list of project names, default is to collect all projects if not specified. For example: 'project1:project2:project3'")]
        public IEnumerable<string> Projects { get; set; }
    }
}
