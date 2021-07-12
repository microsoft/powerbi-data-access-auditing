
######################################################
### Continuous Deployment                         ####
######################################################Write-Host ([Environment]::GetEnvironmentVariable("AdsOpts_CI_Enable"))
if (([Environment]::GetEnvironmentVariable("AdsOpts_CD_Enable")) -eq "True")
{
    Write-Host "Starting CD.."

    #Invoke-Expression -Command  ".\Steps\CD_DeployKeyVault.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployStorageForLogging.ps1"

    #Invoke-Expression -Command  ".\Steps\CD_DeployAppInsights.ps1"

    #Invoke-Expression -Command  ".\Steps\CD_DeployLogAnalytics.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployAppService.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployWebSite.ps1"

    #Invoke-Expression -Command  ".\Steps\CD_DeployVnet.ps1"

    Write-Host "Finishing CD.."
}

 #Invoke-Expression -Command  ".\Cleanup_RemoveAll.ps1"