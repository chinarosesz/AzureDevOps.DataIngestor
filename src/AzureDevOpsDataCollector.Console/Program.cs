using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Clients.AzureDevOps;
using AzureDevOpsDataCollector.Core.Collectors;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Collections.Generic;
using System.Linq;
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
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole((ConsoleLoggerOptions options) =>
                {
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss: ";
                    options.Format = ConsoleLoggerFormat.Systemd;
                });
            });
            ILogger logger = loggerFactory.CreateLogger(string.Empty);

            // Create DbContext client
            VssDbContext dbContext = new VssDbContext(logger, parsedOptions.SqlServerConnectionString);

            // Create AzureDevOps client
            VssClient vssClient;
            if (!string.IsNullOrEmpty(parsedOptions.PersonalAccessToken))
            {
                // Connect using personal access token
                vssClient = new VssClient(parsedOptions.Account, parsedOptions.PersonalAccessToken, VssTokenType.Basic, logger);
            }
            else
            {
                // Connect using current signed in domain joined user
                string bearerToken = await VssClientHelper.GetAzureDevOpsBearerTokenForCurrentUserAsync();
                vssClient = new VssClient(parsedOptions.Account, bearerToken, VssTokenType.Bearer, logger);
            }

            // Getting ready to run each collector based on command options provided from CLI
            CollectorBase collector = null;
            if (parsedOptions is ProjectCommandOptions)
            {
                collector = new ProjectCollector(vssClient, dbContext);
            }
            else if (parsedOptions is RepositoryCommandOptions repositoryCommandOptions)
            {
                collector = new RepositoryCollector(vssClient, dbContext, logger);
            }
            else if (parsedOptions is PullRequestCommandOptions pullRequestCommandOptions)
            {
                List<string> projects = pullRequestCommandOptions.Projects.ToList();
                collector = new PullRequestCollector(vssClient, dbContext, projects);
            }

            // Finally run selected collector!
            await collector.RunAsync();

            // Returns zero on success
            return 0;
        }

        private static CommandOptions ParseArguments(string[] args)
        {
            // Parse command line options
            ParserResult<object> parserResult = Parser.Default.ParseArguments<ProjectCommandOptions, RepositoryCommandOptions, PullRequestCommandOptions>(args);

            // Map results after parsing
            CommandOptions commandOptions = null;
            parserResult.MapResult<ProjectCommandOptions, RepositoryCommandOptions, PullRequestCommandOptions, object>(
                (ProjectCommandOptions opts) => commandOptions = opts,
                (RepositoryCommandOptions opts) => commandOptions = opts,
                (PullRequestCommandOptions opts) => commandOptions = opts,
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
