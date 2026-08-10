#!/bin/sh
set -eu

mkdir -p /tmp/cms-demo/uploads /tmp/cms-demo/home/.aspnet/DataProtection-Keys
export HOME=/tmp/cms-demo/home
if [ ! -f /tmp/cms-demo/cms.db ]; then
    cp /app/demo-seed/cms.db /tmp/cms-demo/cms.db
fi
cp /app/demo-seed/dataprotection/*.xml /tmp/cms-demo/home/.aspnet/DataProtection-Keys/

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
