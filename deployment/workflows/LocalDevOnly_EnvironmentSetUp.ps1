. .\Steps\PushEnvFileIntoVariables.ps1
ParseEnvFile("$env:Secrets_ENVIRONMENT_NAME")
ParseSecretsFile
Invoke-Expression -Command  ".\Steps\CD_SetResourceGroupHash.ps1"
