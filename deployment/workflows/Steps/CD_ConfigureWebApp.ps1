Write-Host "Configuring Web App"

$SourceFile = $env:AdsOpts_CD_FolderPaths_PublishZip + "/powerbiauditapp/Publish.zip"
if($env:AdsOpts_CD_Services_WebSite_Enable -eq "True")
{

    #Update App Settings
    $appsettingsfile = $env:AdsOpts_CD_FolderPaths_PublishUnZip + "/powerbiauditapp/appsettings.json"
    $appSettings = Get-Content $appsettingsfile | ConvertFrom-Json
    $appSettings.AzureAd.AuthorityUri = "https://login.microsoftonline.com/{tenant}/".Replace("{tenant}", $env:Secrets_PowerBISP_TenantId)
    $appSettings.AzureAd.TenantId =  $env:Secrets_PowerBISP_TenantId
    $appSettings.AzureAd.ClientId =  $env:Secrets_PowerBISP_ClientId
    $appSettings.AzureAd.ClientSecret = $env:Secrets_PowerBISp_ClientSecret

    $appSettings.AzureAd2.Domain = "{domain}/".Replace("{domain}",$env:AdsOpts_CD_ResourceGroup_Domain)
    $appSettings.AzureAd2.TenantId = $env:AdsOpts_CD_ResourceGroup_TenantId
    $appSettings.AzureAd2.ClientId = $env:Secrets_WebAppAuthenticationSP_ClientId

    foreach ($item in  $appSettings.PowerBI.Reports) {
        $item.WorkspaceId = $env:AdsOpts_PBIOpts_SampleWorkSpaceId
        $item.ReportId = $item.ReportId.Replace("{SampleReportId1}", $env:AdsOpts_PBIOpts_SampleReportId1).Replace("{SampleReportId2}", $env:AdsOpts_PBIOpts_SampleReportId2)
    }

    $appSettings | ConvertTo-Json  -Depth 10 | set-content $appsettingsfile

    #Repack WebApp
    $CurrentPath = Get-Location
    Set-Location "./../bin/publish"
    $Path = (Get-Location).Path + "/zipped/powerbiauditapp" 
    New-Item -ItemType Directory -Force -Path $Path
    $Path = $Path + "/Publish.zip"
    Compress-Archive -Path '.\unzipped\powerbiauditapp\*' -DestinationPath $Path -force
    #Move back to workflows 
    Set-Location $CurrentPath
    
    # Deploy Web App
    az webapp deployment source config-zip --resource-group $env:AdsOpts_CD_ResourceGroup_Name --name $env:AdsOpts_CD_Services_WebSite_Name --src $SourceFile

    #Enable App Insights
    #az resource create --resource-group $env:AdsOpts_CD_ResourceGroup_Name --resource-type "Microsoft.Insights/components" --name $env:AdsOpts_CD_Services_WebSite_Name --location $env:AdsOpts_CD_ResourceGroup_Location --properties '{\"Application_Type\":\"web\"}'

}
else 
{
    Write-Host "Skipped Configuring Web App"
}