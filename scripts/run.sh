#!/usr/bin/env bash
# SQL Server, API (http://localhost:5090/swagger) and admin UI (http://localhost:5152).
set -euo pipefail
cd "$(dirname "$0")/.."
docker compose -f infra/docker-compose.yml up --build "$@"
