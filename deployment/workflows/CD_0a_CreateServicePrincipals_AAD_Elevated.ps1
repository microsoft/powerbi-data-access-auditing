#First Create the Resource Group 
Invoke-Expression -Command  ".\Steps\CD_DeployResourceGroup.ps1"

########################################################################

###      SetUp Service Principals Required.. Need to run this part with elevated privileges

#########################################################################
if($env:AdsOpts_CD_ServicePrincipals_DeploymentSP_Enable -eq "True")
{
    $existcheck = (az ad sp list --display-name $env:AdsOpts_CD_ServicePrincipals_DeploymentSP_Name)
    Write-Host "Creating Deployment Service Principal"
    $subid =  ((az account show -s $env:AdsOpts_CD_ResourceGroup_Subscription) | ConvertFrom-Json).id
    $SP = az ad sp create-for-rbac --name $env:AdsOpts_CD_ServicePrincipals_DeploymentSP_Name --role contributor --scopes /subscriptions/$subid/resourceGroups/$env:AdsOpts_CD_ResourceGroup_Name    
}


$environmentfile = $env:AdsOpts_CD_FolderPaths_Environments + "/" + $env:ENVIRONMENT_NAME + ".json"
$envsettings = Get-Content $environmentfile | ConvertFrom-Json

if($env:AdsOpts_CD_ServicePrincipals_WebAppAuthenticationSP_Enable -eq "True")
{
    Write-Host "Creating WebAppAuthentication Service Principal"
    
    $roleid = [guid]::NewGuid()
    $roles = '[{\"allowedMemberTypes\":  [\"Application\"],\"description\":  \"Administrator\",\"displayName\":  \"Administrator\",\"id\":  \"@Id\",\"isEnabled\":  true,\"lang\":  null,\"origin\":  \"Users\\Groups\",\"value\":  \"Administrator\"}]'
    $roles = $roles.Replace("@Id",$roleid)
    
    $replyurls = "https://$env:AdsOpts_CD_Services_WebSite_Name.azurewebsites.net/signin-oidc"

    $subid =  ((az account show -s $env:AdsOpts_CD_ResourceGroup_Subscription) | ConvertFrom-Json).id
    $appid = ((az ad app create --display-name $env:AdsOpts_CD_ServicePrincipals_WebAppAuthenticationSP_Name --homepage "api://$env:AdsOpts_CD_ServicePrincipals_WebAppAuthenticationSP_Name"  --identifier-uris "api://$env:AdsOpts_CD_ServicePrincipals_WebAppAuthenticationSP_Name" --app-roles $roles --reply-urls $replyurls) | ConvertFrom-Json).appId
    $spid = ((az ad sp create --id $appid) | ConvertFrom-Json).ObjectId

}

#Update the Environment File
$appid = ((az ad app show --id "api://$env:AdsOpts_CD_ServicePrincipals_WebAppAuthenticationSP_Name") | ConvertFrom-Json).appId
$envsettings.AdsOpts.CD.ServicePrincipals.WebAppAuthenticationSP.ClientId = $appid
$envsettings | ConvertTo-Json  -Depth 10 | set-content $environmentfile

