

Write-Host "Creating RG Hash"
$ResourceGroupHash =uniqueString($env:AdsOpts_CD_ResourceGroup_Name)
Write-Host $ResourceGroupHash

PersistEnvVariable -Name "AdsOpts_CD_ResourceGroup_Hash" -Value $ResourceGroupHash
Write-Host "Created RG Hash"
Write-Host "Setting Service Names"
SetServiceName -RootElement "AdsOpts_CD_Services_AzureSQLServer"
SetServiceName -RootElement "AdsOpts_CD_Services_CoreFunctionApp"
SetServiceName -RootElement "AdsOpts_CD_Services_WebSite" 
SetServiceName -RootElement "AdsOpts_CD_Services_AppInsights"
SetServiceName -RootElement "AdsOpts_CD_Services_Storage_Logging"
SetServiceName -RootElement "AdsOpts_CD_Services_Storage_ADLS"
SetServiceName -RootElement "AdsOpts_CD_Services_Storage_Blob"
SetServiceName -RootElement "AdsOpts_CD_Services_DataFactory"
SetServiceName -RootElement "AdsOpts_CD_Services_AppPlans_WebApp"
SetServiceName -RootElement "AdsOpts_CD_Services_AppPlans_FunctionApp"
SetServiceName -RootElement "AdsOpts_CD_Services_LogAnalytics"
SetServiceName -RootElement "AdsOpts_CD_Services_KeyVault"
SetServiceName -RootElement "AdsOpts_CD_ServicePrincipals_DeploymentSP"
SetServiceName -RootElement "AdsOpts_CD_ServicePrincipals_WebAppAuthenticationSP"
SetServiceName -RootElement "AdsOpts_CD_ServicePrincipals_FunctionAppAuthenticationSP"
