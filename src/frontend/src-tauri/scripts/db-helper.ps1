# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

# MedEasy Database Helper Script [SP] [AIU] [ATV]
# Provides utilities for managing the MedEasy SQLCipher database
# Usage: .\db-helper.ps1 [command] [options]

param (
    [Parameter(Position = 0)]
    [string]$Command = "help",
    
    [Parameter(Position = 1)]
    [string]$Environment = "development",
    
    [Parameter(Position = 2)]
    [string]$Option = ""
)

# Configuration
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$envFile = Join-Path $projectRoot ".env.$Environment"
$backupDir = Join-Path $projectRoot "db-backups"

# Ensure backup directory exists
if (-not (Test-Path $backupDir)) {
    New-Item -ItemType Directory -Path $backupDir | Out-Null
    Write-Host "Created backup directory: $backupDir"
}

# Load environment variables
function Import-EnvFile {
    param (
        [string]$FilePath
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-Error "Environment file not found: $FilePath"
        exit 1
    }
    
    $envVars = @{}
    Get-Content $FilePath | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            $envVars[$key] = $value
            # Set as environment variable for this session
            [Environment]::SetEnvironmentVariable($key, $value, "Process")
        }
    }
    
    return $envVars
}

# Get database path from environment variables
function Get-DatabasePath {
    param (
        [hashtable]$EnvVars
    )
    
    if ($EnvVars.ContainsKey("DATABASE_URL")) {
        $dbUrl = $EnvVars["DATABASE_URL"]
        # Extract file path from sqlite:// URL format
        if ($dbUrl -match '^sqlite:\/\/(.+)$') {
            return $matches[1]
        }
        return $dbUrl
    }
    
    # Default path if not specified
    return Join-Path $projectRoot "medeasy.$Environment.db"
}

# Check if encryption is enabled
function Test-EncryptionEnabled {
    param (
        [hashtable]$EnvVars
    )
    
    if ($EnvVars.ContainsKey("USE_ENCRYPTION")) {
        return [System.Convert]::ToBoolean($EnvVars["USE_ENCRYPTION"])
    }
    
    # Default to true for production, false for development
    return $Environment -eq "production"
}

# Get encryption key
function Get-EncryptionKey {
    param (
        [hashtable]$EnvVars
    )
    
    if ($EnvVars.ContainsKey("MEDEASY_DB_KEY")) {
        return $EnvVars["MEDEASY_DB_KEY"]
    }
    
    Write-Error "MEDEASY_DB_KEY not found in environment variables"
    exit 1
}

# Create a new database
function New-Database {
    param (
        [string]$DbPath,
        [bool]$UseEncryption,
        [string]$EncryptionKey
    )
    
    if (Test-Path $DbPath) {
        Write-Error "Database already exists: $DbPath"
        Write-Host "Use 'reset' command to recreate the database"
        exit 1
    }
    
    Write-Host "Creating new database: $DbPath"
    Write-Host "Encryption: $UseEncryption"
    
    # Create empty database file
    $null = New-Item -ItemType File -Path $DbPath -Force
    
    # Initialize with SQLite CLI if available
    $sqliteCmd = Get-Command sqlite3 -ErrorAction SilentlyContinue
    if ($sqliteCmd) {
        if ($UseEncryption) {
            # For SQLCipher, we need to set the key and pragmas
            $commands = @(
                "PRAGMA key = '$EncryptionKey';",
                "PRAGMA cipher_page_size = 4096;",
                "PRAGMA kdf_iter = 256000;",
                "PRAGMA cipher_memory_security = ON;",
                "PRAGMA cipher_default_kdf_algorithm = PBKDF2_HMAC_SHA512;",
                "PRAGMA cipher_default_plaintext_header_size = 32;",
                "PRAGMA cipher_hmac_algorithm = HMAC_SHA512;",
                "CREATE TABLE schema_migrations (version TEXT PRIMARY KEY, applied_at TEXT NOT NULL);"
            )
            
            $commandString = $commands -join " "
            Invoke-Expression "echo `"$commandString`" | sqlite3 `"$DbPath`""
        }
        else {
            # For regular SQLite
            $commands = @(
                "CREATE TABLE schema_migrations (version TEXT PRIMARY KEY, applied_at TEXT NOT NULL);"
            )
            
            $commandString = $commands -join " "
            Invoke-Expression "echo `"$commandString`" | sqlite3 `"$DbPath`""
        }
    }
    else {
        Write-Warning "sqlite3 command not found. Database created but not initialized."
        Write-Warning "Please run the application to initialize the database schema."
    }
    
    Write-Host "Database created successfully."
}

# Backup the database
function Backup-Database {
    param (
        [string]$DbPath,
        [bool]$UseEncryption,
        [string]$EncryptionKey
    )
    
    if (-not (Test-Path $DbPath)) {
        Write-Error "Database not found: $DbPath"
        exit 1
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = Join-Path $backupDir "medeasy_$Environment`_$timestamp.db"
    
    Write-Host "Backing up database to: $backupFile"
    
    # Simple file copy for backup
    Copy-Item -Path $DbPath -Destination $backupFile -Force
    
    Write-Host "Database backed up successfully."
    return $backupFile
}

# Reset the database (delete and recreate)
function Reset-Database {
    param (
        [string]$DbPath,
        [bool]$UseEncryption,
        [string]$EncryptionKey
    )
    
    if (Test-Path $DbPath) {
        # Backup before reset
        $backupFile = Backup-Database -DbPath $DbPath -UseEncryption $UseEncryption -EncryptionKey $EncryptionKey
        Write-Host "Backed up existing database to: $backupFile"
        
        # Delete existing database
        Remove-Item -Path $DbPath -Force
        Write-Host "Deleted existing database."
    }
    
    # Create new database
    New-Database -DbPath $DbPath -UseEncryption $UseEncryption -EncryptionKey $EncryptionKey
}

# Generate a new encryption key
function New-EncryptionKey {
    # Generate a random 32-byte key (for AES-256)
    $key = [byte[]]::new(32)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($key)
    
    # Convert to Base64 for storage
    $base64Key = [Convert]::ToBase64String($key)
    
    return $base64Key
}

# Show database info
function Show-DatabaseInfo {
    param (
        [string]$DbPath,
        [bool]$UseEncryption,
        [string]$EncryptionKey
    )
    
    if (-not (Test-Path $DbPath)) {
        Write-Error "Database not found: $DbPath"
        exit 1
    }
    
    Write-Host "Database Information:"
    Write-Host "---------------------"
    Write-Host "Path: $DbPath"
    Write-Host "Size: $((Get-Item $DbPath).Length / 1KB) KB"
    Write-Host "Last Modified: $((Get-Item $DbPath).LastWriteTime)"
    Write-Host "Encryption: $UseEncryption"
    
    # Try to get table count if SQLite CLI is available
    $sqliteCmd = Get-Command sqlite3 -ErrorAction SilentlyContinue
    if ($sqliteCmd) {
        if ($UseEncryption) {
            $tableCountCmd = "PRAGMA key = '$EncryptionKey'; SELECT count(*) FROM sqlite_master WHERE type='table';"
            $tableCount = Invoke-Expression "echo `"$tableCountCmd`" | sqlite3 `"$DbPath`""
            Write-Host "Tables: $tableCount"
        }
        else {
            $tableCount = Invoke-Expression "echo `"SELECT count(*) FROM sqlite_master WHERE type='table';`" | sqlite3 `"$DbPath`""
            Write-Host "Tables: $tableCount"
        }
    }
}

# Show help
function Show-Help {
    Write-Host "MedEasy Database Helper Script"
    Write-Host "Usage: .\db-helper.ps1 [command] [environment] [option]"
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "  help        - Show this help message"
    Write-Host "  create      - Create a new database"
    Write-Host "  backup      - Backup the database"
    Write-Host "  reset       - Reset the database (delete and recreate)"
    Write-Host "  info        - Show database information"
    Write-Host "  genkey      - Generate a new encryption key"
    Write-Host ""
    Write-Host "Environments:"
    Write-Host "  development - Development environment (default)"
    Write-Host "  production  - Production environment"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\db-helper.ps1 create development"
    Write-Host "  .\db-helper.ps1 backup production"
    Write-Host "  .\db-helper.ps1 reset development"
    Write-Host "  .\db-helper.ps1 info production"
    Write-Host "  .\db-helper.ps1 genkey"
}

# Main script execution
try {
    # Load environment variables
    $envVars = Load-EnvFile -FilePath $envFile
    $dbPath = Get-DatabasePath -EnvVars $envVars
    $useEncryption = Is-EncryptionEnabled -EnvVars $envVars
    
    # Force encryption in production [SP]
    if ($Environment -eq "production" -and -not $useEncryption) {
        Write-Warning "Encryption is mandatory in production environment."
        $useEncryption = $true
    }
    
    # Get encryption key if needed
    $encryptionKey = ""
    if ($useEncryption) {
        $encryptionKey = Get-EncryptionKey -EnvVars $envVars
    }
    
    # Execute command
    switch ($Command.ToLower()) {
        "create" {
            New-Database -DbPath $dbPath -UseEncryption $useEncryption -EncryptionKey $encryptionKey
        }
        "backup" {
            Backup-Database -DbPath $dbPath -UseEncryption $useEncryption -EncryptionKey $encryptionKey
        }
        "reset" {
            Reset-Database -DbPath $dbPath -UseEncryption $useEncryption -EncryptionKey $encryptionKey
        }
        "info" {
            Show-DatabaseInfo -DbPath $dbPath -UseEncryption $useEncryption -EncryptionKey $encryptionKey
        }
        "genkey" {
            $newKey = New-EncryptionKey
            Write-Host "Generated new encryption key:"
            Write-Host $newKey
            Write-Host ""
            Write-Host "Add this to your .env file as MEDEASY_DB_KEY and MEDEASY_FIELD_ENCRYPTION_KEY"
        }
        default {
            Show-Help
        }
    }
}
catch {
    Write-Error "Error: $_"
    exit 1
}
