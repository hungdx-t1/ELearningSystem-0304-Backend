@echo off
title Khoi chay ELearning API

echo Running backend API, please wait...
echo.

echo [1/3] Dang don dep project (dotnet clean)...
dotnet clean
echo.

echo [2/3] Dang build project (dotnet build)...
dotnet build
echo.

echo [3/3] Dang chuyen huong va chay API (dotnet run)...
cd ELearning.API
dotnet run

pause