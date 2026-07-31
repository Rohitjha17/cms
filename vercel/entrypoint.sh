#!/bin/sh
set -eu

mkdir -p /tmp/cms-demo/uploads /tmp/cms-demo/home
export HOME=/tmp/cms-demo/home
if [ ! -f /tmp/cms-demo/cms.db ]; then
    cp /app/demo-seed/cms.db /tmp/cms-demo/cms.db
fi

cleanup() {
    kill "${API_PID:-}" "${ADMIN_PID:-}" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

(cd /app/api && ASPNETCORE_URLS=http://127.0.0.1:5101 dotnet Cms.Api.dll) &
API_PID=$!

(cd /app/admin && ASPNETCORE_URLS="http://0.0.0.0:${PORT}" dotnet Cms.Admin.dll) &
ADMIN_PID=$!

wait "$ADMIN_PID"
