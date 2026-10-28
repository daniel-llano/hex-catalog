$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..\api
dotnet ef database update --project src/HexCatalog.Infrastructure --startup-project src/HexCatalog.Api
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "HexCatalog migrations applied."
