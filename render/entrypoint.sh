#!/bin/sh
set -eu

# Render mounts a persistent disk at /data. Everything that must survive a restart or a
# redeploy lives there: the database, uploaded media, and the Data Protection home.
DATA_DIR=${DATA_DIR:-/data}
mkdir -p "$DATA_DIR/uploads" "$DATA_DIR/home"
export HOME="$DATA_DIR/home"

# The image ships a database already built from this build's schema. If the disk is empty,
# or the schema in the image differs from the one on disk, the seed replaces it.
#
# Without this check a redeploy that adds a column would leave the old file in place and
# every page would fail with "no such column" — the schema is created by EnsureCreated,
# which never alters an existing database.
IMAGE_SCHEMA=$(cat /app/demo-seed/schema-version 2>/dev/null || echo "unknown")
DISK_SCHEMA=$(cat "$DATA_DIR/.schema-version" 2>/dev/null || echo "none")

if [ ! -f "$DATA_DIR/cms.db" ]; then
    echo "No database on the disk yet — installing the seeded demo database."
    cp /app/demo-seed/cms.db "$DATA_DIR/cms.db"
    printf '%s' "$IMAGE_SCHEMA" > "$DATA_DIR/.schema-version"
elif [ "$IMAGE_SCHEMA" != "$DISK_SCHEMA" ]; then
    echo "Schema changed ($DISK_SCHEMA -> $IMAGE_SCHEMA). Backing up and reseeding."
    mv "$DATA_DIR/cms.db" "$DATA_DIR/cms.db.backup-$(date +%Y%m%d%H%M%S)"
    cp /app/demo-seed/cms.db "$DATA_DIR/cms.db"
    printf '%s' "$IMAGE_SCHEMA" > "$DATA_DIR/.schema-version"
else
    echo "Reusing the existing database on the persistent disk."
fi

# Render supplies the port to listen on.
export PORT=${PORT:-10000}
envsubst '${PORT}' < /app/nginx.conf.template > /etc/nginx/nginx.conf

cleanup() {
    kill "${NGINX_PID:-}" "${API_PID:-}" "${ADMIN_PID:-}" "${WEB_PID:-}" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

nginx -c /etc/nginx/nginx.conf -g 'daemon off;' &
NGINX_PID=$!

(cd /app/api && ASPNETCORE_URLS=http://127.0.0.1:5101 dotnet Cms.Api.dll) &
API_PID=$!

(cd /app/admin && ASPNETCORE_URLS=http://127.0.0.1:5201 dotnet Cms.Admin.dll) &
ADMIN_PID=$!

(cd /app/web && ASPNETCORE_URLS=http://127.0.0.1:5301 PathBase=/site Seed__SkipStartup=true dotnet Cms.Web.dll) &
WEB_PID=$!

wait "$ADMIN_PID"
