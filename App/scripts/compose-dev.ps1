<#
.SYNOPSIS
    Docker Compose script for Development environment

.DESCRIPTION
    Runs docker compose with development configuration:
    - Uses .env.dev environment file
    - Loads docker-compose.yml + docker-compose.dev.yml
    - Enables profiles: App, Api, Database, Cache, Message, Dashboard

.PARAMETER ArgsPassthrough
    Additional arguments to pass to docker compose command

.EXAMPLE
    ./compose-dev.ps1
    Starts all services in detached mode

.EXAMPLE
    ./compose-dev.ps1 logs -f api
    View logs for API service

.EXAMPLE
    ./compose-dev.ps1 down -v
    Stop and remove all containers and volumes
#>

[CmdletBinding()]
Param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ArgsPassthrough
)

$ErrorActionPreference = 'Stop'

# Change to App directory (parent of scripts folder)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appDir = Join-Path $scriptDir '..'
Set-Location $appDir

# Configuration
$envFile = '.env.dev'
$composeFiles = @('docker-compose.yml', 'docker-compose.dev.yml')
$profiles = @('App', 'Api', 'Database', 'Cache', 'Message', 'Dashboard')

# Default command if no arguments provided
if (-not $ArgsPassthrough -or $ArgsPassthrough.Count -eq 0) {
    $ArgsPassthrough = @('up', '-d')
}

# Build file arguments (-f docker-compose.yml -f docker-compose.dev.yml)
$fileArgs = @()
foreach ($file in $composeFiles) {
    $fileArgs += '-f'
    $fileArgs += $file
}

# Build profile arguments (--profile App --profile Api ...)
$profileArgs = @()
foreach ($profile in $profiles) {
    $profileArgs += '--profile'
    $profileArgs += $profile
}

# Build final command arguments
$dockerArgs = @(
    'compose'
    '--env-file', $envFile
    $fileArgs
    $profileArgs
    $ArgsPassthrough
)

# Execute docker compose command
Write-Host "Running: docker $($dockerArgs -join ' ')" -ForegroundColor Cyan
& docker $dockerArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker compose command failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

