#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
dotnet restore Cms.sln
dotnet build Cms.sln -c Release
echo "Build succeeded."
