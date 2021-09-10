using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Ingestors;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Redirect ILogger to Console
            ILogger logger = Program.RedirectLoggerToConsole();

            // Parse command line
            CommandOptions parsedOptions = Program.ParseArguments(args);
            if (parsedOptions == null) { return -1; }

            // Get Personal Access Token from command line or environment variable
            string personalAccessToken = parsedOptions.PersonalAccessToken;
            if (string.IsNullOrEmpty(personalAccessToken))
            {
                personalAccessToken = Environment.GetEnvironmentVariable("VssPersonalAccessToken");
            }

            // Get Sql Server connection string from command line or environment variable
            string sqlServerConnectionString = string.IsNullOrEmpty(parsedOptions.SqlServerConnectionString) ? Environment.GetEnvironmentVariable("VssSqlServerConnectionString") : parsedOptions.SqlServerConnectionString;

            // Create Azure DevOps HttpClient and a standard HttpClient for REST CALLS when needed.
            VssClient vssClient;
            if (string.IsNullOrEmpty(personalAccessToken))
            {
                string bearerToken = await VssClientHelper.SignInUserAndGetTokenUsingMSAL();

                vssClient = new VssClient(parsedOptions.Organization, bearerToken, VssTokenType.Bearer, logger);
            }
            else
            {
                // Connect using personal access token
                vssClient = new VssClient(parsedOptions.Organization, personalAccessToken, VssTokenType.Basic, logger);
            }

            // Run collector
            await Program.RunCollectorAsync(parsedOptions, vssClient, sqlServerConnectionString, logger);

            // Returns zero on success
            return 0;
        }

        private static async Task RunCollectorAsync(CommandOptions parsedOptions, VssClient vssClient, string sqlConnectionString, ILogger logger)
        {
            BaseIngestor collector = null;
            if (parsedOptions is ProjectCommandOptions)
            {
                collector = new ProjectIngestor(vssClient, sqlConnectionString, logger);
            }
            else if (parsedOptions is RepositoryCommandOptions repositoryCommandOptions)
            {
                IEnumerable<string> projects = repositoryCommandOptions.Projects;
                collector = new RepositoryIngestor(vssClient, sqlConnectionString, projects, logger);
            }
            else if (parsedOptions is RepositoryACLCommandOptions repositoryACLCommandOptions)
            {
                IEnumerable<string> projects = repositoryACLCommandOptions.Projects;
                collector = new RepositoryACLIngestor(vssClient, sqlConnectionString, projects, logger);
            }
            else if (parsedOptions is PullRequestCommandOptions pullRequestCommandOptions)
            {
                IEnumerable<string> projects = pullRequestCommandOptions.Projects;
                collector = new PullRequestIngestor(vssClient, sqlConnectionString, projects, logger);
            }
            else if (parsedOptions is CommitCommandOptions commitCommandOptions)
            {
                IEnumerable<string> projects = commitCommandOptions.Projects;
                collector = new CommitIngestor(vssClient, sqlConnectionString, projects, logger);
            }
            else if (parsedOptions is BuildDefinitionCommandOptions buildDefinitionCommandOptions)
            {
                IEnumerable<string> projects = buildDefinitionCommandOptions.Projects;
                collector = new BuildDefinitionIngestor(vssClient, sqlConnectionString, projects, logger);
            }
            else if (parsedOptions is BuildCommandOptions buildCommandOptions)
            {
                IEnumerable<string> projects = buildCommandOptions.Projects;
                collector = new BuildIngestor(vssClient, sqlConnectionString, projects, logger);
            }

            // Finally run selected collector!
            await collector.RunAsync();
        }

        private static ILogger RedirectLoggerToConsole()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole((ConsoleLoggerOptions options) =>
                {
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss: ";
                    options.Format = ConsoleLoggerFormat.Systemd;
                });
            });

            ILogger logger = loggerFactory.CreateLogger(string.Empty);

            return logger;
        }

        private static CommandOptions ParseArguments(string[] args)
        {
            // Parse command line options
            ParserResult<object> parserResult = Parser.Default.ParseArguments<
                ProjectCommandOptions, 
                RepositoryCommandOptions,
                RepositoryACLCommandOptions,
                PullRequestCommandOptions,
                CommitCommandOptions,
                BuildDefinitionCommandOptions,
                BuildCommandOptions>(args);

            // Map results after parsing
            CommandOptions commandOptions = null;
            parserResult.MapResult<ProjectCommandOptions, RepositoryCommandOptions, RepositoryACLCommandOptions, PullRequestCommandOptions, CommitCommandOptions, BuildDefinitionCommandOptions, BuildCommandOptions, object>(
                (ProjectCommandOptions opts) => commandOptions = opts,
                (RepositoryCommandOptions opts) => commandOptions = opts,
                (RepositoryACLCommandOptions opts) => commandOptions = opts,
                (PullRequestCommandOptions opts) => commandOptions = opts,
                (CommitCommandOptions opts) => commandOptions = opts,
                (BuildDefinitionCommandOptions opts) => commandOptions = opts,
                (BuildCommandOptions opts) => commandOptions = opts,
                (errs) => 1);

            // Return null if not able to parse
            if (parserResult.Tag == ParserResultType.NotParsed) 
            {
                HelpText helpText = HelpText.AutoBuild(parserResult);
                helpText.AddEnumValuesToHelpText = true;
                helpText.AddOptions(parserResult);
                commandOptions = null;
            }

            // Return list of parsed commands
            return commandOptions;
        }
    }
}
