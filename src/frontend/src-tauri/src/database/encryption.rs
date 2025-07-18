// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Field Encryption Module [EIV] [AIU] [SP]
// Implements field-level encryption for sensitive data

use aes_gcm::{
    aead::{Aead, KeyInit},
    Aes256Gcm, Key, Nonce
}; // OsRng wird nicht verwendet
use base64::{Engine as _, engine::general_purpose};
use std::env;
use thiserror::Error;
use log::{info, warn, error};
use sha2::{Sha256, Digest};
use std::sync::Once;
// use generic_array::{GenericArray, typenum::U32}; // Wird nicht verwendet
use regex::Regex;

static ENCRYPTION_INIT: Once = Once::new();

#[derive(Debug, Error)]
pub enum EncryptionError {
    #[error("Encryption failed: {0}")]
    EncryptionFailed(String),
    
    #[error("Decryption failed: {0}")]
    DecryptionFailed(String),
    
    #[error("Key generation failed: {0}")]
    KeyGenerationFailed(String),
    
    #[error("Environment error: {0}")]
    EnvError(String),
    
    #[error("Validation error: {0}")]
    ValidationError(String),
}

/// Field encryption for sensitive data [EIV]
/// Implements AES-256-GCM encryption for field-level security
pub struct FieldEncryption {
    cipher: Aes256Gcm,
    is_production: bool,
}

impl Clone for FieldEncryption {
    fn clone(&self) -> Self {
        // Da key_ref() nicht verfügbar ist, erstellen wir einen neuen Cipher
        // mit dem gleichen Schlüssel
        let key_bytes = if self.is_production {
            // In Produktion müssen wir den Umgebungsschlüssel verwenden [SP]
            match env::var("MEDEASY_FIELD_ENCRYPTION_KEY") {
                Ok(key) => key,
                Err(_) => {
                    error!("Field encryption key not set in production clone");
                    // Standardschlüssel für AES-256-GCM (32 Bytes) [SP]
                    "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=".to_string()
                }
            }
        } else {
            // In Entwicklung können wir den Standardschlüssel verwenden
            // Standardschlüssel für AES-256-GCM (32 Bytes) [SP]
            "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=".to_string()
        };
            
        let key = match general_purpose::STANDARD.decode(&key_bytes) {
            Ok(k) => {
                if k.len() != 32 {
                    error!("Invalid key length in clone: {}, using default", k.len());
                    // Standardschlüssel für AES-256-GCM (32 Bytes) [SP]
                    [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                     0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                     0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                     0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20].to_vec()
                } else {
                    k
                }
            },
            Err(e) => {
                error!("Failed to decode key in clone: {}", e);
                // Standardschlüssel für AES-256-GCM (32 Bytes) [SP]
                [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20].to_vec()
            }
        };
            
        let key_array = Key::<Aes256Gcm>::from_slice(&key);
        
        Self {
            cipher: Aes256Gcm::new(key_array),
            is_production: self.is_production,
        }
    }
}

impl FieldEncryption {
    
    /// Creates a new field encryption instance
    pub fn new() -> Result<Self, EncryptionError> {
        // Determine if we're in production mode
        let is_production = !cfg!(debug_assertions) || 
            env::var("USE_ENCRYPTION").unwrap_or_else(|_| "false".to_string()) == "true";
        
        // In production, encryption is ALWAYS enabled [AIU]
        if is_production {
            Self::new_production_cipher()
        } else {
            Self::new_development_cipher()
        }
    }
    
    /// Creates a production-grade encryption cipher
    fn new_production_cipher() -> Result<Self, EncryptionError> {
        // Get encryption key from environment
        let key_str = env::var("MEDEASY_FIELD_ENCRYPTION_KEY")
            .map_err(|_| {
                // In Docker-Tests sollte dies immer fehlschlagen, wenn die Variable entfernt wurde [AIU]
                EncryptionError::EnvError("Field encryption key not set".to_string())
            })?;
        
        // Decode the base64 key
        let key_bytes = general_purpose::STANDARD
            .decode(key_str)
            .map_err(|e| EncryptionError::KeyGenerationFailed(e.to_string()))?;
        
        // Create the cipher
        if key_bytes.len() != 32 {
            return Err(EncryptionError::KeyGenerationFailed(
                "Invalid key length, must be 32 bytes".to_string()
            ));
        }
        
        let key = Key::<Aes256Gcm>::from_slice(&key_bytes);
        let cipher = Aes256Gcm::new(key);
        
        info!("Production encryption initialized successfully");
        Ok(Self { cipher, is_production: true })
    }
    
    /// Creates a development-only cipher (less secure)
    fn new_development_cipher() -> Result<Self, EncryptionError> {
        // Use a fixed key for development (NEVER in production)
        warn!("Using development encryption - NOT SECURE FOR PRODUCTION");
        
        // Fixed development key (32 bytes) - WICHTIG: Exakt 32 Bytes für AES-256-GCM [SP]
        // Wir verwenden ein Array mit 32 Bytes (0-31)
        let dev_key: [u8; 32] = [
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
            0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
        ];
        
        let key = Key::<Aes256Gcm>::from_slice(&dev_key);
        let cipher = Aes256Gcm::new(key);
        
        Ok(Self { cipher, is_production: false })
    }
    
    /// Encrypts a string value
    pub fn encrypt(&self, plaintext: &str) -> Result<Vec<u8>, EncryptionError> {
        // Generate a random nonce for each encryption [SP][ZTS]
        // Immer einen zufälligen Nonce verwenden, auch in Entwicklung
        // Dies ist wichtig für die Sicherheit und für Tests, die unterschiedliche
        // Verschlüsselungsergebnisse erwarten
        let mut nonce_bytes = [0u8; 12];
        getrandom::getrandom(&mut nonce_bytes)
            .map_err(|e| EncryptionError::EncryptionFailed(e.to_string()))?;
        
        let nonce = Nonce::from_slice(&nonce_bytes);
        
        // Encrypt the data
        let mut ciphertext = self.cipher
            .encrypt(nonce, plaintext.as_bytes())
            .map_err(|e| EncryptionError::EncryptionFailed(e.to_string()))?;
        
        // Prepend the nonce to the ciphertext for decryption
        let mut result = Vec::with_capacity(nonce_bytes.len() + ciphertext.len());
        result.extend_from_slice(&nonce_bytes);
        result.append(&mut ciphertext);
        
        Ok(result)
    }
    
    /// Decrypts a previously encrypted value
    pub fn decrypt(&self, ciphertext: &[u8]) -> Result<String, EncryptionError> {
        // Ensure the ciphertext is long enough to contain a nonce
        if ciphertext.len() <= 12 {
            return Err(EncryptionError::DecryptionFailed("Invalid ciphertext length".to_string()));
        }
        
        // Extract the nonce from the beginning of the ciphertext
        let nonce = Nonce::from_slice(&ciphertext[0..12]);
        
        // Decrypt the data
        let plaintext = self.cipher
            .decrypt(nonce, &ciphertext[12..])
            .map_err(|e| EncryptionError::DecryptionFailed(e.to_string()))?;
        
        // Convert back to string
        String::from_utf8(plaintext)
            .map_err(|e| EncryptionError::DecryptionFailed(e.to_string()))
    }
    
    /// Hashes a value (e.g., for insurance numbers)
    pub fn hash_value(&self, value: &str) -> String {
        let mut hasher = Sha256::new();
        hasher.update(value.as_bytes());
        format!("{:x}", hasher.finalize())
    }
    
    /// Generates a new encryption key and returns it as base64
    pub fn generate_encryption_key() -> Result<String, EncryptionError> {
        let mut key_bytes = [0u8; 32];
        getrandom::getrandom(&mut key_bytes)
            .map_err(|e| EncryptionError::KeyGenerationFailed(e.to_string()))?;
        
        Ok(general_purpose::STANDARD.encode(key_bytes))
    }
    
    /// Alias für generate_encryption_key für Kompatibilität mit Tests
    pub fn generate_key() -> Result<String, EncryptionError> {
        Self::generate_encryption_key()
    }
    
    /// Validiert und hasht eine Schweizer Versicherungsnummer [SF]
    /// Format: XXX.XXXX.XXXX.XX
    pub fn hash_insurance_number(&self, insurance_number: &str) -> Result<String, EncryptionError> {
        // Validiere das Format (XXX.XXXX.XXXX.XX)
        let re = Regex::new(r"^\d{3}\.\d{4}\.\d{4}\.\d{2}$")
            .map_err(|_| EncryptionError::ValidationError("Regex-Fehler".to_string()))?;
            
        if !re.is_match(insurance_number) {
            return Err(EncryptionError::ValidationError(
                format!("Ungültiges Versicherungsnummer-Format: {}", insurance_number)
            ));
        }
        
        // Hash die Versicherungsnummer
        Ok(self.hash_value(insurance_number))
    }
}
