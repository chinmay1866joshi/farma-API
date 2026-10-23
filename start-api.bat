@echo off
title Kafh Organic Farm REST API
echo ========================================================
echo Starting Kafh Organic Farm Backend API (ASP.NET Core)...
echo Base URL: http://localhost:5275
echo Swagger:  http://localhost:5275/swagger
echo ========================================================
cd /d "%~dp0"
dotnet run
pause
