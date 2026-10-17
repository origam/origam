@echo off
node "%~dp0runUi.js" mssql %*
exit /b %errorlevel%
