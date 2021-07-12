#Move From Workflows to Function App
$CurrentPath = (Get-Location).Path
Set-Location "..\..\powerbiauditapp"
dotnet restore
dotnet publish --no-restore --configuration Release --output '..\Deployment\bin\publish\unzipped\powerbiauditapp\'
#Move back to workflows 
Set-Location $CurrentPath
Set-Location "../bin/publish"
$Path = (Get-Location).Path + "/zipped/powerbiauditapp" 
New-Item -ItemType Directory -Force -Path $Path
$Path = $Path + "/Publish.zip"
Compress-Archive -Path '.\unzipped\powerbiauditapp\*' -DestinationPath $Path -force
#Move back to workflows 
Set-Location $CurrentPath