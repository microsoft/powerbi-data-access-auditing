Write-Host "Creating Storage Account (Blob) For Data Lake"
if($env:AdsOpts_CD_Services_Storage_Blob_Enable -eq "True")
{
    #StorageAccount For Logging
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/Storage_Blob.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location storage-account-name=$env:AdsOpts_CD_Services_Storage_Blob_Name
    Write-Host "Creating Storage Account (Blob) For Data Lake"
}
else 
{
    Write-Host "Skipped Creation of Storage (Blob) For Data Lake"
}