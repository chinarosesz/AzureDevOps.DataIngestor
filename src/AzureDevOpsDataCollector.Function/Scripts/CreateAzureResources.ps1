# This script automates creation of Azure resources required to execute Azure DevOps Data Collector service
# $serviceName: Make sure you pass in a name that exists for all resources so something unique, for example vsodatacollect1
# Todo: Need to add name check to ensure name doesn't exist already before running script

param
(
	[Parameter(Mandatory=$true)][string]$subscriptionName = "Name of subscription to create resources in",
    [Parameter(Mandatory=$true)][string]$serviceName = "Name of data collector - for exampe: vsodataingest1",
    [Parameter(Mandatory=$true)][string]$region = "westus2",
    [Parameter(Mandatory=$true)][string]$sqlPassword = "Sql password to be created",
    [Parameter(Mandatory=$true)][string]$vssPersonalAccessToken = "InsertYourPersonalAccessToken"
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

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

function CreateResourceGroup
{
    Write-Title "Create resource group $serviceName if it doesn't exist"
    if ((Get-AzResourceGroup -Name $serviceName -ErrorAction SilentlyContinue) -eq $null)
    {
        Write-Host "Creating resource group"
        New-AzResourceGroup -Name $serviceName -Location $region
    }
}

function CreateStorageAccount
{
    Write-Title "Create storage account $serviceName if it doesn't exist"
    if ((Get-AzStorageAccount -Name $serviceName -ResourceGroupName $serviceName -ErrorAction SilentlyContinue) -eq $null)
    {
        New-AzStorageAccount -ResourceGroupName $serviceName -AccountName $serviceName -Location $region -SkuName Standard_GRS
    }
}

function CreateApplicationInsights
{
    Write-Title "Create application insights $serviceName if it doesn't exist"
    if ((Get-AzApplicationInsights -Name $serviceName -ResourceGroupName $serviceName -ErrorAction SilentlyContinue) -eq $null)
    {
        Write-Host "Creating application insights"
        New-AzApplicationInsights -Location $region -Name $serviceName -ResourceGroupName $serviceName
    }
}

function CreateSqlServer
{
    $password = ConvertTo-SecureString $sqlPassword -AsPlainText -Force
    $creds =  New-Object -TypeName PSCredential -ArgumentList $serviceName, $password

    Write-Title "Create SQL Server $serviceName if it doesn't exist"

    if ((Get-AzSqlServer -ServerName $serviceName) -eq $null)
    {
        New-AzSqlServer -Location $region -ResourceGroupName $serviceName -ServerName $serviceName -SqlAdministratorCredentials $creds
    }
    if ((Get-AzSqlDatabase -ServerName $serviceName -ResourceGroupName $serviceName -DatabaseName $serviceName -ErrorAction SilentlyContinue) -eq $null)
    {
        New-AzSqlDatabase -DatabaseName $serviceName -ResourceGroupName $serviceName -ServerName $serviceName
    }
}

function CreateAppServicePlan
{
    Write-Title "Create app service plan $serviceName if it doesn't exist"
    if ((Get-AzAppServicePlan -Name $serviceName -ResourceGroupName $serviceName) -eq $null)
    {
        New-AzAppServicePlan -Location $region -Name $serviceName -ResourceGroupName $serviceName -Tier Basic
    }
}

function CreateFunctionApp
{
    Write-Title "Create function app $serviceName if it doesn't exist"

    if ((Get-AzFunctionApp -Name $serviceName -ResourceGroupName $serviceName) -eq $null)
    {
        Write-Host "Creating function app"
        New-AzFunctionApp -Name $serviceName `
                          -PlanName $serviceName `
                          -ResourceGroupName $serviceName `
                          -StorageAccountName $serviceName `
                          -FunctionsVersion 3 `
                          -OSType Windows `
                          -RuntimeVersion 3 `
                          -RunTime DotNet
    }

    Write-Host "Updating function app to use application insights"
    Update-AzFunctionApp -Name $serviceName -ResourceGroupName $serviceName -ApplicationInsightsName $serviceName -IdentityType SystemAssigned

    Write-Host "Updating function app to store connection string"
    $sqlConnectionString = "Server=tcp:$serviceName.database.windows.net;Database=$serviceName;User ID=$serviceName;Password=$sqlPassword;Trusted_Connection=False;Encrypt=True;"
    Update-AzFunctionAppSetting -AppSetting @{ "SqlConnectionString" =  $sqlConnectionString } -Name $serviceName -ResourceGroupName $serviceName

    Write-Host "Updating function app to store personal access token"
    Update-AzFunctionAppSetting -AppSetting @{ "VssPersonalAccessToken" =  $vssPersonalAccessToken } -Name $serviceName -ResourceGroupName $serviceName
}

# Main
Login
CreateResourceGroup
CreateAppServicePlan
CreateApplicationInsights
CreateStorageAccount
CreateSqlServer
CreateFunctionApp