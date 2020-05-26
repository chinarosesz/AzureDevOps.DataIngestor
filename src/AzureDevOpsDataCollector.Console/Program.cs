using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Collectors;
using CommandLine;
using EFCore.AutomaticMigrations;
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

            // Create AzureDevOps client
            VssClientConnector vssClientConnector = new VssClientConnector(parsedOptions.Account, parsedOptions.PersonalAccessToken);

            // Create DbContext client
            VssDbContext dbContext = new VssDbContext();
            await MigrateDatabaseToLatestVersion.ExecuteAsync(dbContext);

            // Collect Repository data
            RepositoryCollector repositoryCollector = new RepositoryCollector(vssClientConnector, dbContext, parsedOptions.Projects);
            await repositoryCollector.RunAsync();

            return 0;
        }

        private static CommandOptions ParseArguments(string[] args)
        {
            // Parse command line options
            CommandOptions commandOptions = new CommandOptions();
            ParserResult<CommandOptions> results = Parser.Default.ParseArguments<CommandOptions>(args);

            // Map results after parsing
            results.MapResult<CommandOptions, object>((CommandOptions opts) => commandOptions = opts, (errs) => 1);

            // Throw if not able to parse
            if (results.Tag == ParserResultType.NotParsed)
            {
                return null;
            }

            return commandOptions;
        }
    }
}
