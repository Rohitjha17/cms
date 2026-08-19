@echo off
title School CMS
setlocal

set "ROOT=%~dp0"
set "DATA=%ROOT%data"
if not exist "%DATA%" mkdir "%DATA%"
if not exist "%DATA%\uploads" mkdir "%DATA%\uploads"

rem Everything lives in the data folder next to this file. Back it up by copying that folder.
set "ASPNETCORE_ENVIRONMENT=Production"
set "Database__Provider=Sqlite"
set "ConnectionStrings__Sqlite=Data Source=%DATA%\cms.db;Default Timeout=30"
set "Storage__Provider=Local"
set "Storage__LocalRootPath=%DATA%\uploads"
set "Storage__LocalBaseUrl=/uploads"
set "DemoMode__Enabled=true"
set "Seed__EnableDemoData=true"
set "Seed__DemoAdminPassword=Admin@12345"
set "PublicSite__BaseUrl=http://localhost:5301"

rem A port already in use is the most common reason nothing appears, and it is silent
rem otherwise: the application exits immediately and its window is already minimised.
for %%P in (5101 5201 5301) do (
    netstat -ano -p tcp | find "LISTENING" | find ":%%P " >nul 2>&1
    if not errorlevel 1 (
        echo.
        echo   Port %%P on this computer is already being used by another program.
        echo   School CMS cannot start until that program is closed.
        echo.
        echo   If you started School CMS earlier, run Stop.bat first, then try again.
        echo.
        pause
        exit /b 1
    )
)

echo.
echo   Starting School CMS. The first start takes about a minute while the
echo   database is prepared. Leave this window open while you use it.
echo.

rem The console starts first: it is what creates and fills the database.
start "School CMS - console" /min "%ROOT%app\admin\Cms.Admin.exe" --urls http://localhost:5201

rem Wait until it genuinely answers, rather than guessing at a number of seconds.
rem A slow machine on its first run can take well over a minute.
powershell -NoProfile -Command "$ErrorActionPreference='SilentlyContinue'; for ($i=0; $i -lt 150; $i++) { try { $r = Invoke-WebRequest -UseBasicParsing -Uri 'http://localhost:5201/Account/Login' -TimeoutSec 2; if ($r.StatusCode -eq 200) { exit 0 } } catch { }; Start-Sleep -Seconds 1 }; exit 1"

if errorlevel 1 (
    echo.
    echo   School CMS did not finish starting.
    echo.
    echo   Most often this means Windows blocked it. Close this window, right-click
    echo   Cms.Admin.exe in the app\admin folder, choose Properties, tick Unblock
    echo   if it is there, then run Start.bat again.
    echo.
    pause
    exit /b 1
)

rem These two share the same database and must not try to create it again.
set "Seed__SkipStartup=true"
start "School CMS - websites" /min "%ROOT%app\web\Cms.Web.exe" --urls http://localhost:5301
start "School CMS - api" /min "%ROOT%app\api\Cms.Api.exe" --urls http://localhost:5101
timeout /t 8 /nobreak >nul

start http://localhost:5201

echo   Ready.
echo.
echo   Console  : http://localhost:5201    (sign in: admin@demo.local / Admin@12345)
echo   Websites : http://localhost:5301/school    and    /college
echo.
echo   Close this window or run Stop.bat when you are finished.
echo.
pause
