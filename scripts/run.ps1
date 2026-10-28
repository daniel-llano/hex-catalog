# SQL Server, API (http://localhost:5090/swagger) and admin UI (http://localhost:5152).
Set-Location (Join-Path $PSScriptRoot "..")
docker compose -f infra/docker-compose.yml up --build @args
