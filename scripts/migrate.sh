#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../api"
dotnet ef database update --project src/HexCatalog.Infrastructure --startup-project src/HexCatalog.Api
echo "HexCatalog migrations applied."
