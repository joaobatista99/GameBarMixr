# GameBarMixr - Instalador 1-Clique para Xbox Game Bar
# Requisito: .NET 8+ SDK (https://dot.net) — sem necessidade de Visual Studio

param(
    [string]$Platform = "x64",
    [string]$Configuration = "Release"
)

Write-Host "============================================================" -ForegroundColor Green
Write-Host "  GameBarMixr - Instalador do Widget do Xbox Game Bar" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green

$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Join-Path $scriptDir "..\GameBarMixr"
$csprojPath = Join-Path $projectDir "GameBarMixr.csproj"
$outputDir  = Join-Path $projectDir "bin\publish\$Platform"

# ── PASSO 0: Verifica .NET SDK ──────────────────────────────────────────────
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERRO] .NET SDK nao encontrado. Instale em: https://dot.net/download" -ForegroundColor Red
    exit 1
}

$dotnetVer = dotnet --version
Write-Host "       .NET SDK: $dotnetVer" -ForegroundColor Gray

# ── PASSO 1: Gerar Assets ───────────────────────────────────────────────────
Write-Host ""
Write-Host "[1/4] Gerando assets de imagem..." -ForegroundColor Yellow
$generateAssetsScript = Join-Path $scriptDir "generate_assets.ps1"
if (Test-Path $generateAssetsScript) {
    & "$generateAssetsScript"
}

# ── PASSO 2: Compilar com dotnet publish ────────────────────────────────────
Write-Host ""
Write-Host "[2/4] Compilando GameBarMixr (WinForms / $Configuration / $Platform)..." -ForegroundColor Yellow

if (-not (Test-Path $csprojPath)) {
    Write-Host "[ERRO] GameBarMixr.csproj nao encontrado em $csprojPath" -ForegroundColor Red
    exit 1
}

dotnet publish "$csprojPath" `
    --configuration $Configuration `
    --output "$outputDir" `
    /p:Platform=$Platform

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "[ERRO] Falha na compilacao. Verifique os erros acima." -ForegroundColor Red
    exit 1
}

Write-Host "       Build OK -> $outputDir" -ForegroundColor Green

# ── PASSO 3: Preparar pacote (manifest + assets) ────────────────────────────
Write-Host ""
Write-Host "[3/4] Preparando pacote AppX..." -ForegroundColor Yellow

$manifestSrc = Join-Path $projectDir "AppxManifest.xml"
$manifestDst = Join-Path $outputDir  "AppxManifest.xml"
$assetsSrc   = Join-Path $projectDir "Assets"
$assetsDst   = Join-Path $outputDir  "Assets"

# Substitui $targetnametoken$ pelo nome correto do exe no manifest
$content = Get-Content $manifestSrc -Raw
$content = $content -replace '\$targetnametoken\$', 'GameBarMixr'
Set-Content -Path $manifestDst -Value $content

if (Test-Path $assetsSrc) {
    Copy-Item -Path $assetsSrc -Destination $assetsDst -Recurse -Force
}

# ── PASSO 4: Registrar no Xbox Game Bar ─────────────────────────────────────
Write-Host ""
Write-Host "[4/4] Registrando widget no Xbox Game Bar..." -ForegroundColor Yellow

Add-AppxPackage -Register "$manifestDst" -ForceApplicationShutdown

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  Instalacao concluida com SUCESSO!" -ForegroundColor Green
    Write-Host "  Pressione Win+G -> Widgets -> Audio & Bluetooth Mixer" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[ERRO] Falha ao registrar o pacote." -ForegroundColor Red
}
