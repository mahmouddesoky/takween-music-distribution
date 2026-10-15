<#
.SYNOPSIS
    Starts the whole app with one command: backend API (port 5080) + Angular frontend (port 4200).

.DESCRIPTION
    - Checks that .NET and Node.js are installed.
    - Runs `npm install` the first time (when frontend/node_modules is missing).
    - Opens the API in its own window, waits until it answers, then opens the frontend in its own window.
    - Opens http://localhost:4200 in the browser.
    Close the two windows (or press Ctrl+C in them) to stop everything.

.PARAMETER Fresh
    Deletes the local SQLite database first so it is re-created and re-seeded.

.PARAMETER Test
    Runs the backend tests before starting.

.PARAMETER NoBrowser
    Does not open the browser.

.EXAMPLE
    .\run.ps1
    .\run.ps1 -Fresh -Test
#>
param(
    [switch]$Fresh,
    [switch]$Test,
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'

$root        = $PSScriptRoot
$backendDir  = Join-Path $root 'backend'
$apiProject  = Join-Path $backendDir 'src\MusicDistribution.Api'
$frontendDir = Join-Path $root 'frontend'
$apiUrl      = 'http://localhost:5080'
$webUrl      = 'http://localhost:4200'

function Write-Step($message) { Write-Host "==> $message" -ForegroundColor Cyan }
function Fail($message) { Write-Host "ERROR: $message" -ForegroundColor Red; exit 1 }

function Test-PortInUse([int]$port) {
    # The Angular dev server may listen on IPv6 only (::1), so check both loopback addresses.
    foreach ($address in '127.0.0.1', '::1') {
        $client = New-Object System.Net.Sockets.TcpClient([System.Net.IPAddress]::Parse($address).AddressFamily)
        try { $client.Connect($address, $port); return $true } catch { } finally { $client.Dispose() }
    }
    return $false
}

# Use PowerShell 7 for the child windows when available, otherwise Windows PowerShell.
$shell = if (Get-Command pwsh -ErrorAction SilentlyContinue) { 'pwsh' } else { 'powershell' }

# 1. Prerequisites
Write-Step 'Checking prerequisites'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Fail '.NET SDK not found. Install .NET 10: https://dotnet.microsoft.com/download' }
if (-not (Get-Command npm -ErrorAction SilentlyContinue))    { Fail 'Node.js/npm not found. Install Node.js 20+: https://nodejs.org' }
if (-not ((dotnet --list-sdks) -match '^10\.')) { Fail '.NET 10 SDK is required (dotnet --list-sdks shows none).' }
Write-Host "    dotnet $(dotnet --version), node $(node --version)"

if (Test-PortInUse 5080) { Fail "Port 5080 is already in use. Stop the other API first (maybe an old run is still open)." }
if (Test-PortInUse 4200) { Fail "Port 4200 is already in use. Stop the other frontend first (maybe an old run is still open)." }

# 2. Optional fresh database
if ($Fresh) {
    Write-Step 'Deleting the local database (it will be re-created and re-seeded)'
    Get-ChildItem -Path $apiProject -Filter 'musicdistribution.db*' -ErrorAction SilentlyContinue | Remove-Item -Force
}

# 3. Optional tests
if ($Test) {
    Write-Step 'Running backend tests'
    dotnet test $backendDir --nologo
    if ($LASTEXITCODE -ne 0) { Fail 'Tests failed.' }
}

# 4. Frontend dependencies (first run only)
if (-not (Test-Path (Join-Path $frontendDir 'node_modules'))) {
    Write-Step 'Installing frontend packages (first run only, takes a minute)'
    Push-Location $frontendDir
    try {
        npm install
        if ($LASTEXITCODE -ne 0) { Fail 'npm install failed.' }
    } finally { Pop-Location }
}

# 5. Backend in its own window
Write-Step "Starting the API on $apiUrl"
$apiCommand = "`$Host.UI.RawUI.WindowTitle = 'Music Distribution - API (5080)'; dotnet run --project '$apiProject'"
Start-Process $shell -WorkingDirectory $backendDir -ArgumentList '-NoExit', '-Command', $apiCommand | Out-Null

Write-Host '    Waiting for the API to answer (first build can take ~30s)' -NoNewline
$deadline = (Get-Date).AddMinutes(3)
$apiReady = $false
while ((Get-Date) -lt $deadline) {
    try {
        Invoke-WebRequest -Uri "$apiUrl/api/dsps" -UseBasicParsing -TimeoutSec 3 | Out-Null
        $apiReady = $true
        break
    } catch {
        Write-Host '.' -NoNewline
        Start-Sleep -Seconds 2
    }
}
Write-Host ''
if (-not $apiReady) { Fail 'The API did not start within 3 minutes. Check the API window for the error.' }
Write-Host '    API is up.' -ForegroundColor Green

# 6. Frontend in its own window
Write-Step "Starting the frontend on $webUrl"
$webCommand = "`$Host.UI.RawUI.WindowTitle = 'Music Distribution - Frontend (4200)'; npm start"
Start-Process $shell -WorkingDirectory $frontendDir -ArgumentList '-NoExit', '-Command', $webCommand | Out-Null

Write-Host '    Waiting for the frontend' -NoNewline
$deadline = (Get-Date).AddMinutes(3)
$webReady = $false
while ((Get-Date) -lt $deadline) {
    if (Test-PortInUse 4200) { $webReady = $true; break }
    Write-Host '.' -NoNewline
    Start-Sleep -Seconds 2
}
Write-Host ''
if (-not $webReady) { Fail 'The frontend did not start within 3 minutes. Check the Frontend window for the error.' }

if (-not $NoBrowser) { Start-Process $webUrl }

Write-Host ''
Write-Host 'Everything is running:' -ForegroundColor Green
Write-Host "  App        $webUrl   (admin / admin123)"
Write-Host "  API        $apiUrl"
Write-Host "  API docs   $apiUrl/scalar/v1"
Write-Host ''
Write-Host 'To stop: close the "API" and "Frontend" windows (or press Ctrl+C in each).'
