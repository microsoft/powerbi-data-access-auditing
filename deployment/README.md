# Azure Data Service Go Fast - Deployment

In this section you will automatically provision all Azure resources required to run ADSGoFast Framework. We will use a pre-defined ARM template with the definition of all Azure services used for a small development environment.

## Azure services

The following Azure services will be deployed in your subscription:

Name                        | Type | Pricing Tier | Pricing Info |
----------------------------|------|--------------|--------------|
ADFIR-*suffix*	                | Virtual machine | Standard D4s v3 (4 vcpus, 16 GiB memory) | https://azure.microsoft.com/en-us/pricing/details/virtual-machines/windows/
ADFIR-*suffix*OsDisk            | Disk | E10 | https://azure.microsoft.com/en-us/pricing/details/managed-disks/
ADFIR-*suffix*NetInt            | Network interface ||
ads-kv-*suffix*                 | Azure KeyVault| Standard | https://azure.microsoft.com/en-au/pricing/details/key-vault/
adsgofast-srv-*suffix*          | SQL server || 
adsgofast                       | Azure SQL Database (Framework Metadata) | S2 DTU50 | https://azure.microsoft.com/en-us/pricing/details/sql-database/single/
AdventureWorksLT                | Azure SQL Database (Sample DB used for testing) | S2 DTU50 | https://azure.microsoft.com/en-us/pricing/details/sql-database/single/
Staging                         | Azure SQL Database (Staging ODS) | S2 DTU100 | https://azure.microsoft.com/en-us/pricing/details/sql-database/single/
adsgofast-vnet                  | Virtual network || https://azure.microsoft.com/en-us/pricing/details/virtual-network/
ADSGoFastADF-*suffix*           | Data factory (V2) | Data pipelines | https://azure.microsoft.com/en-us/pricing/details/data-factory/
appinsights-adsgofast           | Azure Monitor (Application Insights) || https://azure.microsoft.com/en-us/pricing/details/monitor/
azure-bastion-ads-go-fast       | Bastion | | https://azure.microsoft.com/en-au/pricing/details/azure-bastion/
azure-bastion-ads-go-fast-pip   | Public IP address || https://azure.microsoft.com/en-us/pricing/details/ip-addresses/
datalakestg*suffix*             | Azure Data Lake Storage Gen2 | LRS - StorageV2 | https://azure.microsoft.com/en-us/pricing/details/storage/data-lake/
FuncApp-*suffix*                | Azure Functions | Consumption plan | https://azure.microsoft.com/en-au/pricing/details/functions/
hpnf-DM-ADSGoFast-Dev-RG        | App Service plan for Azure Function ||
hpnw-DM-ADSGoFast-Dev-RG        | App Service plan for Web App ||
logstg*suffix*                  | Azure Blob Storage | LRS - StorageV2 | https://azure.microsoft.com/en-us/pricing/details/storage/blobs/

<br>**IMPORTANT**: When you deploy the resources in your own subscription you are responsible for the charges related to the use of the services provisioned. If you don't want any extra charges associated with the resources you should delete the resource group and all resources in it.calc

--------------------------------------
## Requirements

Install and/or update with the latest version for the following:

- PowerShell (https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-7.1)
- Latest Azure CLI (https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- VS Code (https://code.visualstudio.com/download)
- Git (https://git-scm.com/downloads)
- In PowerShell run the following:
  - Install-Module -Name Az -AllowClobber
  - Install-Module AzureADPreview

--------------------------------------
## Deploying and Configuring ADS Go Fast
In this section you will use a PoweShell script to deploy and configure ADS GoFast Framework. It will perform a full deployment and configuration, only requiring for two manual intervation (Azure Function and Web App) and it will ask you a couple of times to sign to Azure.

The deployment and configuration will take around **90 minutes** to complete, as we automated Azure Data Factory Integration RunTime installation and Configuration (including JRE Install).

1. Clone this GitHub Repo

2. Go to folder /ads-go-fast/deploy and execute .\Deploy.ps1

3. In this steps you will need to enter the following information:
    <br>- **Subscription**: [your Azure subscription]
    <br>- **Resource group**: [name of a new Resource Group]
    <br>- **Azure AD User**: [your user name address such as: sergio.zenati@contoso.com]
    <br>- **Service Principal**: [Service Principal name for Azure Function (Used for development)]
    <br>- **Location**: [Azure Data Center name such as, Australia East] 

4. Wait until MSI access grant is completed.

5. Go to Azure Portal, Resource group and open Azure Function:
    <br>- Go to **Authentication / Authorization**
    <br>- Enable **App Service Authentication**
    <br>- Click on **Azure Active Directory**
    <br>- Enable **Express**
    <br>- Leave it as default **Create New Azure AD App**
    <br>- Click Ok and Save

6. Wait until complete the PowerShell script **Deploy.ps1** 

7. In this step, you will need to give API permission for the Azure Function Application:
  - Go to AAD -> App Registrations, find the Azure Function App created on step 5 (Authentication)
  - Go to Manage -> API permissions
  - Go to Ad a permission -> Select 'My APIs' -> Select Azure Function App Name -> Select 'Application permissions' -> Select 'All' under permissions
  - Click Add Permissions

8. Go to Azure Portal, Resource group and open Web App:
    <br>- Update the AppSettings.json with the template provided by the deployment script:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AZURE_TENANT_ID": "" //Azure Tenant Id,
  "AZURE_CLIENT_ID": "" //Service Prinicpal Client Id. Only needed for local development environment,
  "AZURE_CLIENT_SECRET": "" //Service Prinicpal secret. Only needed for local development environment,
  "UseMSI": false ///Set to true for Azure deployments and false for local developement deployments,
  "AdsGoFastTaskMetaDataDatabaseServer": "" //Address of the framework configuration database,
  "AdsGoFastTaskMetaDataDatabaseName": "" //Db name of the framework configuration database,
  "AppInsightsWorkspaceId": "" //Application Insights Workspace Id for the Azure Function Application that you deployed previously. This allows the web applicication to display function activity information for monitoring purposes ,
  "AdGroups": [
  ],
  "AllowedHosts": "*",
  "SecurityModelOptions": {
    "SecurityRoles": [
      {
        "SecurityGroupId": "", //If you want to restrict access to the application create a Security group in Azure AD for Administrators of this application. Put the object Id of that group here.
        "Name": "Administrators",
        "AllowActions": [ //If you want to  restrict access to certain areas of the application so that Admins can only access these areas then move items from the "GlobalAllowActions" list below to here. 
        ]
      }
    ],
    "GlobalAllowActions": [
      "ADFActivityErrors",
      "ADFPipelineStats",
      "AFExecutionSummary",
      "AFLogMonitor",
      "Dashboard",
      "DataFactory",
      "FrameworkTaskRunner",
      "FrameworkTaskRunnerDapper",
      "ReportsAndStatistics",
      "ScheduleInstance",
      "ScheduleMaster",
      "SourceAndTargetSystems",
      "SourceAndTargetSystemsJsonSchema",
      "SubjectArea",
      "TaskGroup",
      "TaskGroupDependency",
      "TaskInstance",
      "TaskInstanceExecution",
      "TaskMaster",
      "TaskMasterWaterMark",
      "TaskType",
      "TaskTypeMapping",
      "Wizards"

    ],
    "GlobalDenyActions": [
      "Customisations.Delete",
      "DataFactory.Delete",
      "FrameworkTaskRunner.Delete",
      "FrameworkTaskRunnerDapper.Delete",
      "ScheduleInstance.Delete",
      "ScheduleMaster.Delete",
      "SourceAndTargetSystems.Delete",
      "SourceAndTargetSystemsJsonSchema.Delete",
      "TaskGroup.Delete",
      "TaskGroupDependency.Delete",
      "TaskGroupDependency.DeletePlus",
      "TaskInstance.Delete",
      "TaskMaster.Delete",
      "TaskMasterWaterMark.Delete",
      "TaskType.Delete",
      "TaskTypeMapping.Delete"
    ]
  },
  // Details below are specificly for AAD integration. You will need to create an App Registration specifically for AAD integration. 
  // Do this using the Azure Portal and enable AAD integration for your web app either using the express settup method or by doing it manually. 
  // Fill in the relevant info below
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "",
    "TenantId": "",
    "ClientId": "",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath ": "/signout-callback-oidc"
  }

}
```
9. In this step, you will need to give API permission for the Web App Service Principal:   
  - Go to AAD -> App Registrations, find the Web App Service Principal
  - Go to Manage -> API permissions
  - Make sure the User.Read permission is added for Azure Active Directory Graph
  - Go to Ad a permission -> Select 'Microsoft Graph' -> Select 'Delegated Permissions' -> Select 'User.Read'

10. Restart Azure Function and WebApp

Well done, the deployment and configuration is completed.

--------------------------------------
## Limitation

- Deployment doesn't support the deployment on existing Azure resources, such as, Resource Group, Log Analytics, Azure SQL and others.
    <br>- If you need to use existing Azure resource you will need to change the ARM Template and Deploy.ps1 script.

