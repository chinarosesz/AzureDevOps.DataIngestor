# Azure DevOps Data Collector
Collect Azure DevOps data by calling Azure DevOps SDK which calls into their REST API's. The following collectors are implemented:
* Project
* Repository
* PullRequest
* BuildDefinition

Data is collected and inserted directly into SQL Server. List of represented in SQL tables can be found from these collectors:
* VssProject
* VssRepository
* VssPullRequest
* VssBuildDefinition
* VssBuildDefinitionStep

# Downloads
1. Install tool from https://www.nuget.org/packages/AzureDevOpsDataCollector.Console
1. Run executable for help menu

# Sample Usage
Collects project from Azure DevOps
* <code>AzureDevOpsDataCollector.Console.exe project --account myaccount --pat MyPersonalAccessToken --connection MySqlServerConnectionString</code>

Collects build definition from Azure DevOps given a list of projects or for all projects
* <code>AzureDevOpsDataCollector.Console.exe builddefinition --account myaccount --projects project1:project2 --pat MyPersonalAccessToken --connection MySqlServerConnectionString</code>
* <code>AzureDevOpsDataCollector.Console.exe builddefinition --account myaccount --pat MyPersonalAccessToken --connection MySqlServerConnectionString</code>

