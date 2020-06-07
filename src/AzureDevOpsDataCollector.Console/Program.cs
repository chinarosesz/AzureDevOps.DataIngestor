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
            CommandOptionsBase parsedOptions = Program.ParseArguments(args);
            if (parsedOptions == null) { return -1; }

            // Create DbContext client
            VssDbContext dbContext = new VssDbContext();
            await MigrateDatabaseToLatestVersion.ExecuteAsync(dbContext);

            // Create AzureDevOps client
            string account = parsedOptions.Account;
            string personalAccessToken = parsedOptions.PersonalAccessToken;
            VssClient vssClient = new VssClient(account, personalAccessToken);

            // Getting ready to run each collector based on command options provided from CLI
            CollectorBase collector = null;
            if (parsedOptions is ProjectCommandOptions)
            {
                collector = new ProjectCollector(vssClient, dbContext);
            }
            else if (parsedOptions is RepositoryCommandOptions repositoryCommandOptions)
            {
                IEnumerable<string> projects = repositoryCommandOptions.Projects;
                collector = new RepositoryCollector(vssClient, dbContext, projects);
            }

            // Finally run selected collector!
            await collector.RunAsync();

            // Returns zero on success
            return 0;
        }

        private static CommandOptionsBase ParseArguments(string[] args)
        {
            // Parse command line options
            ParserResult<object> parserResult = Parser.Default.ParseArguments<ProjectCommandOptions, RepositoryCommandOptions>(args);

            // Map results after parsing
            CommandOptionsBase commandOptions = null;
            parserResult.MapResult<ProjectCommandOptions, RepositoryCommandOptions, object>(
                (ProjectCommandOptions opts) => commandOptions = opts,
                (RepositoryCommandOptions opts) => commandOptions = opts,
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
