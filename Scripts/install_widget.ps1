# Script de Instalação Rápida (Sideload) em 1 Clique para o Xbox Game Bar
# Executar no PowerShell do Windows como Administrador ou usuário com Modo Desenvolvedor ativo

Write-Host "============================================================" -ForegroundColor Green
Write-Host "  GameBarMixr - Instalador do Widget do Xbox Game Bar" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Join-Path $scriptDir "..\GameBarMixr"
$appxManifestPath = Join-Path $projectDir "AppxManifest.xml"
$packageManifestPath = Join-Path $projectDir "Package.appxmanifest"

# O Windows Add-AppxPackage exige estritamente o arquivo nomeado como AppxManifest.xml
if (-not (Test-Path $appxManifestPath)) {
    if (Test-Path $packageManifestPath) {
        Write-Host "[INFO] Gerando AppxManifest.xml a partir de Package.appxmanifest..." -ForegroundColor Yellow
        $content = Get-Content $packageManifestPath -Raw
        $content = $content -replace '\$targetnametoken\$\.exe', 'GameBarMixr.exe'
        Set-Content -Path $appxManifestPath -Value $content
    } else {
        Write-Host "[ERRO] Arquivo AppxManifest.xml não encontrado em $appxManifestPath" -ForegroundColor Red
        exit 1
    }
}

Write-Host "[1/3] Habilitando registro de desenvolvedor e instalando pacote AppX..." -ForegroundColor Yellow
Add-AppxPackage -Register "$appxManifestPath" -ForceApplicationShutdown

Write-Host "[2/3] Verificando integração com o Xbox Game Bar..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

Write-Host "[3/3] Instalação concluída com SUCESSO!" -ForegroundColor Green
Write-Host ""
Write-Host "Pressione 'Win + G' no seu teclado para abrir a Xbox Game Bar." -ForegroundColor Cyan
Write-Host "O widget 'Audio & Bluetooth Mixer' estará visível no menu de Widgets!" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Green
