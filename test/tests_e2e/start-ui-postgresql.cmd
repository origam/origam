@echo off
node "%~dp0runUi.js" postgresql %*
exit /b %errorlevel%
