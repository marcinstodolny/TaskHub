param(
    [switch]$SkipCompose
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$envExamplePath = Join-Path $repoRoot ".env.example"
$envPath = Join-Path $repoRoot ".env"

if (-not (Test-Path -LiteralPath $envPath)) {
    Copy-Item -LiteralPath $envExamplePath -Destination $envPath
    Write-Host "Created .env from .env.example"
}
else {
    Write-Host ".env already exists"
}

if ($SkipCompose) {
    Write-Host "Skipping docker compose startup"
    exit 0
}

Push-Location $repoRoot
try {
    docker compose up --build
}
finally {
    Pop-Location
}
