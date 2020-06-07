using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Collectors;
using CommandLine;
using CommandLine.Text;
using EFCore.AutomaticMigrations;
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

            // Create DbContext client
            VssDbContext dbContext = new VssDbContext();
            await MigrateDatabaseToLatestVersion.ExecuteAsync(dbContext);

            // Collect Project
            if (parsedOptions.ProjectCommandOptions != null)
            {
                string account = parsedOptions.ProjectCommandOptions.Account;
                string personalAccessToken = parsedOptions.ProjectCommandOptions.PersonalAccessToken;

                // Create AzureDevOps client
                VssClient vssClient = new VssClient(account, personalAccessToken);

                // Run
                ProjectCollector collector = new ProjectCollector(vssClient, dbContext);
                await collector.RunAsync();
            }
            // Collect Repository
            else if (parsedOptions.RepositoryCommandOptions != null)
            {
                string account = parsedOptions.RepositoryCommandOptions.Account;
                string personalAccessToken = parsedOptions.RepositoryCommandOptions.PersonalAccessToken;
                IEnumerable<string> projects = parsedOptions.RepositoryCommandOptions.Projects;

                // Create AzureDevOps client
                VssClient vssClientConnector = new VssClient(account, personalAccessToken);

                // Run
                RepositoryCollector repositoryCollector = new RepositoryCollector(vssClientConnector, dbContext, projects);
                await repositoryCollector.RunAsync();
            }

            // Returns zero on success
            return 0;
        }

        private static CommandOptions ParseArguments(string[] args)
        {
            // Parse command line options
            ParserResult<object> parserResult = Parser.Default.ParseArguments<ProjectCommandOptions, RepositoryCommandOptions>(args);

            // Map results after parsing
            CommandOptions commandOptions = new CommandOptions();
            parserResult.MapResult<ProjectCommandOptions, RepositoryCommandOptions, object>(
                (ProjectCommandOptions opts) => commandOptions.ProjectCommandOptions = opts,
                (RepositoryCommandOptions opts) => commandOptions.RepositoryCommandOptions = opts,
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
