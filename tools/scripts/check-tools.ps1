# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

# check-tools.ps1
# Ueberprueft ob alle Entwicklungstools installiert sind

Write-Host "`n[CHECK] MedEasy Entwicklungstools Check" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$tools = @(
    @{Name="Git"; Command="git --version"},
    @{Name="GitHub CLI"; Command="gh --version"},
    @{Name=".NET SDK"; Command="dotnet --version"},
    @{Name="Node.js"; Command="node --version"},
    @{Name="Python"; Command="python --version"},
    @{Name="Rust"; Command="rustc --version"}
)

$missing = @()

foreach ($tool in $tools) {
    try {
        $output = Invoke-Expression $tool.Command 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] $($tool.Name): $output" -ForegroundColor Green
        } else {
            throw
        }
    } catch {
        Write-Host "[FEHLT] $($tool.Name): Nicht installiert" -ForegroundColor Red
        $missing += $tool.Name
    }
}

if ($missing.Count -gt 0) {
    Write-Host "`n[WARNUNG] Fehlende Tools:" -ForegroundColor Yellow
    $missing | ForEach-Object { Write-Host "   - $_" -ForegroundColor Yellow }
    Write-Host "`nInstalliere fehlende Tools mit winget oder den jeweiligen Installern." -ForegroundColor Cyan
} else {
    Write-Host "`n[OK] Alle Tools installiert! Bereit fuer Phase 0.2" -ForegroundColor Green
}

# Optional: CUDA Check
Write-Host "`n[INFO] Optionale Tools:" -ForegroundColor Cyan
try {
    $nvccVersion = nvcc --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] CUDA Toolkit installiert" -ForegroundColor Green
    } else {
        throw
    }
} catch {
    Write-Host "[INFO] CUDA Toolkit: Nicht installiert (optional fuer GPU-Acceleration)" -ForegroundColor Gray
}
