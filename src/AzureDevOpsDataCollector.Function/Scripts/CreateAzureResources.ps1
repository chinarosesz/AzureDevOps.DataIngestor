# This script automates creation of Azure resources required to execute Azure DevOps Data Collector service
# $serviceName: Make sure you pass in a name that exists for all resources so something unique, for example vsodatacollect1
# Todo: Need to add name check to ensure name doesn't exist already before running script
# Sample command line: .\CreateAzureResources.ps1 -subscriptionName "SubscriptionName" -serviceName "azuredevopsdatacollector" -sqlPassword "SomeReallyGoodPassword" -vssPersonalAccessToken "AzureDevOpsPersonalAccessToken"
param
(
	[Parameter(Mandatory=$true, HelpMessage="Azure subscription name")][string]$subscriptionName,
    [Parameter(Mandatory=$true, HelpMessage="Name of your data collector")][string]$serviceName,
    [Parameter(Mandatory=$true, HelpMessage="SQL Server password for the SQL account")][string]$sqlPassword,
    [Parameter(Mandatory=$true, HelpMessage="Azure DevOps Personal Access Token used to collect data")][string]$vssPersonalAccessToken,
    [Parameter(Mandatory=$false, HelpMessage="Region for your data collectdor to run in")][string]$region = "westus2"
)

$ErrorActionPreference = 'Stop'

function Write-Title($title)
{
    Write-Host
    Write-Host "$title" -ForegroundColor Green
}

function Login
{
    Write-Title "Logging in"
    $azureContext = Get-AzContext
    Write-Host "The context is $($azureContext.Subscription.Name)"
    if ($azureContext.Subscription.Name -ne $subscriptionName)
    {
        Write-Host "User is not connected to an Azure account or the context is different than subscription $subscriptionName. Connecting to the subscription manually"
        Connect-AzAccount -Subscription $subscriptionName
    }
}

function RunChecksBeforeCreation
{
    Write-Title "Validating parameters"

    $errorMessage = "$serviceName already exists, rerun this script and choose a different name"

    Write-Host "Validate Resource Group $serviceName doesn't exist"
    if ((Get-AzResourceGroup -Name $serviceName -ErrorAction Ignore) -ne $null)
    {
        throw "Resource group $errorMessage"
    }

    Write-Host "Validate Storage Account $serviceName doesn't exist"
    if ((Get-AzStorageAccount -Name $serviceName -ResourceGroupName $serviceName -ErrorAction Ignore) -ne $null)
    {
        throw "Storage Account $errorMessage"
    }

    Write-Host "Validate Application Insights $serviceName doesn't exist"
    if ((Get-AzApplicationInsights -Name $serviceName -ResourceGroupName $serviceName -ErrorAction Ignore) -ne $null)
    {
        throw "Appliation Insights $errorMessage"
    }

    Write-Host "Validate SQL Server $serviceName doesn't exist"
    if ((Get-AzSqlServer -ServerName $serviceName) -ne $null)
    {
        throw "SQL Server $errorMessage"
    }

    Write-Host "Validate SQL Database $serviceName doesn't exist"
    if ((Get-AzSqlDatabase -ServerName $serviceName -ResourceGroupName $serviceName -DatabaseName $serviceName -ErrorAction Ignore) -ne $null)
    {
        throw "SQL Database $errorMessage"
    }

    Write-Host "Validate App Service Plan $serviceName doesn't exist"
    if ((Get-AzAppServicePlan -Name $serviceName -ResourceGroupName $serviceName -ErrorAction Ignore) -ne $null)
    {
        throw "App Service Plan $errorMessage"
    }

    Write-Host "Validate Function App $serviceName doesn't exist"
    if ((Get-AzFunctionApp -Name $serviceName -ResourceGroupName $serviceName) -ne $null)
    {
        throw "Function App $errorMessage"
    }
}

function CreateResourceGroup
{
    Write-Title "Create Resource Group $serviceName"
    New-AzResourceGroup -Name $serviceName -Location $region
}

function CreateStorageAccount
{
    Write-Title "Create Storage account $serviceName"
    New-AzStorageAccount -ResourceGroupName $serviceName -AccountName $serviceName -Location $region -SkuName Standard_GRS
}

function CreateApplicationInsights
{
    Write-Title "Create Application Insights $serviceName"
    New-AzApplicationInsights -Location $region -Name $serviceName -ResourceGroupName $serviceName
}

function CreateSqlServerAndDatabase
{
    Write-Title "Create SQL Server and Database $serviceName"
    
    Write-Host "Creating SQL Server"
    $password = ConvertTo-SecureString $sqlPassword -AsPlainText -Force
    $creds =  New-Object -TypeName PSCredential -ArgumentList $serviceName, $password
    New-AzSqlServer -Location $region -ResourceGroupName $serviceName -ServerName $serviceName -SqlAdministratorCredentials $creds

    Write-Host "Creating Firewall to allow Azure services and resources to access this server"
    New-AzSqlServerFirewallRule -ResourceGroupName $serviceName -ServerName $serviceName -AllowAllAzureIPs

    Write-Host "Creating SQL Database"
    New-AzSqlDatabase -DatabaseName $serviceName -ResourceGroupName $serviceName -ServerName $serviceName
}

function CreateAppServicePlan
{
    Write-Title "Create app service plan $serviceName"
    New-AzAppServicePlan -Location $region -Name $serviceName -ResourceGroupName $serviceName -Tier Basic
}

function CreateFunctionApp
{
    Write-Title "Create Function App $serviceName"
    $sqlConnectionString = "Server=tcp:$serviceName.database.windows.net;Database=$serviceName;User ID=$serviceName;Password=$sqlPassword;Trusted_Connection=False;Encrypt=True;"
    New-AzFunctionApp -Name $serviceName -PlanName $serviceName -ResourceGroupName $serviceName -Runtime DotNet -StorageAccountName $serviceName -ApplicationInsightsName $serviceName -AppSetting @{"VssPersonalAccessToken"="$vssPersonalAccessToken"; "SqlConnectionString"="$sqlConnectionString"} -IdentityType SystemAssigned -OSType Windows -FunctionsVersion 3 -RuntimeVersion 3
}

# Main
Login
RunChecksBeforeCreation
CreateResourceGroup
CreateAppServicePlan
CreateApplicationInsights
CreateStorageAccount
CreateSqlServerAndDatabase
CreateFunctionApp

Write-Title "Infrastructure created successfully. You can find all resources in resource group $serviceName"