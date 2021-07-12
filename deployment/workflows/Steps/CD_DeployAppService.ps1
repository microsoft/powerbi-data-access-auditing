
if ($env:AdsOpts_CD_Services_AppPlans_WebApp_Enable -eq "True")
{
    Write-Host "Creating App Service for Web App"
    #App Service (Includes both functions and web)
    $storageaccountkey = (az storage account keys list -g $env:AdsOpts_CD_ResourceGroup_Name -n $env:AdsOpts_CD_Services_Storage_Logging_Name | ConvertFrom-Json)[0].value

    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/AppService_Web.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location asp_name=$env:AdsOpts_CD_Services_AppPlans_WebApp_Name 
}
else 
{
    Write-Host "Skipped Creation of App Service for Web App"
}
