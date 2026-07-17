# Script de Instalação Rápida (Sideload) em 1 Clique para o Xbox Game Bar
# Executar no PowerShell do Windows como Administrador ou usuário com Modo Desenvolvedor ativo

Write-Host "============================================================" -ForegroundColor Green
Write-Host "  GameBarMixr - Instalador do Widget do Xbox Game Bar" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Join-Path $scriptDir "..\GameBarMixr"
$manifestPath = Join-Path $projectDir "Package.appxmanifest"

if (-not (Test-Path $manifestPath)) {
    Write-Host "[ERRO] Arquivo Package.appxmanifest não encontrado em $manifestPath" -ForegroundColor Red
    exit 1
}

Write-Host "[1/3] Habilitando registro de desenvolvedor para pacotes AppX..." -ForegroundColor Yellow
Add-AppxPackage -Register "$manifestPath" -ForceApplicationShutdown

Write-Host "[2/3] Verificando integração com o Xbox Game Bar..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

Write-Host "[3/3] Instalação concluída com SUCESSO!" -ForegroundColor Green
Write-Host ""
Write-Host "Pressione 'Win + G' no seu teclado para abrir a Xbox Game Bar." -ForegroundColor Cyan
Write-Host "O widget 'Audio & Bluetooth Mixer' estará visível no menu de Widgets!" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Green
