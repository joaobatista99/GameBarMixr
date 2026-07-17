# Script de Compilação e Empacotamento MSIX para Microsoft Store Partner Center

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  GameBarMixr - Gerador de Pacote MSIX para Microsoft Store" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $scriptDir "..\GameBarMixr.sln"

Write-Host "[1/2] Compilando projeto C# WinUI 3 no modo $Configuration ($Platform)..." -ForegroundColor Yellow
dotnet publish "$solutionPath" -c $Configuration -r win-$Platform --self-contained true /p:GenerateAppxPackageOnBuild=true /p:AppxPackageDir="bundle\"

if ($LASTEXITCODE -eq 0) {
    Write-Host "[2/2] Pacote MSIX gerado com sucesso!" -ForegroundColor Green
    Write-Host "O arquivo de upload para a Microsoft Store (.msixbundle / .msix) está disponível na pasta 'GameBarMixr\bin\MSIX\' ou 'bundle\'." -ForegroundColor Green
} else {
    Write-Host "[ERRO] Falha ao compilar o pacote MSIX." -ForegroundColor Red
}
