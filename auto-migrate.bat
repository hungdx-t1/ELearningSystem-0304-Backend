@echo off
:: Cấu hình hiển thị tiếng Việt có dấu trên Terminal
chcp 65001 >nul

echo Running auto migration and update database...
echo.

:: 1. Chạy Build trước để đảm bảo code không có lỗi
echo [*] Building project...
dotnet build
if %errorlevel% neq 0 (
    echo.
    echo [!] Error when building project. Please check and fix the code before migrating!
    pause
    exit /b %errorlevel%
)

echo.
:: 2. Yêu cầu nhập tên Migration từ bàn phím
set /p migration_name="-> Enter Migration name: "

:: Kiểm tra nếu người dùng lỡ nhấn Enter mà không gõ gì
if "%migration_name%"=="" (
    echo.
    echo [!] You didn't enter a migration name! Canceling operation.
    pause
    exit /b
)

echo.
:: 3. Chạy lệnh Add Migration
echo [*] Creating Migration: %migration_name% ...
dotnet ef migrations add "%migration_name%" --project ELearning.Infrastructure --startup-project ELearning.API

if %errorlevel% neq 0 (
    echo.
    echo [!] Failed to create migration! Please check the log above.
    pause
    exit /b %errorlevel%
)

echo.
:: 4. Chạy lệnh Update Database
echo [*] Updating Database (PostgreSQL)...
dotnet ef database update --project ELearning.Infrastructure --startup-project ELearning.API

if %errorlevel% neq 0 (
    echo.
    echo [!] Failed to update database! Please check the log above.
    pause
    exit /b %errorlevel%
)

echo.
echo   [v] DONE! DATABASE UPDATED SUCCESSFULLY!
pause