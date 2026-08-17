#!/bin/sh
set -eu

# /data is a persistent disk. The seed database is copied in ONLY on a first boot;
# on every later deploy the school's real data is left untouched.
# Data Protection keys live inside that database, shared by all three apps.
mkdir -p /data/uploads /data/home
export HOME=/data/home
if [ ! -f /data/cms.db ]; then
    cp /app/demo-seed/cms.db /data/cms.db
fi

cleanup() {
    kill "${NGINX_PID:-}" "${API_PID:-}" "${ADMIN_PID:-}" "${WEB_PID:-}" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

# Which host serves the console decides how traffic is routed. With Platform__Domain set, that
# host keeps the console at / and the websites under /site, and every OTHER host — a school's own
# domain — serves that school's website at its root. Without it, there is no way to tell a school
# domain from the platform's own, so the console stays at the root of every host, as before.
if [ -n "${Platform__Domain:-}" ]; then
    sed "s/__PLATFORM_DOMAIN__/${Platform__Domain}/g" \
        /app/nginx.multi-host.conf.template > /tmp/nginx.conf
    echo "nginx: console on ${Platform__Domain}, other hosts serve their own website at /"
else
    cp /app/nginx.single-host.conf /tmp/nginx.conf
    echo "nginx: Platform__Domain is not set — console at / on every host, websites under /site"
fi

nginx -c /tmp/nginx.conf -g 'daemon off;' &
NGINX_PID=$!

(cd /app/api && ASPNETCORE_URLS=http://127.0.0.1:5101 dotnet Cms.Api.dll) &
API_PID=$!

(cd /app/admin && ASPNETCORE_URLS=http://127.0.0.1:5201 dotnet Cms.Admin.dll) &
ADMIN_PID=$!

(cd /app/web && ASPNETCORE_URLS=http://127.0.0.1:5301 Seed__SkipStartup=true dotnet Cms.Web.dll) &
WEB_PID=$!

wait "$ADMIN_PID"
