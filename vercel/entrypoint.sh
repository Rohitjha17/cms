#!/bin/sh
set -eu

mkdir -p /tmp/cms-demo/uploads /tmp/cms-demo/home
export HOME=/tmp/cms-demo/home

cleanup() {
    kill "${API_PID:-}" "${ADMIN_PID:-}" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

ASPNETCORE_URLS=http://127.0.0.1:5101 dotnet /app/api/Cms.Api.dll &
API_PID=$!

attempt=0
until curl --fail --silent http://127.0.0.1:5101/swagger/v1/swagger.json >/dev/null; do
    attempt=$((attempt + 1))
    if ! kill -0 "$API_PID" 2>/dev/null || [ "$attempt" -ge 60 ]; then
        echo "CMS API did not become ready." >&2
        exit 1
    fi
    sleep 1
done

ASPNETCORE_URLS=http://127.0.0.1:5201 dotnet /app/admin/Cms.Admin.dll &
ADMIN_PID=$!

attempt=0
until curl --fail --silent http://127.0.0.1:5201/Account/Login >/dev/null; do
    attempt=$((attempt + 1))
    if ! kill -0 "$ADMIN_PID" 2>/dev/null || [ "$attempt" -ge 60 ]; then
        echo "CMS Admin did not become ready." >&2
        exit 1
    fi
    sleep 1
done

envsubst '${PORT}' </etc/nginx/nginx.conf.template >/tmp/nginx.conf
nginx -c /tmp/nginx.conf -g 'daemon off;'
