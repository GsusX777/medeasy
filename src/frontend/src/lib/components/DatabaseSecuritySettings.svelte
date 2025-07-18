<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- MedEasy Datenbanksicherheitseinstellungen [SP][AIU][ATV] -->
<script lang="ts">
  import { onMount } from 'svelte';
  import { invoke } from '@tauri-apps/api/tauri';
  import { initDb, databaseInitialized, databaseError } from '$lib/stores/database';

  // Security settings state
  let encryptionStatus = "Unbekannt";
  let encryptionEnabled = true;
  let anonymizationEnabled = true;
  let auditEnabled = true;
  let isProduction = false;
  let loading = true;
  let error: string | null = null;
  let isAdmin = false;
  let encryptionKeyStatus = "Unbekannt";
  let fieldEncryptionKeyStatus = "Unbekannt";

  // Konstanten für Umgebungsvariablen
  const ENV_USE_ENCRYPTION = "USE_ENCRYPTION";
  const ENV_ENFORCE_ANONYMIZATION = "ENFORCE_ANONYMIZATION";
  const ENV_ENFORCE_AUDIT = "ENFORCE_AUDIT";
  const ENV_MEDEASY_DB_KEY = "MEDEASY_DB_KEY";
  const ENV_MEDEASY_FIELD_ENCRYPTION_KEY = "MEDEASY_FIELD_ENCRYPTION_KEY";

  onMount(async () => {
    try {
      // In a real implementation, this would come from authentication
      isAdmin = localStorage.getItem('medeasy_is_admin') === 'true';
      
      // Check if we're in production
      isProduction = await invoke('is_production_environment');
      
      // Get security settings
      await loadSecuritySettings();
      
      // Initialize database
      await initDb();
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
    } finally {
      loading = false;
    }
  });

  /**
   * Load current security settings
   */
  async function loadSecuritySettings(): Promise<void> {
    try {
      // Get encryption status
      const encryptionSetting = await invoke('get_environment_variable', { name: ENV_USE_ENCRYPTION });
      encryptionEnabled = encryptionSetting === 'true';
      encryptionStatus = encryptionEnabled ? "Aktiviert" : "Deaktiviert";
      
      // In production, encryption is always enabled [SP]
      if (isProduction && !encryptionEnabled) {
        encryptionEnabled = true;
        encryptionStatus = "Aktiviert (erzwungen in Produktion)";
      }
      
      // Get anonymization status - always enabled [AIU]
      const anonymizationSetting = await invoke('get_environment_variable', { name: ENV_ENFORCE_ANONYMIZATION });
      anonymizationEnabled = anonymizationSetting !== 'false';
      
      // Get audit status
      const auditSetting = await invoke('get_environment_variable', { name: ENV_ENFORCE_AUDIT });
      auditEnabled = auditSetting !== 'false';
      
      // In production, audit is always enabled [ATV]
      if (isProduction && !auditEnabled) {
        auditEnabled = true;
      }
      
      // Check encryption keys
      await checkEncryptionKeys();
    } catch (err) {
      console.error('Failed to load security settings:', err);
      throw err;
    }
  }

  /**
   * Check if encryption keys are set
   */
  async function checkEncryptionKeys(): Promise<void> {
    try {
      // Check database encryption key
      const dbKeyExists = await invoke('check_environment_variable_exists', { name: ENV_MEDEASY_DB_KEY });
      encryptionKeyStatus = dbKeyExists ? "Gesetzt" : "Nicht gesetzt";
      
      // Check field encryption key
      const fieldKeyExists = await invoke('check_environment_variable_exists', { name: ENV_MEDEASY_FIELD_ENCRYPTION_KEY });
      fieldEncryptionKeyStatus = fieldKeyExists ? "Gesetzt" : "Nicht gesetzt";
    } catch (err) {
      console.error('Failed to check encryption keys:', err);
      throw err;
    }
  }

  /**
   * Toggle encryption setting
   * Only allowed in development mode [SP]
   */
  async function toggleEncryption(): Promise<void> {
    if (isProduction) {
      error = "Verschlüsselung kann in der Produktionsumgebung nicht deaktiviert werden [SP]";
      return;
    }
    
    try {
      loading = true;
      
      // Toggle encryption setting
      encryptionEnabled = !encryptionEnabled;
      await invoke('set_environment_variable', { 
        name: ENV_USE_ENCRYPTION, 
        value: encryptionEnabled.toString() 
      });
      
      encryptionStatus = encryptionEnabled ? "Aktiviert" : "Deaktiviert";
      
      // Reload database connection to apply changes
      await initDb();
      
      // Log this action in the audit trail
      await invoke('create_audit_log', {
        entityName: 'security_settings',
        entityId: 'encryption',
        action: 'UPDATE',
        changes: JSON.stringify({ encryption_enabled: encryptionEnabled }),
        containsSensitiveData: true,
        userId: localStorage.getItem('medeasy_user_id') || 'default_user'
      });
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
      // Revert UI state if operation failed
      encryptionEnabled = !encryptionEnabled;
    } finally {
      loading = false;
    }
  }

  /**
   * Toggle audit setting
   * Only allowed in development mode [ATV]
   */
  async function toggleAudit(): Promise<void> {
    if (isProduction) {
      error = "Audit-Trail kann in der Produktionsumgebung nicht deaktiviert werden [ATV]";
      return;
    }
    
    try {
      loading = true;
      
      // Toggle audit setting
      auditEnabled = !auditEnabled;
      await invoke('set_environment_variable', { 
        name: ENV_ENFORCE_AUDIT, 
        value: auditEnabled.toString() 
      });
      
      // Log this action in the audit trail (if still enabled)
      if (auditEnabled) {
        await invoke('create_audit_log', {
          entityName: 'security_settings',
          entityId: 'audit',
          action: 'UPDATE',
          changes: JSON.stringify({ audit_enabled: auditEnabled }),
          containsSensitiveData: true,
          userId: localStorage.getItem('medeasy_user_id') || 'default_user'
        });
      }
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
      // Revert UI state if operation failed
      auditEnabled = !auditEnabled;
    } finally {
      loading = false;
    }
  }

  /**
   * Generate new encryption keys
   * This is a sensitive operation and should be carefully controlled
   */
  async function generateNewKeys(): Promise<void> {
    if (!confirm('WARNUNG: Das Generieren neuer Schlüssel kann dazu führen, dass bestehende Daten nicht mehr entschlüsselt werden können. Sind Sie sicher, dass Sie fortfahren möchten?')) {
      return;
    }
    
    try {
      loading = true;
      error = null;
      
      // Generate new database encryption key
      const newDbKey = await invoke('generate_encryption_key', { length: 32 });
      await invoke('set_environment_variable', { 
        name: ENV_MEDEASY_DB_KEY, 
        value: newDbKey 
      });
      
      // Generate new field encryption key
      const newFieldKey = await invoke('generate_encryption_key', { length: 32 });
      await invoke('set_environment_variable', { 
        name: ENV_MEDEASY_FIELD_ENCRYPTION_KEY, 
        value: newFieldKey 
      });
      
      // Update key status
      encryptionKeyStatus = "Gesetzt (neu)";
      fieldEncryptionKeyStatus = "Gesetzt (neu)";
      
      // Log this action in the audit trail
      await invoke('create_audit_log', {
        entityName: 'security_settings',
        entityId: 'encryption_keys',
        action: 'UPDATE',
        changes: JSON.stringify({ keys_regenerated: true }),
        containsSensitiveData: true,
        userId: localStorage.getItem('medeasy_user_id') || 'default_user'
      });
      
      // Reload database connection to apply changes
      await initDb();
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
    } finally {
      loading = false;
    }
  }

  /**
   * Export encryption keys (for backup)
   * This is a sensitive operation and should be carefully controlled
   */
  async function exportKeys(): Promise<void> {
    if (!confirm('WARNUNG: Das Exportieren von Verschlüsselungsschlüsseln ist eine sicherheitskritische Operation. Stellen Sie sicher, dass Sie die exportierten Schlüssel sicher aufbewahren. Fortfahren?')) {
      return;
    }
    
    try {
      loading = true;
      error = null;
      
      // Get encryption keys
      const dbKey = await invoke('get_environment_variable', { name: ENV_MEDEASY_DB_KEY });
      const fieldKey = await invoke('get_environment_variable', { name: ENV_MEDEASY_FIELD_ENCRYPTION_KEY });
      
      // Create export data
      const exportData = {
        db_key: dbKey,
        field_key: fieldKey,
        export_date: new Date().toISOString(),
        environment: isProduction ? 'production' : 'development'
      };
      
      // Export as JSON file
      const jsonData = JSON.stringify(exportData, null, 2);
      const blob = new Blob([jsonData], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      
      // Create download link
      const a = document.createElement('a');
      a.href = url;
      a.download = `medeasy_encryption_keys_${new Date().toISOString().replace(/:/g, '-')}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      
      // Log this action in the audit trail
      await invoke('create_audit_log', {
        entityName: 'security_settings',
        entityId: 'encryption_keys',
        action: 'READ',
        changes: JSON.stringify({ keys_exported: true }),
        containsSensitiveData: true,
        userId: localStorage.getItem('medeasy_user_id') || 'default_user'
      });
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
    } finally {
      loading = false;
    }
  }

  /**
   * Import encryption keys
   * This is a sensitive operation and should be carefully controlled
   */
  async function importKeys(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    
    if (!confirm('WARNUNG: Das Importieren von Verschlüsselungsschlüsseln kann dazu führen, dass bestehende Daten nicht mehr entschlüsselt werden können. Sind Sie sicher, dass Sie fortfahren möchten?')) {
      input.value = '';
      return;
    }
    
    try {
      loading = true;
      error = null;
      
      // Read the file
      const file = input.files[0];
      const text = await file.text();
      const keys = JSON.parse(text);
      
      // Validate keys
      if (!keys.db_key || !keys.field_key) {
        throw new Error('Ungültiges Schlüsselformat');
      }
      
      // Set encryption keys
      await invoke('set_environment_variable', { 
        name: ENV_MEDEASY_DB_KEY, 
        value: keys.db_key 
      });
      
      await invoke('set_environment_variable', { 
        name: ENV_MEDEASY_FIELD_ENCRYPTION_KEY, 
        value: keys.field_key 
      });
      
      // Update key status
      encryptionKeyStatus = "Gesetzt (importiert)";
      fieldEncryptionKeyStatus = "Gesetzt (importiert)";
      
      // Log this action in the audit trail
      await invoke('create_audit_log', {
        entityName: 'security_settings',
        entityId: 'encryption_keys',
        action: 'UPDATE',
        changes: JSON.stringify({ keys_imported: true }),
        containsSensitiveData: true,
        userId: localStorage.getItem('medeasy_user_id') || 'default_user'
      });
      
      // Reload database connection to apply changes
      await initDb();
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
    } finally {
      loading = false;
      // Reset input
      input.value = '';
    }
  }
</script>

<div class="database-security">
  <h2>Datenbanksicherheitseinstellungen</h2>
  
  {#if loading && !$databaseInitialized}
    <div class="loading">
      <span class="loader"></span>
      <p>Lade Sicherheitseinstellungen...</p>
    </div>
  {:else if error || $databaseError}
    <div class="error">
      <p>{error || $databaseError}</p>
      <button on:click={() => { error = null; loadSecuritySettings(); }}>
        Erneut versuchen
      </button>
    </div>
  {:else}
    <div class="settings-container">
      <div class="setting-card">
        <div class="setting-header">
          <h3>SQLCipher Datenbankverschlüsselung [SP]</h3>
          <span class="status {encryptionEnabled ? 'enabled' : 'disabled'}">
            {encryptionStatus}
          </span>
        </div>
        <div class="setting-content">
          <p>
            Die SQLCipher-Verschlüsselung verwendet AES-256 zur Verschlüsselung der gesamten Datenbank.
            {#if isProduction}
              In der Produktionsumgebung ist die Verschlüsselung immer aktiviert und kann nicht deaktiviert werden.
            {:else}
              In der Entwicklungsumgebung kann die Verschlüsselung deaktiviert werden, um die Entwicklung zu erleichtern.
            {/if}
          </p>
          <div class="key-status">
            <span>Datenbankschlüssel: <strong>{encryptionKeyStatus}</strong></span>
          </div>
          {#if !isProduction}
            <button 
              on:click={toggleEncryption} 
              disabled={loading}
              class={encryptionEnabled ? 'danger' : 'success'}
            >
              {encryptionEnabled ? 'Verschlüsselung deaktivieren' : 'Verschlüsselung aktivieren'}
            </button>
          {/if}
        </div>
      </div>
      
      <div class="setting-card">
        <div class="setting-header">
          <h3>Feldverschlüsselung [EIV]</h3>
          <span class="status enabled">Aktiviert</span>
        </div>
        <div class="setting-content">
          <p>
            Die Feldverschlüsselung verwendet AES-256-GCM zur Verschlüsselung sensibler Daten wie Namen, Notizen und Pfade.
            Diese Verschlüsselung ist immer aktiviert und kann nicht deaktiviert werden.
          </p>
          <div class="key-status">
            <span>Feldverschlüsselungsschlüssel: <strong>{fieldEncryptionKeyStatus}</strong></span>
          </div>
        </div>
      </div>
      
      <div class="setting-card">
        <div class="setting-header">
          <h3>Anonymisierung [AIU]</h3>
          <span class="status enabled">Immer aktiviert</span>
        </div>
        <div class="setting-content">
          <p>
            Die Anonymisierung ist eine Kernfunktion von MedEasy und kann nicht deaktiviert werden.
            Alle Transkripte müssen anonymisiert werden, bevor sie gespeichert werden können.
            Transkripte mit niedriger Anonymisierungskonfidenz werden automatisch zur Überprüfung markiert.
          </p>
        </div>
      </div>
      
      <div class="setting-card">
        <div class="setting-header">
          <h3>Audit-Trail [ATV]</h3>
          <span class="status {auditEnabled ? 'enabled' : 'disabled'}">
            {isProduction ? 'Immer aktiviert' : (auditEnabled ? 'Aktiviert' : 'Deaktiviert')}
          </span>
        </div>
        <div class="setting-content">
          <p>
            Der Audit-Trail protokolliert alle Datenbankoperationen, einschließlich Lese-, Schreib-, Aktualisierungs- und Löschvorgänge.
            {#if isProduction}
              In der Produktionsumgebung ist der Audit-Trail immer aktiviert und kann nicht deaktiviert werden.
            {:else}
              In der Entwicklungsumgebung kann der Audit-Trail deaktiviert werden, um die Entwicklung zu erleichtern.
            {/if}
          </p>
          {#if !isProduction}
            <button 
              on:click={toggleAudit} 
              disabled={loading}
              class={auditEnabled ? 'danger' : 'success'}
            >
              {auditEnabled ? 'Audit-Trail deaktivieren' : 'Audit-Trail aktivieren'}
            </button>
          {/if}
        </div>
      </div>
      
      {#if isAdmin}
        <div class="setting-card admin">
          <div class="setting-header">
            <h3>Schlüsselverwaltung (Admin)</h3>
            <span class="admin-badge">Admin</span>
          </div>
          <div class="setting-content">
            <p class="warning">
              WARNUNG: Die folgenden Operationen sind sicherheitskritisch und sollten nur von autorisierten Administratoren durchgeführt werden.
              Das Ändern von Verschlüsselungsschlüsseln kann dazu führen, dass bestehende Daten nicht mehr entschlüsselt werden können.
            </p>
            
            <div class="key-actions">
              <button 
                on:click={generateNewKeys} 
                disabled={loading}
                class="danger"
              >
                Neue Schlüssel generieren
              </button>
              
              <button 
                on:click={exportKeys} 
                disabled={loading}
                class="warning"
              >
                Schlüssel exportieren
              </button>
              
              <div class="import-container">
                <label for="import-keys" class="import-label">
                  Schlüssel importieren
                </label>
                <input 
                  type="file" 
                  id="import-keys" 
                  accept=".json"
                  on:change={importKeys}
                  disabled={loading}
                />
              </div>
            </div>
          </div>
        </div>
      {/if}
      
      <div class="database-status">
        <h3>Datenbankstatus</h3>
        <p>
          Status: 
          <span class="status {$databaseInitialized ? 'enabled' : 'disabled'}">
            {$databaseInitialized ? 'Initialisiert' : 'Nicht initialisiert'}
          </span>
        </p>
        {#if !$databaseInitialized && !$databaseError}
          <button on:click={() => initDb()} disabled={loading}>
            Datenbank initialisieren
          </button>
        {/if}
      </div>
    </div>
  {/if}
</div>

<style>
  .database-security {
    width: 100%;
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
  }
  
  h2 {
    color: #2c3e50;
    margin-bottom: 20px;
    border-bottom: 2px solid #3498db;
    padding-bottom: 10px;
  }
  
  h3 {
    color: #2c3e50;
    margin: 0;
  }
  
  .loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 0;
  }
  
  .loader {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #3498db;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 20px;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  .error {
    background-color: #ffecec;
    color: #e74c3c;
    padding: 15px;
    border-radius: 5px;
    margin: 20px 0;
    border-left: 4px solid #e74c3c;
  }
  
  .settings-container {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }
  
  .setting-card {
    background-color: #f8f9fa;
    border-radius: 5px;
    overflow: hidden;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  
  .setting-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 15px;
    background-color: #ecf0f1;
    border-bottom: 1px solid #ddd;
  }
  
  .setting-content {
    padding: 15px;
  }
  
  .status {
    display: inline-block;
    padding: 5px 10px;
    border-radius: 3px;
    font-size: 0.8rem;
    font-weight: bold;
    color: white;
  }
  
  .status.enabled {
    background-color: #2ecc71;
  }
  
  .status.disabled {
    background-color: #e74c3c;
  }
  
  .key-status {
    background-color: #eaf2f8;
    padding: 10px;
    border-radius: 3px;
    margin: 10px 0;
  }
  
  .setting-card.admin {
    background-color: #f8f5ff;
    border: 1px solid #8e44ad;
  }
  
  .admin-badge {
    background-color: #8e44ad;
    color: white;
    padding: 5px 10px;
    border-radius: 3px;
    font-size: 0.8rem;
    font-weight: bold;
  }
  
  .warning {
    background-color: #fff5e6;
    border-left: 4px solid #f39c12;
    padding: 10px;
    margin: 10px 0;
    color: #c0392b;
  }
  
  .key-actions {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-top: 15px;
  }
  
  button {
    padding: 8px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    background-color: #3498db;
    color: white;
    font-weight: bold;
    transition: background-color 0.3s;
  }
  
  button:hover {
    background-color: #2980b9;
  }
  
  button:disabled {
    background-color: #bdc3c7;
    cursor: not-allowed;
  }
  
  button.success {
    background-color: #2ecc71;
  }
  
  button.success:hover {
    background-color: #27ae60;
  }
  
  button.danger {
    background-color: #e74c3c;
  }
  
  button.danger:hover {
    background-color: #c0392b;
  }
  
  button.warning {
    background-color: #f39c12;
  }
  
  button.warning:hover {
    background-color: #e67e22;
  }
  
  .import-container {
    position: relative;
    overflow: hidden;
  }
  
  .import-label {
    display: inline-block;
    padding: 8px 15px;
    border-radius: 4px;
    cursor: pointer;
    background-color: #9b59b6;
    color: white;
    font-weight: bold;
    transition: background-color 0.3s;
  }
  
  .import-label:hover {
    background-color: #8e44ad;
  }
  
  input[type="file"] {
    position: absolute;
    left: 0;
    top: 0;
    opacity: 0;
    width: 100%;
    height: 100%;
    cursor: pointer;
  }
  
  .database-status {
    background-color: #f8f9fa;
    padding: 15px;
    border-radius: 5px;
    margin-top: 20px;
    border-left: 4px solid #3498db;
  }
</style>
