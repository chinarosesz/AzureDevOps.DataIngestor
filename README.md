# Azure DevOps Data Ingestor
A console application that calls Azure DevOps client libraries and ingests data into a specified SQL Server database. The 
following ingestors are available and can be specified from the command line.
* Project
* Repository
* PullRequest
* BuildDefinition

# Downloads
Install tool from https://www.nuget.org/packages/AzureDevOps.DataIngestor

# Usage
The console application can be executed by calling <code>AzureDevOps.DataIngestor.exe</code>. The help menu shows up if you 
give it no parameters. Below are some usage examples.

#### Project
To collect projects data from Azure DevOps.
> <code>AzureDevOps.DataIngestor.exe project --organization MyOrg --pat MyPersonalAccessToken --sqlserverconnectionstring MySqlServerConnectionString</code>

#### Build Definition
To collect build definition data from Azure DevOps for all projects, param <code>--projects</code> is not required.
> <code>AzureDevOps.Ingestor.exe builddefinition --organization MyOrg --pat MyPersonalAccessToken --connection MySqlServerConnectionString</code>

To collect build definition data from Azure DevOps given a list of projects
> <code>AzureDevOps.Ingestor.exe builddefinition --organization MyOrg --projects project1:project2 --pat MyPersonalAccessToken --connection MySqlServerConnectionString</code>


