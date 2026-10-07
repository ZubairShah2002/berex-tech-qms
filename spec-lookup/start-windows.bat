@echo off
rem Product Specification Lookup - start on this PC (office network).
rem Double-click this file. Keep the window open while people use the app.
title Product Specification Lookup
cd /d "%~dp0"

where node >nul 2>nul
if errorlevel 1 (
  echo Node.js is not installed.
  echo Install the "LTS" version from https://nodejs.org , then double-click this file again.
  start https://nodejs.org/
  pause
  exit /b 1
)

if not exist "server\dist\index.js" (
  echo First start: installing and building. This needs internet and takes a few minutes...
  pushd web && call npm ci && call npm run build && popd || goto :failed
  pushd server && call npm ci && call npm run build && popd || goto :failed
)

set NODE_ENV=production
if "%PORT%"=="" set PORT=8080
node server\dist\index.js
pause
exit /b 0

:failed
echo Setup failed. Check the internet connection and try again.
pause
exit /b 1
