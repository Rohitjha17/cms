@echo off
title Stopping School CMS
taskkill /F /IM Cms.Admin.exe >nul 2>&1
taskkill /F /IM Cms.Web.exe   >nul 2>&1
taskkill /F /IM Cms.Api.exe   >nul 2>&1
echo School CMS has been stopped. Your data is kept in the "data" folder.
timeout /t 3 /nobreak >nul
