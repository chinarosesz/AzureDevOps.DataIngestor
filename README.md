# Azure DevOps Data Collector
Collect Azure DevOps data by calling Azure DevOps SDK which calls into their REST API's. The following collectors are implemented:
* Project
* Repository
* PullRequest

Data is collected and inserted directly into SQL Server. 

# Downloads
1. Install tool from https://www.nuget.org/packages/AzureDevOpsDataCollector.Console
1. Run executable for help menu

# Sample Usage
<code>AzureDevOpsDataCollector.Console.exe project --account myaccount --pat MyPersonalAccessToken --connection MySqlServerConnectionString</code>
