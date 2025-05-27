@echo off
echo ShareItems_WebApp Runner Script
echo This script makes it easy to start the application correctly

:: Check for and kill any processes on port 5093
FOR /F "tokens=5" %%P IN ('netstat -ano ^| findstr :5093') DO (
    echo Found process using port 5093: %%P
    echo Attempting to terminate...
    taskkill /F /PID %%P
    echo Process terminated.
)

:: Navigate to the correct directory and start the application
cd ShareItems_WebApp
echo Starting ShareItems_WebApp from the correct directory...
dotnet run 