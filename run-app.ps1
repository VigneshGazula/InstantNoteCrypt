# ShareItems_WebApp Runner Script
# This script makes it easy to start the application correctly

# Check if there are existing dotnet processes that might interfere
$processes = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
foreach ($process in $processes) {
    Write-Host "Found dotnet process with ID: $($process.Id)"
    $netstat = netstat -ano | findstr $($process.Id)
    if ($netstat -match "5093") {
        Write-Host "This process is using port 5093. Attempting to terminate..."
        Stop-Process -Id $process.Id -Force
        Write-Host "Process terminated."
    }
}

# Navigate to the correct directory and start the application
Set-Location -Path "$PSScriptRoot\ShareItems_WebApp"
Write-Host "Starting ShareItems_WebApp from the correct directory..."
dotnet run 