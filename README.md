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
give it no parameters. The mininum requirements when running AzureDevOps.Ingestor.exe are the name of your organization, 
Azure DevOps personal access token, SQL connection string, and which data ingestor you want to run.

Both SQL connection string and personal access token can set from enviornment variable so you don't have to supply it from the
command line. This is also documented when you execute the tool's help menu.

Here is an example command where I have already set personal access token and sql connection string as an enviornment variable.
> <code>AzureDevOps.DataIngestor.exe repository --organization MyOrg</code>

Here is an example command with personal access token
> <code>AzureDevOps.DataIngestor.exe repository --organization MyOrg --pat MyPersonalAccessToken</code>

Here is an example command with SQL connection string being passed in and personal access token set as an environment variable
> <code>AzureDevOps.DataIngestor.exe repository --organization MyOrg --sqlserverconnectionstring MySqlServerConnectionString</code>

An example command with SQL connection string and personal access token being passed in
> <code>AzureDevOps.DataIngestor.exe repository --organization MyOrg --pat MyPersonalAccessToken --sqlserverconnectionstring MySqlServerConnectionString</code>

The below usage demonstrates how to call each ingestor. These examples already have personal access token and sql connection 
string set as environment variables so they don't have to be passed in to the command line.
 
#### Project
To collect projects data from Azure DevOps.
> <code>AzureDevOps.DataIngestor.exe project --organization MyOrg</code>

#### Build Definition
Collect build definition data from Azure DevOps for all projects, param <code>--projects</code> is not required.
> <code>AzureDevOps.Ingestor.exe builddefinition --organization MyOrg</code>

To collect build definition data from Azure DevOps given a list of projects
> <code>AzureDevOps.Ingestor.exe builddefinition --organization MyOrg --projects project1:project2</code>

#### Pull Request
Pull requests are collected by going back one month. This ingestor only collects completed and active pull requests.
Currently there is no support for abandoned pull requests. Once the ingestor is finished running, a watermark is updated to
the most recent run date, and the next time this ingestor gets called again, it will not collect the same pull requests.
> <code>AzureDevOps.DataIngestor.exe pullrequest --organization MyOrg --projects MyProject</code>

### Repository
Collect all repositories from the whole organization.
> <code>AzureDevOps.Ingestor.exe repository --organization MyOrg</code>

Collect all repositories for a selected set of projects
> <code>AzureDevOps.Ingestor.exe repository --organization MyOrg --projects project1:project2</code>

# Support
If you have any questions/feedback/suggestions. Feel free to create an issue or submit a pull request. Additonal ingestors 
are added upon requests.

# Contribution
This is an open source project, feel free to create a PR for any changes you wish to make. 

