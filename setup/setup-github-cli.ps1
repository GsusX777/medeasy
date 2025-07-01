# setup-github-cli.ps1
# GitHub CLI Setup Script für MedEasy Projekt
# Version: 1.0
# Datum: 01.07.2025

#Requires -RunAsAdministrator

# Farben für Output
$script:colors = @{
    Success = "Green"
    Error = "Red"
    Warning = "Yellow"
    Info = "Cyan"
    Normal = "White"
}

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Type = "Normal"
    )
    Write-Host $Message -ForegroundColor $script:colors[$Type]
}

function Write-Header {
    param([string]$Title)
    Write-Host "`n" -NoNewline
    Write-Host ("=" * 60) -ForegroundColor $script:colors.Info
    Write-Host $Title -ForegroundColor $script:colors.Info
    Write-Host ("=" * 60) -ForegroundColor $script:colors.Info
    Write-Host ""
}

function Test-GitHubCLI {
    try {
        $ghVersion = gh --version 2>$null
        if ($ghVersion) {
            return $true
        }
    }
    catch {
        return $false
    }
    return $false
}

function Get-LatestGitHubCLIVersion {
    try {
        $apiUrl = "https://api.github.com/repos/cli/cli/releases/latest"
        $release = Invoke-RestMethod -Uri $apiUrl -UseBasicParsing
        return $release.tag_name.TrimStart('v')
    }
    catch {
        Write-ColorOutput "WARNING: Konnte neueste Version nicht ermitteln, verwende Fallback..." -Type Warning
        return "2.52.0"  # Fallback Version
    }
}

# Ersetzen Sie die Install-GitHubCLI Funktion mit dieser Version:

function Install-GitHubCLI {
    Write-Header "GitHub CLI Installation"
    
    # Prüfe ob bereits installiert
    if (Test-GitHubCLI) {
        try {
            $ghOutput = gh --version 2>$null
            $currentVersion = "Unknown"
            if ($ghOutput -match 'gh version ([\d.]+)') {
                $currentVersion = $Matches[1]
            }
            Write-ColorOutput "OK: GitHub CLI ist bereits installiert (Version: $currentVersion)" -Type Success
            
            # Frage nach Update
            $update = Read-Host "Moechten Sie auf die neueste Version aktualisieren? (j/n)"
            if ($update -ne 'j') {
                return $true
            }
        }
        catch {
            Write-ColorOutput "OK: GitHub CLI ist installiert" -Type Success
            return $true
        }
    }
    
    try {
        Write-ColorOutput "DOWNLOAD: Ermittle neueste GitHub CLI Version..." -Type Info
        $version = Get-LatestGitHubCLIVersion
        
        # Download URL für Windows
        $downloadUrl = "https://github.com/cli/cli/releases/download/v$version/gh_${version}_windows_amd64.msi"
        $tempFile = Join-Path $env:TEMP "gh_installer.msi"
        
        Write-ColorOutput "DOWNLOAD: Lade GitHub CLI v$version herunter..." -Type Info
        Write-ColorOutput "   URL: $downloadUrl" -Type Normal
        
        # Download mit Fortschritt
        $progressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $downloadUrl -OutFile $tempFile -UseBasicParsing
        $progressPreference = 'Continue'
        
        if (Test-Path $tempFile) {
            Write-ColorOutput "INSTALL: Installiere GitHub CLI..." -Type Info
            
            # MSI Installation
            $arguments = "/i `"$tempFile`" /quiet /norestart"
            $process = Start-Process msiexec.exe -ArgumentList $arguments -Wait -PassThru
            
            if ($process.ExitCode -eq 0) {
                Write-ColorOutput "OK: GitHub CLI wurde erfolgreich installiert!" -Type Success
                
                # Pfad aktualisieren
                $machinePath = [System.Environment]::GetEnvironmentVariable("Path", "Machine")
                $userPath = [System.Environment]::GetEnvironmentVariable("Path", "User")
                $env:Path = $machinePath + ";" + $userPath
                
                # Cleanup
                Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
                
                # Verifizierung - mit besserer Fehlerbehandlung
                Start-Sleep -Seconds 2
                
                # Neues PowerShell für frischen PATH
                $testGh = powershell -Command "gh --version" 2>$null
                if ($testGh) {
                    Write-ColorOutput "OK: Installation verifiziert" -Type Success
                    if ($testGh -match 'gh version ([\d.]+)') {
                        $newVersion = $Matches[1]
                        Write-ColorOutput "   Version: $newVersion" -Type Normal
                    }
                    return $true
                }
                else {
                    # Installation war erfolgreich, aber PATH noch nicht aktualisiert
                    Write-ColorOutput "OK: Installation erfolgreich. Bitte PowerShell neu starten fuer PATH-Update." -Type Warning
                    return $true
                }
            }
            else {
                Write-ColorOutput "ERROR: Installation fehlgeschlagen (Exit Code: $($process.ExitCode))" -Type Error
                return $false
            }
        }
    }
    catch {
        # Prüfe ob Installation trotz Fehler erfolgreich war
        Start-Sleep -Seconds 2
        $testGh = powershell -Command "gh --version" 2>$null
        if ($testGh) {
            Write-ColorOutput "OK: GitHub CLI wurde installiert (trotz Warnung)" -Type Success
            return $true
        }
        
        Write-ColorOutput "ERROR: Fehler bei der Installation: $_" -Type Error
        return $false
    }
    
    return $false
}

function Test-GitHubAuth {
    try {
        $authStatus = gh auth status 2>&1
        if ($authStatus -match "Logged in") {
            return $true
        }
    }
    catch {
        return $false
    }
    return $false
}

function Invoke-GitHubLogin {
    Write-Header "GitHub Authentifizierung"
    
    if (Test-GitHubAuth) {
        Write-ColorOutput "OK: Bereits bei GitHub authentifiziert" -Type Success
        $showStatus = Read-Host "Moechten Sie den Auth-Status anzeigen? (j/n)"
        if ($showStatus -eq 'j') {
            Write-ColorOutput "`nAktueller Status:" -Type Info
            gh auth status
        }
        return $true
    }
    
    Write-ColorOutput "LOGIN: Starte GitHub Login..." -Type Info
    Write-ColorOutput @"

Waehlen Sie Ihre bevorzugte Authentifizierungsmethode:
1. Browser (Empfohlen) - Oeffnet GitHub im Browser
2. Token - Verwenden Sie ein Personal Access Token

"@ -Type Normal
    
    $choice = Read-Host "Ihre Wahl (1 oder 2)"
    
    try {
        switch ($choice) {
            "1" {
                Write-ColorOutput "`nBROWSER: Oeffne Browser fuer Authentifizierung..." -Type Info
                Write-ColorOutput "   Folgen Sie den Anweisungen im Browser" -Type Normal
                gh auth login --web --git-protocol https
            }
            "2" {
                Write-ColorOutput "`nTOKEN: Token-basierte Authentifizierung" -Type Info
                Write-ColorOutput @"
                
Sie benoetigen ein Personal Access Token mit folgenden Scopes:
- repo (Full control of private repositories)
- read:org (Read org and team membership)
- admin:public_key (Full control of user public keys)

Erstellen Sie ein Token unter: https://github.com/settings/tokens/new

"@ -Type Warning
                $token = Read-Host "Geben Sie Ihr Personal Access Token ein" -AsSecureString
                $tokenPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($token))
                
                # Token via stdin übergeben
                $tokenPlain | gh auth login --with-token --git-protocol https
            }
            default {
                Write-ColorOutput "ERROR: Ungueltige Auswahl" -Type Error
                return $false
            }
        }
        
        # Verifiziere Login
        Start-Sleep -Seconds 2
        if (Test-GitHubAuth) {
            Write-ColorOutput "`nOK: Erfolgreich bei GitHub authentifiziert!" -Type Success
            return $true
        }
        else {
            Write-ColorOutput "`nERROR: Authentifizierung fehlgeschlagen" -Type Error
            return $false
        }
    }
    catch {
        Write-ColorOutput "ERROR: Fehler bei der Authentifizierung: $_" -Type Error
        return $false
    }
}

function Show-RepositoryPreparation {
    Write-Header "Repository-Erstellung Vorbereitung"
    
    Write-ColorOutput "INFO: Sammle Informationen fuer Repository-Erstellung..." -Type Info
    
    # Account-Informationen
    try {
        Write-ColorOutput "`nACCOUNT: Account-Informationen:" -Type Info
        $viewer = gh api user --jq '.login, .name, .email' 2>$null
        if ($viewer) {
            $viewerLines = $viewer -split "`n"
            Write-ColorOutput "   Username: $($viewerLines[0])" -Type Normal
            if ($viewerLines[1]) {
                Write-ColorOutput "   Name: $($viewerLines[1])" -Type Normal
            }
            if ($viewerLines[2]) {
                Write-ColorOutput "   Email: $($viewerLines[2])" -Type Normal
            }
        }
    }
    catch {
        Write-ColorOutput "WARNING: Konnte Account-Details nicht abrufen" -Type Warning
    }
    
    # Repository-Vorschlag
    Write-ColorOutput "`nREPO: Repository-Details fuer MedEasy:" -Type Info
    Write-ColorOutput "   Name: medeasy" -Type Normal
    Write-ColorOutput "   Beschreibung: KI-gestuetzte medizinische Dokumentation" -Type Normal
    Write-ColorOutput "   Sichtbarkeit: Private" -Type Normal
    Write-ColorOutput "   .gitignore: VisualStudio" -Type Normal
    Write-ColorOutput "   Lizenz: Proprietary (oder MIT)" -Type Normal
    
    # Beispiel-Befehl
    Write-ColorOutput "`nTIPP: Repository-Erstellung Befehl:" -Type Info
    Write-ColorOutput @"
    
gh repo create medeasy \
  --private \
  --description "KI-gestuetzte medizinische Dokumentation" \
  --gitignore VisualStudio \
  --clone

"@ -Type Normal
}

function Get-Organizations {
    Write-Header "Verfuegbare Organisationen"
    
    try {
        Write-ColorOutput "ORGS: Lade Organisationen..." -Type Info
        
        # Hole Organisationen
        $orgs = gh api user/orgs --jq '.[].login' 2>$null
        
        if ($orgs) {
            Write-ColorOutput "`nOK: Gefundene Organisationen:" -Type Success
            $orgArray = $orgs -split "`n" | Where-Object { $_ }
            
            for ($i = 0; $i -lt $orgArray.Count; $i++) {
                Write-ColorOutput "   $($i + 1). $($orgArray[$i])" -Type Normal
            }
            
            Write-ColorOutput "`nTIPP: Sie koennen ein Repository in einer Organisation erstellen mit:" -Type Info
            Write-ColorOutput "   gh repo create <org-name>/medeasy --private ..." -Type Normal
        }
        else {
            Write-ColorOutput "INFO: Keine Organisationen gefunden" -Type Info
            Write-ColorOutput "   Das Repository wird unter Ihrem persoenlichen Account erstellt" -Type Normal
        }
        
        # Zeige auch Teams wenn in Orgs
        if ($orgs) {
            Write-ColorOutput "`nINFO: Fuer detaillierte Org-Informationen verwenden Sie:" -Type Info
            Write-ColorOutput "   gh org list" -Type Normal
        }
        
    }
    catch {
        Write-ColorOutput "WARNING: Konnte Organisationen nicht abrufen: $_" -Type Warning
    }
}

function Save-Configuration {
    Write-Header "Konfiguration speichern"
    
    $configPath = Join-Path $PSScriptRoot "github-cli-config.json"
    
    $config = @{
        SetupDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        GitHubCLIVersion = "Not installed"
        Authenticated = Test-GitHubAuth
        DefaultProtocol = "https"
        ConfigPath = $configPath
    }
    
    if (Test-GitHubCLI) {
        try {
            $ghVersionOutput = gh --version 2>$null
            if ($ghVersionOutput -and $ghVersionOutput -match 'gh version ([\d.]+)') {
                $config.GitHubCLIVersion = $Matches[1]
            }
            else {
                $config.GitHubCLIVersion = "Installed (version unknown)"
            }
        }
        catch {
            $config.GitHubCLIVersion = "Installed (version check failed)"
        }
    }
    
    try {
        $config | ConvertTo-Json -Depth 3 | Set-Content -Path $configPath -Encoding UTF8
        Write-ColorOutput "OK: Konfiguration gespeichert in: $configPath" -Type Success
    }
    catch {
        Write-ColorOutput "WARNING: Konnte Konfiguration nicht speichern: $_" -Type Warning
    }
}

# Hauptskript
function Main {
    Clear-Host
    Write-ColorOutput @"
+----------------------------------------------------------+
|            MedEasy - GitHub CLI Setup Script             |
|                    Version 1.0                           |
+----------------------------------------------------------+
"@ -Type Info

    # Schritt 1: Installation
    if (-not (Install-GitHubCLI)) {
        Write-ColorOutput "`nERROR: Setup abgebrochen - GitHub CLI Installation fehlgeschlagen" -Type Error
        exit 1
    }
    
    # Schritt 2: Authentifizierung
    if (-not (Invoke-GitHubLogin)) {
        Write-ColorOutput "`nWARNING: Setup fortgesetzt - Authentifizierung kann spaeter durchgefuehrt werden" -Type Warning
    }
    
    # Schritt 3: Repository-Vorbereitung
    Show-RepositoryPreparation
    
    # Schritt 4: Organisationen
    Get-Organizations
    
    # Schritt 5: Konfiguration speichern
    Save-Configuration
    
    # Abschluss
    Write-Header "Setup abgeschlossen"
    Write-ColorOutput "OK: GitHub CLI ist eingerichtet und bereit!" -Type Success
    Write-ColorOutput "`nNAECHSTE SCHRITTE:" -Type Info
    Write-ColorOutput "   1. Erstellen Sie das Repository mit: gh repo create" -Type Normal
    Write-ColorOutput "   2. Klonen Sie es mit: gh repo clone <repo-name>" -Type Normal
    Write-ColorOutput "   3. Konfigurieren Sie Branch-Protection" -Type Normal
    Write-ColorOutput "`nHILFE: gh --help oder https://cli.github.com/manual/" -Type Info
}

# Skript ausführen
Main