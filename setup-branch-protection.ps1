# setup-branch-protection.ps1
# MedEasy - Branch Protection Konfiguration
# Zweck: Sichert den main Branch gegen versehentliche Änderungen

param(
    [string]$RepoName = "medeasy",
    [string]$Branch = "main",
    [switch]$Minimal
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
Write-ColorOutput "`n[MEDEASY] Branch Protection Setup" $InfoColor
Write-ColorOutput "============================================" $InfoColor
Write-ColorOutput "Sichere den $Branch Branch ab`n" $InfoColor

# Prüfe ob gh installiert ist
Write-ColorOutput "[1/3] Pruefe GitHub CLI..." $InfoColor
try {
    $ghVersion = gh --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI nicht gefunden"
    }
    Write-ColorOutput "[OK] GitHub CLI verfuegbar" $SuccessColor
} catch {
    Write-ColorOutput "[FEHLER] GitHub CLI nicht installiert" $ErrorColor
    Write-ColorOutput "Installiere mit: winget install GitHub.cli" $InfoColor
    exit 1
}

# Prüfe ob Repository existiert
Write-ColorOutput "`n[2/3] Pruefe Repository..." $InfoColor
try {
    $repoInfo = gh repo view $RepoName --json name,isPrivate,defaultBranchRef 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Repository nicht gefunden"
    }
    
    $repoData = $repoInfo | ConvertFrom-Json
    Write-ColorOutput "[OK] Repository '$($repoData.name)' gefunden" $SuccessColor
    Write-ColorOutput "   - Privat: $($repoData.isPrivate)" $InfoColor
    Write-ColorOutput "   - Default Branch: $($repoData.defaultBranchRef.name)" $InfoColor
    
} catch {
    Write-ColorOutput "[FEHLER] Repository '$RepoName' nicht gefunden oder kein Zugriff" $ErrorColor
    Write-ColorOutput "Stelle sicher, dass du im richtigen Verzeichnis bist oder gib den vollen Namen an (user/repo)" $InfoColor
    exit 1
}

# Branch Protection einrichten
Write-ColorOutput "`n[3/3] Konfiguriere Branch Protection fuer '$Branch'..." $InfoColor

if ($Minimal) {
    # Minimale Protection (nur Force-Push und Deletion verhindern)
    Write-ColorOutput "Verwende minimale Protection-Einstellungen..." $InfoColor
    
    $protection = @{
        "enforce_admins" = $false
        "required_pull_request_reviews" = $null
        "required_status_checks" = $null
        "restrictions" = $null
        "allow_force_pushes" = $false
        "allow_deletions" = $false
        "required_conversation_resolution" = $false
        "lock_branch" = $false
        "allow_fork_syncing" = $false
    }
} else {
    # Vollständige Protection für Produktion
    Write-ColorOutput "Verwende vollstaendige Protection-Einstellungen..." $InfoColor
    
    $protection = @{
        "enforce_admins" = $true
        "required_pull_request_reviews" = @{
            "required_approving_review_count" = 1
            "dismiss_stale_reviews" = $true
            "require_code_owner_reviews" = $false
            "require_last_push_approval" = $false
            "bypass_pull_request_allowances" = @{
                "users" = @()
                "teams" = @()
                "apps" = @()
            }
        }
        "required_status_checks" = @{
            "strict" = $true
            "contexts" = @()  # Wird später mit CI/CD gefüllt
        }
        "restrictions" = $null  # Keine Einschränkungen wer pushen darf
        "allow_force_pushes" = $false
        "allow_deletions" = $false
        "block_creations" = $false
        "required_conversation_resolution" = $true
        "lock_branch" = $false
        "allow_fork_syncing" = $false
    }
}

# Konvertiere zu JSON
$protectionJson = $protection | ConvertTo-Json -Depth 10

# Wende Protection an
try {
    # Erstelle temporäre Datei für JSON (PowerShell Pipe-Probleme umgehen)
    $tempFile = New-TemporaryFile
    Set-Content -Path $tempFile.FullName -Value $protectionJson -Encoding UTF8
    
    # API-Aufruf
    $result = gh api "repos/$RepoName/branches/$Branch/protection" `
        --method PUT `
        --input $tempFile.FullName `
        2>&1
    
    # Cleanup
    Remove-Item $tempFile.FullName -Force
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "`n[OK] Branch Protection erfolgreich aktiviert!" $SuccessColor
        
        if ($Minimal) {
            Write-ColorOutput "`nMinimale Protection aktiviert:" $InfoColor
            Write-ColorOutput "   - Force Push: BLOCKIERT" $SuccessColor
            Write-ColorOutput "   - Branch loeschen: BLOCKIERT" $SuccessColor
            Write-ColorOutput "   - Direkte Commits: ERLAUBT" $WarningColor
        } else {
            Write-ColorOutput "`nVollstaendige Protection aktiviert:" $InfoColor
            Write-ColorOutput "   - Force Push: BLOCKIERT" $SuccessColor
            Write-ColorOutput "   - Branch loeschen: BLOCKIERT" $SuccessColor
            Write-ColorOutput "   - Pull Request Reviews: ERFORDERLICH (1)" $SuccessColor
            Write-ColorOutput "   - Veraltete Reviews: WERDEN VERWORFEN" $SuccessColor
            Write-ColorOutput "   - Admins: MUESSEN SICH AN REGELN HALTEN" $SuccessColor
            Write-ColorOutput "   - Conversations: MUESSEN AUFGELOEST SEIN" $SuccessColor
        }
        
    } else {
        throw "API-Aufruf fehlgeschlagen: $result"
    }
    
} catch {
    Write-ColorOutput "`n[FEHLER] Branch Protection konnte nicht aktiviert werden" $ErrorColor
    Write-ColorOutput "Fehler: $_" $ErrorColor
    
    Write-ColorOutput "`nMoegliche Gruende:" $WarningColor
    Write-ColorOutput "   - Keine Admin-Rechte am Repository" $InfoColor
    Write-ColorOutput "   - Repository ist zu neu (warte 30 Sekunden)" $InfoColor
    Write-ColorOutput "   - Branch existiert noch nicht" $InfoColor
    Write-ColorOutput "   - Kostenloses GitHub-Konto (einige Features nur in Pro)" $InfoColor
    
    exit 1
}

# Status anzeigen
Write-ColorOutput "`n============================================" $InfoColor
Write-ColorOutput "Branch Protection Status:" $InfoColor

try {
    $status = gh api "repos/$RepoName/branches/$Branch/protection" 2>$null | ConvertFrom-Json
    
    if ($status) {
        Write-ColorOutput "`n[AKTUELLE EINSTELLUNGEN]" $SuccessColor
        Write-ColorOutput "   Enforce Admins: $($status.enforce_admins.enabled)" $InfoColor
        Write-ColorOutput "   Allow Force Pushes: $($status.allow_force_pushes.enabled)" $InfoColor
        Write-ColorOutput "   Allow Deletions: $($status.allow_deletions.enabled)" $InfoColor
        
        if ($status.required_pull_request_reviews) {
            Write-ColorOutput "   Required Reviews: $($status.required_pull_request_reviews.required_approving_review_count)" $InfoColor
        }
    }
} catch {
    Write-ColorOutput "[INFO] Status konnte nicht abgerufen werden" $WarningColor
}

Write-ColorOutput "`n============================================" $InfoColor
Write-ColorOutput "[FERTIG] Branch Protection konfiguriert!" $SuccessColor

# Hinweise
if (!$Minimal) {
    Write-ColorOutput "`n[HINWEIS] Fuer vollstaendige Sicherheit:" $WarningColor
    Write-ColorOutput "   1. Erstelle eine .github/CODEOWNERS Datei" $InfoColor
    Write-ColorOutput "   2. Konfiguriere CI/CD Status Checks" $InfoColor
    Write-ColorOutput "   3. Aktiviere Secret Scanning" $InfoColor
    Write-ColorOutput "   4. Aktiviere Dependabot" $InfoColor
}

Write-ColorOutput "`nWeitere Infos: https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/defining-the-mergeability-of-pull-requests/about-protected-branches" $InfoColor