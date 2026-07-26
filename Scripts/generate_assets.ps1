# Script PowerShell para gerar os assets PNG de placeholder para o GameBarMixr
# Executa no Windows sem dependencias externas, usando System.Drawing

Add-Type -AssemblyName System.Drawing

function New-SolidPng {
    param(
        [string]$OutputPath,
        [int]$Width,
        [int]$Height,
        [string]$Label = ""
    )

    $bmp = New-Object System.Drawing.Bitmap($Width, $Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)

    # Xbox dark background
    $bg = [System.Drawing.Color]::FromArgb(255, 18, 18, 18)
    $g.Clear($bg)

    # Draw Xbox green circle
    $accent = [System.Drawing.Color]::FromArgb(255, 16, 124, 65)
    $brush = New-Object System.Drawing.SolidBrush($accent)
    $margin = [int]($Width * 0.15)
    $g.FillEllipse($brush, $margin, $margin, $Width - $margin * 2, $Height - $margin * 2)

    # Draw white label if provided
    if ($Label) {
        $font = New-Object System.Drawing.Font("Segoe UI", [math]::Max(7, $Width / 8), [System.Drawing.FontStyle]::Bold)
        $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $sf = New-Object System.Drawing.StringFormat
        $sf.Alignment = [System.Drawing.StringAlignment]::Center
        $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
        $rect = New-Object System.Drawing.RectangleF(0, 0, $Width, $Height)
        $g.DrawString($Label, $font, $textBrush, $rect, $sf)
    }

    $g.Dispose()

    $dir = Split-Path $OutputPath -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }

    $bmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "  Created: $OutputPath ($Width x $Height)" -ForegroundColor Gray
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$assetsDir = Join-Path $scriptDir "..\GameBarMixr\Assets"

Write-Host "Generating GameBarMixr asset images..." -ForegroundColor Cyan

New-SolidPng -OutputPath "$assetsDir\StoreLogo.png"          -Width 50  -Height 50  -Label "G"
New-SolidPng -OutputPath "$assetsDir\Square44x44Logo.png"    -Width 44  -Height 44  -Label "G"
New-SolidPng -OutputPath "$assetsDir\Square150x150Logo.png"  -Width 150 -Height 150 -Label "GMixr"
New-SolidPng -OutputPath "$assetsDir\Wide310x150Logo.png"    -Width 310 -Height 150 -Label "GameBarMixr"

Write-Host "All assets generated successfully!" -ForegroundColor Green
