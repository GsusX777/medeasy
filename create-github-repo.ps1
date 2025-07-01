# create-github-repo-minimal.ps1
# MedEasy Phase 0.1: Entwicklungsumgebung Setup
# Zweck: Minimales Git Repository für Entwicklungsstart

param(
    [string]$RepoName = "medeasy",
    [string]$Description = "MedEasy - Intelligente medizinische Dokumentation (MVP Development)",
    [switch]$SkipGitHub
)

# Farben für Output
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Header
Write-ColorOutput "`n[MEDEASY] Repository Setup - Phase 0.1" $InfoColor
Write-ColorOutput "============================================" $InfoColor
Write-ColorOutput "Minimale Entwicklungsumgebung Setup`n" $InfoColor

# 1. Git initialisieren
Write-ColorOutput "[1/9] Initialisiere Git Repository..." $InfoColor
if (!(Test-Path ".git")) {
    git init
    Write-ColorOutput "[OK] Git Repository initialisiert" $SuccessColor
} else {
    Write-ColorOutput "[WARNUNG] Git Repository existiert bereits" $WarningColor
}

# 2. Basis-Ordnerstruktur (nur Platzhalter)
Write-ColorOutput "`n[2/9] Erstelle Basis-Ordnerstruktur..." $InfoColor

$directories = @(
    "src",
    "docs", 
    "tests",
    "tools",
    "config"
)

foreach ($dir in $directories) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        # Platzhalter .gitkeep damit Ordner committed werden
        New-Item -ItemType File -Path "$dir/.gitkeep" -Force | Out-Null
    }
}
Write-ColorOutput "[OK] Ordnerstruktur erstellt" $SuccessColor

# 3. Minimale .gitignore
Write-ColorOutput "`n[3/9] Erstelle .gitignore..." $InfoColor
$gitignoreContent = @'
# MedEasy Development - Basic .gitignore
# Weitere Eintraege werden in spaeteren Phasen ergaenzt

# IDE
.vs/
.vscode/
.idea/
*.suo
*.user

# OS
.DS_Store
Thumbs.db

# Umgebungsvariablen
.env
.env.*
!.env.example

# Build-Ausgaben (wird spaeter erweitert)
bin/
obj/
node_modules/
dist/
target/

# Logs
*.log
logs/

# Temporaere Dateien
*.tmp
temp/
'@

Set-Content -Path ".gitignore" -Value $gitignoreContent -Encoding UTF8
Write-ColorOutput "[OK] .gitignore erstellt" $SuccessColor

# 4. Minimales README
Write-ColorOutput "`n[4/9] Erstelle README.md..." $InfoColor
$readmeContent = @'
# MedEasy - MVP Development

## Projekt-Uebersicht
Intelligente, datenschutzkonforme Dokumentation und Analyse im Praxisalltag

## Status: Phase 0 - Entwicklungsumgebung Setup

### Phase 0.1 - Abgeschlossen
- [x] Git Repository initialisiert  
- [x] Basis-Ordnerstruktur erstellt
- [x] Entwicklungstools-Liste definiert

### Naechste Schritte (Phase 0.2)
- [ ] Windsurf AI Context einrichten
- [ ] Auto-Dokumentation konfigurieren
- [ ] Entwicklungstools installieren

## Erforderliche Entwicklungstools

### Basis-Tools
- **Git** (bereits installiert)
- **GitHub CLI** - `winget install GitHub.cli`
- **Windsurf IDE** - [Download](https://windsurf.ai)

### Programmiersprachen & Runtimes
- **.NET 8 SDK** - `winget install Microsoft.DotNet.SDK.8`
- **Node.js 20.x** - `winget install OpenJS.NodeJS.LTS`
- **Python 3.11** - `winget install Python.Python.3.11`
- **Rust** - `winget install Rustlang.Rust.GNU`

### Optional (fuer GPU-Acceleration)
- **CUDA Toolkit** - Fuer Whisper GPU-Beschleunigung

### API Keys (spaeter benoetigt)
- OpenAI API Key
- Claude API Key  
- Gemini API Key

## Setup-Anleitung

1. Alle Tools installieren (siehe oben)
2. Repository klonen
3. In Phase 0.2 fortfahren

---

**Hinweis:** Dies ist ein minimales Setup. Die vollstaendige Projektstruktur wird in den kommenden Phasen aufgebaut.
'@

Set-Content -Path "README.md" -Value $readmeContent -Encoding UTF8
Write-ColorOutput "[OK] README.md erstellt" $SuccessColor

# 5. .env.example (minimal)
Write-ColorOutput "`n[5/9] Erstelle .env.example..." $InfoColor
$envExampleContent = @'
# MedEasy Environment Variables Template
# Kopiere zu .env und fuelle aus (spaeter in Phase 0.2)

# Wird in spaeteren Phasen erweitert
ENVIRONMENT=development
'@

Set-Content -Path ".env.example" -Value $envExampleContent -Encoding UTF8
Write-ColorOutput "[OK] .env.example erstellt" $SuccessColor

# 6. Tool-Check Script
Write-ColorOutput "`n[6/9] Erstelle Tool-Check Script..." $InfoColor

# Erstelle Verzeichnis falls nicht vorhanden
if (!(Test-Path "tools/scripts")) {
    New-Item -ItemType Directory -Path "tools/scripts" -Force | Out-Null
}

$toolCheckContent = @'
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
'@

Set-Content -Path "tools/scripts/check-tools.ps1" -Value $toolCheckContent -Encoding UTF8
Write-ColorOutput "[OK] Tool-Check Script erstellt" $SuccessColor

# 7. Initial Commit
Write-ColorOutput "`n[7/9] Erstelle Initial Commit..." $InfoColor
git add .
git commit -m "Initial commit: MedEasy MVP Phase 0.1

- Minimale Repository-Struktur
- Basis .gitignore
- Entwicklungstools-Dokumentation
- Tool-Check Script" 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-ColorOutput "[OK] Initial Commit erstellt" $SuccessColor
} else {
    Write-ColorOutput "[WARNUNG] Commit bereits vorhanden oder keine Aenderungen" $WarningColor
}

# 8. GitHub Repository (optional)
if (!$SkipGitHub) {
    Write-ColorOutput "`n[8/9] GitHub Repository Setup..." $InfoColor
    
    # Check if gh is installed
    try {
        $ghVersion = gh --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "GitHub CLI gefunden. Erstelle Repository..." $InfoColor
            
            # Create private repository  
            $createResult = gh repo create $RepoName --private --description "$Description" --source=. --remote=origin --push 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-ColorOutput "[OK] GitHub Repository erstellt und verbunden" $SuccessColor
                $repoUrl = gh repo view --json url -q .url
                Write-ColorOutput "   URL: $repoUrl" $InfoColor
            } else {
                Write-ColorOutput "[WARNUNG] GitHub Repository konnte nicht erstellt werden" $WarningColor
                Write-ColorOutput "   Fuehre spaeter aus: gh repo create" $InfoColor
            }
        } else {
            throw
        }
    } catch {
        Write-ColorOutput "[WARNUNG] GitHub CLI nicht installiert - Repository muss manuell erstellt werden" $WarningColor
        Write-ColorOutput "   Installiere mit: winget install GitHub.cli" $InfoColor
    }
} else {
    Write-ColorOutput "`n[INFO] GitHub Setup uebersprungen (-SkipGitHub)" $WarningColor
}

# 9. Abschluss
Write-ColorOutput "`n============================================" $InfoColor
Write-ColorOutput "[FERTIG] Phase 0.1 abgeschlossen!" $SuccessColor
Write-ColorOutput "`nNaechste Schritte:" $InfoColor
Write-ColorOutput "1. Fuehre aus: .\tools\scripts\check-tools.ps1" $WarningColor
Write-ColorOutput "2. Installiere fehlende Tools" $InfoColor
Write-ColorOutput "3. Starte mit Phase 0.2 (Windsurf Context Setup)" $InfoColor
Write-ColorOutput "`nDokumentation: README.md" $InfoColor
Write-ColorOutput "============================================" $InfoColor

# Return success
exit 0