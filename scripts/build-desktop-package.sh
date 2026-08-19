#!/usr/bin/env bash
# Builds the Windows desktop package: the three applications, compiled for Windows with the
# .NET runtime included, plus the scripts and instructions a non-technical person needs.
#
# The result runs on a Windows machine with nothing installed on it — no .NET, no database,
# no web server — and stores everything in a "data" folder beside the scripts.
#
# Usage:  scripts/build-desktop-package.sh [output-directory]
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="${1:-$ROOT/../SchoolCMS-Desktop}"
STAGE="$OUT/SchoolCMS"

echo "Building into $STAGE"
rm -rf "$STAGE"
mkdir -p "$STAGE/app"

publish() {
    local project="$1" name="$2"
    echo "  publishing $name…"
    dotnet publish "$ROOT/src/$project" \
        --configuration Release \
        --runtime win-x64 \
        --self-contained true \
        --output "$STAGE/app/$name" \
        --verbosity quiet --nologo
}

publish Cms.Admin admin
publish Cms.Web   web
publish Cms.Api   api

cp "$ROOT/desktop/Start.bat" "$ROOT/desktop/Stop.bat" "$ROOT/desktop/READ-ME-FIRST.txt" "$STAGE/"

# Nothing here should carry source or build leftovers to the customer.
find "$STAGE" \( -name '*.pdb' -o -name '*.cs' -o -name '*.csproj' \) -delete

echo "Zipping…"
( cd "$OUT" && rm -f SchoolCMS.zip && zip -qr SchoolCMS.zip SchoolCMS )

echo
echo "Done: $OUT/SchoolCMS.zip"
du -sh "$STAGE" "$OUT/SchoolCMS.zip" | sed 's/^/  /'
