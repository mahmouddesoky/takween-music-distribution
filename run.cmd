@echo off
rem Double-click to start the API (5080) and the Angular frontend (4200).
rem Options are passed through, e.g.:  run.cmd -Fresh -Test
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1" %*
if errorlevel 1 pause
