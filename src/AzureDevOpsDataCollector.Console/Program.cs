using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Clients.AzureDevOps;
using AzureDevOpsDataCollector.Core.Collectors;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Console
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Parse command line
            CommandOptions parsedOptions = Program.ParseArguments(args);
            if (parsedOptions == null) { return -1; }

            // Redirect ILogger to Console
            ILogger logger = Program.RedirectLoggerToConsole();

            // Create DbContext client
            VssDbContext dbContext = new VssDbContext(logger, parsedOptions.SqlServerConnectionString);

            // Create AzureDevOps client
            VssClient vssClient = await Program.ConnectAzureDevOpsAsync(parsedOptions, logger);

            // Run collector
            await Program.RunCollectorAsync(parsedOptions, vssClient, dbContext, logger);

            // Returns zero on success
            return 0;
        }

        private static async Task RunCollectorAsync(CommandOptions parsedOptions, VssClient vssClient, VssDbContext dbContext, ILogger logger)
        {
            CollectorBase collector = null;
            if (parsedOptions is ProjectCommandOptions)
            {
                collector = new ProjectCollector(vssClient, dbContext);
            }
            else if (parsedOptions is RepositoryCommandOptions repositoryCommandOptions)
            {
                IEnumerable<string> projects = repositoryCommandOptions.Projects;
                collector = new RepositoryCollector(vssClient, dbContext, logger);
            }
            else if (parsedOptions is PullRequestCommandOptions pullRequestCommandOptions)
            {
                IEnumerable<string> projects = pullRequestCommandOptions.Projects;
                collector = new PullRequestCollector(vssClient, dbContext, projects);
            }
            else if (parsedOptions is BuildDefinitionCommandOptions buildDefinitionCommandOptions)
            {
                IEnumerable<string> projects = buildDefinitionCommandOptions.Projects;
                collector = new BuildDefinitionCollector(vssClient, dbContext, projects, logger);
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

        private static async Task<VssClient> ConnectAzureDevOpsAsync(CommandOptions parsedOptions, ILogger logger)
        {
            VssClient vssClient;
            if (!string.IsNullOrEmpty(parsedOptions.PersonalAccessToken))
            {
                // Connect using personal access token
                vssClient = new VssClient(parsedOptions.Account, parsedOptions.PersonalAccessToken, VssTokenType.Basic, logger);
            }
            else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VssPersonalAccessToken")))
            {
                // Connect using personal access token from environment variable
                vssClient = new VssClient(parsedOptions.Account, Environment.GetEnvironmentVariable("VssPersonalAccessToken"), VssTokenType.Basic, logger);
            }
            else
            {
                // Connect using current signed in domain joined user
                string bearerToken = await VssClientHelper.GetAzureDevOpsBearerTokenForCurrentUserAsync();
                vssClient = new VssClient(parsedOptions.Account, bearerToken, VssTokenType.Bearer, logger);
            }

            return vssClient;
        }

        private static CommandOptions ParseArguments(string[] args)
        {
            // Parse command line options
            ParserResult<object> parserResult = Parser.Default.ParseArguments<
                ProjectCommandOptions, 
                RepositoryCommandOptions, 
                PullRequestCommandOptions, 
                BuildDefinitionCommandOptions>(args);

            // Map results after parsing
            CommandOptions commandOptions = null;
            parserResult.MapResult<ProjectCommandOptions, RepositoryCommandOptions, PullRequestCommandOptions, BuildDefinitionCommandOptions, object>(
                (ProjectCommandOptions opts) => commandOptions = opts,
                (RepositoryCommandOptions opts) => commandOptions = opts,
                (PullRequestCommandOptions opts) => commandOptions = opts,
                (BuildDefinitionCommandOptions opts) => commandOptions = opts,
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
