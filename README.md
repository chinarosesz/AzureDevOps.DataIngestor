# Azure DevOps Data Ingestor
A console application that calls Azure DevOps client libraries and ingests data into a specified SQL Server database. The 
following ingestors are available and can be specified from the command line.
* Project
* Repository
* PullRequest
* Commit
* BuildDefinition

# Downloads
Install tool from https://www.nuget.org/packages/AzureDevOps.DataIngestor

# Usage
The console application can be executed by calling <code>AzureDevOps.DataIngestor.exe</code>. The help menu shows up if you 
give it no parameters. The mininum requirements when running <code>AzureDevOps.Ingestor.exe</code> are the name of your 
organization, Azure DevOps personal access token, SQL connection string, and which data ingestor you want to run.

<pre>
c:\>AzureDevOps.DataIngestor.exe --help
AzureDevOps.DataIngestor 1.0.7
Copyright (C) 2020 https://github.com/chinarosesz/AzureDevOps.DataIngestor

  project            Collect projects data given a specific project or all projects by default

  repository         Collect repository data given a specific project or all projects by default

  pullrequest        Collect pull request data given a specific project or all projects by default
  
  commit             Collect commit data given a specific project or all projects by default

  builddefinition    Collect bulid definition data given a specific project or all projects by default

  help               Display more information on a specific command.

  version            Display version information.
</pre>

Both SQL connection string and personal access token can set as enviornment variables so you don't have to supply it from the
command line. Below is an example command with Azure DevOps personal access token and sql connection string already set as an enviornment 
variable.
<pre>
SET VssPersonalAccessToken=MyPersonalAccessTokenString
SET SqlServerConnectionString="My Sql Server Connection String Can Contain Space"
AzureDevOps.DataIngestor.exe repository --organization MyOrg</pre>

Example with personal access token
<pre>AzureDevOps.DataIngestor.exe repository --organization MyOrg --pat MyPersonalAccessToken</pre>

Example with SQL connection string being passed in and personal access token set as an environment variable
<pre>
SET VssPersonalAccessToken=MyPersonalAccessTokenString
AzureDevOps.DataIngestor.exe repository --organization MyOrg --sqlserverconnectionstring MySqlServerConnectionString
</pre>

Example with SQL connection string and personal access token being passed in
<pre>AzureDevOps.DataIngestor.exe repository --organization MyOrg --pat MyPersonalAccessToken --sqlserverconnectionstring MySqlServerConnectionString</pre>
 
### Project
To collect projects data from Azure DevOps.
<pre>AzureDevOps.DataIngestor.exe project --organization MyOrg</pre>

### Build 
Collect build data from Azure DevOps for all projects, param <code>--projects</code> is not required.
<pre>AzureDevOps.Ingestor.exe build --organization MyOrg</pre>

Collect build data from Azure DevOps given a list of projects
<pre>AzureDevOps.Ingestor.exe build --organization MyOrg --projects project1:project2</pre>

### Build Definition
Collect build definition data from Azure DevOps for all projects, param <code>--projects</code> is not required.
<pre>AzureDevOps.Ingestor.exe builddefinition --organization MyOrg</pre>

Collect build definition data from Azure DevOps given a list of projects
<pre>AzureDevOps.Ingestor.exe builddefinition --organization MyOrg --projects project1:project2</pre>

### Pull Request
Pull requests are collected by going back one month. This ingestor only collects completed and active pull requests.
Currently there is no support for abandoned pull requests. Once the ingestor is finished running, a watermark is updated to
the most recent run date, and the next time this ingestor gets called again, it will not collect the same pull requests.
<pre>AzureDevOps.DataIngestor.exe pullrequest --organization MyOrg --projects MyProject</pre>

### Commits
Commits are collected by going back one month. Once the ingestor is finished running, a watermark is updated to
the most recent run date, and the next time this ingestor gets called again, it will not collect the same commits.
<pre>AzureDevOps.DataIngestor.exe commit --organization MyOrg --projects MyProject</pre>

### Repository
Collect all repositories from the whole organization.
<pre>AzureDevOps.Ingestor.exe repository --organization MyOrg</pre>

Collect all repositories for a selected set of projects
<pre>AzureDevOps.Ingestor.exe repository --organization MyOrg --projects project1:project2</pre>

# Support
If you have any questions/feedback/suggestions. Feel free to create an issue or submit a pull request. Additonal ingestors 
are added upon requests.

# Contribution
This is an open source project, feel free to create a PR and I'll review the changes. 

