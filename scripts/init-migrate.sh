#!/usr/bin/env bash
# Adds a new EF Core migration for DeadlockDbContext.
# Usage: scripts/init-migrate.sh [migration-name]
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/../src/Voidwell.Deadlock.Data"

export ASPNETCORE_ENVIRONMENT=Development

migration="${1:-deadlockdbcontext.$(date +%Y_%m_%d_%H_%M_%S)}"

dotnet ef migrations add "$migration" -v \
    -c Voidwell.Deadlock.Data.DeadlockDbContext \
    -o ./Migrations
