Write-Host "Creating Storage Account For Logging"
Write-Host $env:AdsOpts_CD_Services_Storage_Logging_Name
if($env:AdsOpts_CD_Services_Storage_Logging_Enable -eq "True")
{
    #StorageAccount For Logging
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/Storage_Logging.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location storage-log-account-name=$env:AdsOpts_CD_Services_Storage_Logging_Name
    Write-Host "Creating Storage Account For Logging"
}
else 
{
    Write-Host "Skipped Creation of Storage Account For Logging"
}