<#
.SYNOPSIS
    Docker Compose script for Staging environment

.DESCRIPTION
    Runs docker compose with staging configuration:
    - Uses .env.stag environment file
    - Loads docker-compose.yml + docker-compose.stag.yml
    - Enables profiles: App, Api, Database, Cache, Message

.PARAMETER ArgsPassthrough
    Additional arguments to pass to docker compose command

.EXAMPLE
    ./compose-stag.ps1
    Starts all services in detached mode

.EXAMPLE
    ./compose-stag.ps1 ps
    View status of all services

.EXAMPLE
    ./compose-stag.ps1 down
    Stop all services
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
$envFile = '.env.stag'
$composeFiles = @('docker-compose.yml', 'docker-compose.stag.yml')
$profiles = @('App', 'Api', 'Database', 'Cache', 'Message')

# Default command if no arguments provided
if (-not $ArgsPassthrough -or $ArgsPassthrough.Count -eq 0) {
    $ArgsPassthrough = @('up', '-d')
}

# Build file arguments (-f docker-compose.yml -f docker-compose.stag.yml)
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

