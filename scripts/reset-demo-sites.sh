#!/usr/bin/env bash
# Deletes the four demo websites so the application rebuilds them on its next start.
#
# They are seeded once and then left alone, so that editing one in the console is not undone
# by a restart. That also means a change to the seed data does not reach them until the old
# ones are gone, which is what this does.
#
# Local SQLite only. See docs/DEMO_SITES.md.
set -euo pipefail

DB="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/shared/cms-local.db}"

if [ ! -f "$DB" ]; then
    echo "No database at $DB" >&2
    exit 1
fi

KEYS="'prestige','campus','bulletin','atrium'"

sqlite3 "$DB" "
DELETE FROM MenuItems      WHERE SiteId IN (SELECT Id FROM Sites WHERE SiteKey IN ($KEYS));
DELETE FROM Menus          WHERE SiteId IN (SELECT Id FROM Sites WHERE SiteKey IN ($KEYS));
DELETE FROM Pages          WHERE SiteId IN (SELECT Id FROM Sites WHERE SiteKey IN ($KEYS));
DELETE FROM HomePageSections WHERE SiteId IN (SELECT Id FROM Sites WHERE SiteKey IN ($KEYS));
DELETE FROM ContentEntries WHERE SiteId IN (SELECT Id FROM Sites WHERE SiteKey IN ($KEYS));
DELETE FROM SeoSettings    WHERE SiteId IN (SELECT Id FROM Sites WHERE SiteKey IN ($KEYS));
DELETE FROM Sites          WHERE SiteKey IN ($KEYS);
"

echo "Removed the four demo websites. Restart the application to rebuild them."
