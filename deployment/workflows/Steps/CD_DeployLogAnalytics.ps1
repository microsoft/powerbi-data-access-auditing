
if ($env:AdsOpts_CD_Services_DataFactory_Enable -eq "True")
{
    Write-Host "Creating Log Analyticss"
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/LogAnalytics.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location workspaces_adsgofastloganalytics_name=$env:AdsOpts_CD_Services_LogAnalytics_Name 
}
else 
{
    Write-Host "Skipped Creation of  Log Analytics"
}
