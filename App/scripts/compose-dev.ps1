Param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ArgsPassthrough
)

$ErrorActionPreference = 'Stop'

Set-Location (Join-Path $PSScriptRoot '..')

$envFile = '.env.dev'
$files = @('docker-compose.yml','docker-compose.dev.yml')
$profiles = @('App','Api','Database','Cache','Message','Dashboard')

if (-not $ArgsPassthrough -or $ArgsPassthrough.Count -eq 0) {
  $ArgsPassthrough = @('up','-d')
}

$fileArgs = @()
foreach ($f in $files) { $fileArgs += @('-f', $f) }

$profileArgs = @()
foreach ($p in $profiles) { $profileArgs += @('--profile', $p) }

docker compose --env-file $envFile @fileArgs @profileArgs @ArgsPassthrough

