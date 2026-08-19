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

echo.
echo   Starting School CMS. The first start takes about a minute while the
echo   database is created. Leave this window open while you use it.
echo.

rem The console starts first because it is what creates and fills the database.
start "School CMS - console" /min "%ROOT%app\admin\Cms.Admin.exe" --urls http://localhost:5201
timeout /t 25 /nobreak >nul

rem The other two share that same database and must not try to create it again.
set "Seed__SkipStartup=true"
start "School CMS - websites" /min "%ROOT%app\web\Cms.Web.exe" --urls http://localhost:5301
start "School CMS - api" /min "%ROOT%app\api\Cms.Api.exe" --urls http://localhost:5101
timeout /t 8 /nobreak >nul

start http://localhost:5201

echo   Console  : http://localhost:5201    (sign in: admin@demo.local / Admin@12345)
echo   Websites : http://localhost:5301/school    and    /college
echo.
echo   Close this window or run Stop.bat when you are finished.
echo.
pause
