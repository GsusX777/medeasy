# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

<#
.SYNOPSIS
    MedEasy KP100 Sicherheitstests ausführen [KP100][SP][AIU][ATV]
.DESCRIPTION
    Dieses Skript baut und führt die MedEasy KP100 Sicherheitstests in einer Docker-Umgebung aus.
    Es stellt sicher, dass alle kritischen Sicherheitsfunktionen korrekt funktionieren.
.NOTES
    Autor: MedEasy Team
    Version: 1.0
    Datum: 10.07.2025
#>

# Konfiguration
$ErrorActionPreference = "Stop"
$DockerfilePath = "Dockerfile.test"
$ImageName = "medeasy-security-tests"
$ContainerName = "medeasy-security-tests-container"

# Farben für die Ausgabe
$Green = [ConsoleColor]::Green
$Red = [ConsoleColor]::Red
#$Yellow = [ConsoleColor]::Yellow
$Cyan = [ConsoleColor]::Cyan

# Funktion zum Anzeigen von Nachrichten
function Write-Status {
    param (
        [string]$Message,
        [ConsoleColor]$Color = [ConsoleColor]::White
    )
    Write-Host $Message -ForegroundColor $Color
}

# Prüfe, ob Docker installiert ist
try {
    docker --version | Out-Null
    Write-Status "Docker ist installiert." -Color $Green
}
catch {
    Write-Status "Docker ist nicht installiert oder nicht im PATH. Bitte installieren Sie Docker." -Color $Red
    exit 1
}

# Bereinige alte Container und Images
Write-Status "Bereinige alte Container und Images..." -Color $Cyan
docker rm -f $ContainerName 2>$null
docker rmi -f $ImageName 2>$null

# Baue das Docker-Image
Write-Status "Baue Docker-Image für Sicherheitstests..." -Color $Cyan
docker build -t $ImageName -f $DockerfilePath .

if ($LASTEXITCODE -ne 0) {
    Write-Status "Fehler beim Bauen des Docker-Images." -Color $Red
    exit 1
}

Write-Status "Docker-Image erfolgreich gebaut." -Color $Green

# Führe die Tests aus
Write-Status "Führe Sicherheitstests aus..." -Color $Cyan
docker run --name $ContainerName $ImageName

if ($LASTEXITCODE -ne 0) {
    Write-Status "Einige Tests sind fehlgeschlagen. Bitte überprüfen Sie die Ausgabe." -Color $Red
    exit 1
}

Write-Status "Alle Sicherheitstests erfolgreich bestanden! [KP100]" -Color $Green

# Bereinige nach den Tests
Write-Status "Bereinige Container..." -Color $Cyan
docker rm -f $ContainerName 2>$null

Write-Status "Sicherheitstests abgeschlossen." -Color $Green
