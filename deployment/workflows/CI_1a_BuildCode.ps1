
######################################################
### Continuous Integration                         ####
######################################################
if (([Environment]::GetEnvironmentVariable("AdsOpts_CI_Enable")) -eq "True")
{
    Write-Host "Starting CI.."

    Invoke-Expression -Command  ".\Steps\CI_BuildWebApp.ps1"
    
    Write-Host "Finishing CI.."
}