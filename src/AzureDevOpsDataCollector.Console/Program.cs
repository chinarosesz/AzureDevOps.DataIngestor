﻿using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Collectors;
using CommandLine;
using CommandLine.Text;
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
            VssClient vssClientConnector = new VssClient(parsedOptions.Account, parsedOptions.PersonalAccessToken);

            // Create DbContext client
            VssDbContext dbContext = new VssDbContext();
            await MigrateDatabaseToLatestVersion.ExecuteAsync(dbContext);

            // Collect Repository data
            if (parsedOptions.CollectorType == CollectorType.Repository)
            {
                RepositoryCollector repositoryCollector = new RepositoryCollector(vssClientConnector, dbContext, parsedOptions.Projects);
                await repositoryCollector.RunAsync();
            }
            else if (parsedOptions.CollectorType == CollectorType.Project)
            {
                ProjectCollector collector = new ProjectCollector(vssClientConnector, dbContext, parsedOptions.Projects);
                await collector.RunAsync();
            }

            // Returns zero on success
            return 0;
        }

        private static CommandOptions ParseArguments(string[] args)
        {
            // Parse command line options
            Parser parser = new Parser((ParserSettings parserSettings) =>
            {
                parserSettings.AutoHelp = false;
            });
            ParserResult<CommandOptions> parserResult = parser.ParseArguments<CommandOptions>(args);

            // Map results after parsing
            CommandOptions commandOptions = new CommandOptions();
            parserResult.MapResult<CommandOptions, object>((CommandOptions opts) => commandOptions = opts, (errs) => 1);

            // Return null if not able to parse
            if (parserResult.Tag == ParserResultType.NotParsed) 
            {
                HelpText helpText = HelpText.AutoBuild(parserResult);
                helpText.AddEnumValuesToHelpText = true;
                helpText.AddOptions(parserResult);
                System.Console.WriteLine(helpText);
                return null; 
            }

            // Return list of parsed commands
            return commandOptions;
        }
    }
}
