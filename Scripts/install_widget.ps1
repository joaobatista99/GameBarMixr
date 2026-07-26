# GameBarMixr - Instalador 1-Clique para Xbox Game Bar
# Executar no PowerShell do Windows como Administrador, com Modo Desenvolvedor habilitado.
#
# Requisito: .NET 8 SDK instalado (https://dot.net)
# Verifique com: dotnet --version

param(
    [string]$Platform = "x64"  # ou x86 / ARM64
)

Write-Host "============================================================" -ForegroundColor Green
Write-Host "  GameBarMixr - Instalador do Widget do Xbox Game Bar" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green

$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir  = Join-Path $scriptDir "..\GameBarMixr"
$csprojPath  = Join-Path $projectDir "GameBarMixr.csproj"

# ── PASSO 0: Verifica .NET SDK ───────────────────────────────────────────────
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "[ERRO] .NET SDK não encontrado." -ForegroundColor Red
    Write-Host "Instale em: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# ── PASSO 1: Gerar Assets ───────────────────────────────────────────────────
Write-Host ""
Write-Host "[1/4] Gerando assets de imagem..." -ForegroundColor Yellow
$generateAssetsScript = Join-Path $scriptDir "generate_assets.ps1"
if (Test-Path $generateAssetsScript) {
    & "$generateAssetsScript"
} else {
    Write-Host "       [AVISO] generate_assets.ps1 não encontrado, pulando." -ForegroundColor DarkYellow
}

# ── PASSO 2: Compilar o projeto C# ─────────────────────────────────────────
Write-Host ""
Write-Host "[2/4] Compilando GameBarMixr.csproj (Release / $Platform)..." -ForegroundColor Yellow

if (-not (Test-Path $csprojPath)) {
    Write-Host "[ERRO] GameBarMixr.csproj não encontrado em $csprojPath" -ForegroundColor Red
    exit 1
}

dotnet publish "$csprojPath" `
    --configuration Release `
    --runtime win-$Platform `
    --self-contained true `
    --output "$projectDir\bin\publish\$Platform" `
    /p:WindowsAppSdkSelfContained=true

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "[ERRO] Falha na compilação. Verifique os erros acima." -ForegroundColor Red
    Write-Host "Dica: Abra a solução no Visual Studio para diagnosticar." -ForegroundColor Yellow
    exit 1
}

# ── PASSO 3: Copiar AppxManifest.xml e Assets para o output ────────────────
Write-Host ""
Write-Host "[3/4] Preparando pacote para registro..." -ForegroundColor Yellow

$publishDir  = "$projectDir\bin\publish\$Platform"
$manifestSrc = Join-Path $projectDir "AppxManifest.xml"
$manifestDst = Join-Path $publishDir "AppxManifest.xml"
$assetsSrc   = Join-Path $projectDir "Assets"
$assetsDst   = Join-Path $publishDir "Assets"

Copy-Item -Path $manifestSrc -Destination $manifestDst -Force

if (Test-Path $assetsSrc) {
    Copy-Item -Path $assetsSrc -Destination $assetsDst -Recurse -Force
}

# ── PASSO 4: Registrar no Xbox Game Bar ────────────────────────────────────
Write-Host ""
Write-Host "[4/4] Registrando widget no Xbox Game Bar..." -ForegroundColor Yellow
Add-AppxPackage -Register "$manifestDst" -ForceApplicationShutdown

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  Instalação concluída com SUCESSO!" -ForegroundColor Green
    Write-Host "  Pressione Win+G para abrir a Xbox Game Bar."              -ForegroundColor Cyan
    Write-Host "  Clique em Widgets e selecione 'Audio & Bluetooth Mixer'." -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[ERRO] Falha ao registrar o pacote. Verifique as mensagens acima." -ForegroundColor Red
}
