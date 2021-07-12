. .\Steps\PushEnvFileIntoVariables.ps1
ParseSecretsFile("")
ParseEnvFile("$env:Secrets_ENVIRONMENT_NAME")
Invoke-Expression -Command  ".\Steps\CD_SetResourceGroupHash.ps1"
