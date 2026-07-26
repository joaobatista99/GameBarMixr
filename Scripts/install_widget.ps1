# GameBarMixr - Instalador 1-Clique para Xbox Game Bar
# Executa no PowerShell do Windows como Administrador, com Modo Desenvolvedor habilitado.
#
# Requisito: Visual Studio Build Tools 2022 instalado com workload:
#   - ".NET desktop build tools"
#   - "Windows App SDK C# Templates"

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

# ── Função para localizar MSBuild.exe do Visual Studio ──────────────────────
function Find-MSBuild {
    $candidates = @(
        # VS Build Tools 2022
        "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        # VS Community/Professional/Enterprise 2022
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        # VS 2019 fallback
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($path in $candidates) {
        if (Test-Path $path) {
            return $path
        }
    }

    # Try vswhere.exe for dynamic detection
    $vswhere = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $installPath = & "$vswhere" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath 2>$null
        if ($installPath) {
            $msbuild = Join-Path $installPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $msbuild) { return $msbuild }
        }
    }

    return $null
}

# ── PASSO 1: Gerar Assets ───────────────────────────────────────────────────
Write-Host ""
Write-Host "[1/4] Gerando assets de imagem..." -ForegroundColor Yellow
$generateAssetsScript = Join-Path $scriptDir "generate_assets.ps1"
if (Test-Path $generateAssetsScript) {
    & "$generateAssetsScript"
} else {
    Write-Host "       [AVISO] generate_assets.ps1 nao encontrado, pulando." -ForegroundColor DarkYellow
}

# ── PASSO 2: Localizar MSBuild ──────────────────────────────────────────────
Write-Host ""
Write-Host "[2/4] Localizando Visual Studio MSBuild..." -ForegroundColor Yellow

$msbuild = Find-MSBuild

if (-not $msbuild) {
    Write-Host ""
    Write-Host "[ERRO] MSBuild.exe nao encontrado." -ForegroundColor Red
    Write-Host ""
    Write-Host "Instale o Visual Studio Build Tools 2022:" -ForegroundColor Yellow
    Write-Host "  https://visualstudio.microsoft.com/downloads/#build-tools" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Durante a instalacao, selecione:" -ForegroundColor Yellow
    Write-Host "  [x] .NET desktop build tools" -ForegroundColor White
    Write-Host "  [x] Windows App SDK C# Templates (painel direito, Optional)" -ForegroundColor White
    exit 1
}

Write-Host "       Encontrado: $msbuild" -ForegroundColor Gray

# ── PASSO 3: Compilar com MSBuild do VS ─────────────────────────────────────
Write-Host ""
Write-Host "[3/4] Compilando GameBarMixr com MSBuild ($Configuration / $Platform)..." -ForegroundColor Yellow

$outputDir = "$projectDir\bin\$Configuration\net8.0-windows10.0.19041.0\publish"

& "$msbuild" "$csprojPath" `
    /t:Restore,Build,Publish `
    /p:Configuration=$Configuration `
    /p:Platform=$Platform `
    /p:PublishDir="$outputDir" `
    /p:WindowsPackageType=None `
    /m `
    /nologo `
    /verbosity:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "[ERRO] Falha na compilacao. Verifique os erros acima." -ForegroundColor Red
    Write-Host "Dica: Abra a solucao no Visual Studio para diagnosticar." -ForegroundColor Yellow
    exit 1
}

Write-Host "       Build concluido com sucesso!" -ForegroundColor Green

# ── PASSO 4: Preparar e Registrar o Pacote ──────────────────────────────────
Write-Host ""
Write-Host "[4/4] Registrando widget no Xbox Game Bar..." -ForegroundColor Yellow

$manifestSrc = Join-Path $projectDir "AppxManifest.xml"
$manifestDst = Join-Path $outputDir "AppxManifest.xml"
$assetsSrc   = Join-Path $projectDir "Assets"
$assetsDst   = Join-Path $outputDir "Assets"

Copy-Item -Path $manifestSrc -Destination $manifestDst -Force
if (Test-Path $assetsSrc) {
    Copy-Item -Path $assetsSrc -Destination $assetsDst -Recurse -Force
}

Add-AppxPackage -Register "$manifestDst" -ForceApplicationShutdown

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  Instalacao concluida com SUCESSO!" -ForegroundColor Green
    Write-Host "  Pressione Win+G para abrir a Xbox Game Bar." -ForegroundColor Cyan
    Write-Host "  Va em Widgets e selecione 'Audio & Bluetooth Mixer'." -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[ERRO] Falha ao registrar o pacote. Verifique as mensagens acima." -ForegroundColor Red
}
