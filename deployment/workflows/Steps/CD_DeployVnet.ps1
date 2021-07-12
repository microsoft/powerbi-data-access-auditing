SetServiceName -RootElement "AdsOpts_CD_Services_Vnet"
Write-Host "Creating Vnet"
Write-Host $env:AdsOpts_CD_Services_Vnet_Name
if($env:AdsOpts_CD_Services_Vnet_Enable -eq "True")
{
    #StorageAccount For Logging
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/Networking.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location vnet-name=$env:AdsOpts_CD_Services_Vnet_Name
    Write-Host "Creating Vnet"
}
else 
{
    Write-Host "Skipped Creation of Vnet"
}