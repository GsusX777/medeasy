<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Kann nicht deaktiviert werden
  [SP] SQLCipher Pflicht - Alle Patientendaten werden mit SQLCipher verschlüsselt
  [ATV] Audit-Trail Vollständig - Jede Operation wird geloggt
-->
<script lang="ts">
  import { appState } from '$lib/stores/session';
  import { invoke } from '@tauri-apps/api/tauri';
  
  // State
  let cloudConsentGiven = $appState.cloudConsentGiven;
  let processingLocation = $appState.processingLocation;
  let saving = false;
  let error: string | null = null;
  
  // [ZTS] Typsichere Event-Handler
  function handleCloudConsentChange(event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    updateCloudConsent(checkbox.checked);
  }
  
  // [CT] Cloud-Transparenz - Verarbeitungsort ändern
  async function updateProcessingLocation(location: 'local' | 'cloud') {
    if (location === 'cloud' && !cloudConsentGiven) {
      return;
    }
    
    try {
      saving = true;
      error = null;
      
      // In einer echten Implementierung würde dies über Tauri an das Backend gehen
      // await invoke('set_processing_location', { location });
      
      // Simulierte Verzögerung
      await new Promise(resolve => setTimeout(resolve, 500));
      
      processingLocation = location;
      
      // Store aktualisieren
      appState.update(state => ({
        ...state,
        processingLocation: location
      }));
      
      saving = false;
    } catch (err) {
      error = `Fehler beim Aktualisieren der Einstellungen: ${err}`;
      saving = false;
    }
  }
  
  // [CT] Cloud-Transparenz - Cloud-Einwilligung aktualisieren
  function updateCloudConsent(consent: boolean) {
    cloudConsentGiven = consent;
    
    // Wenn die Einwilligung zurückgezogen wird, auf lokal umschalten
    if (!consent && processingLocation === 'cloud') {
      updateProcessingLocation('local');
    }
    
    // Store aktualisieren
    appState.update(state => ({
      ...state,
      cloudConsentGiven: consent
    }));
  }
</script>

<div class="security-settings">
  <h3>Sicherheitseinstellungen</h3>
  
  <div class="settings-section">
    <h4>Datenverarbeitung</h4>
    <p class="section-description">
      Wählen Sie, wo Ihre Daten verarbeitet werden sollen.
    </p>
    
    <div class="setting-group">
      <div class="setting-option">
        <input 
          type="radio" 
          id="local-processing" 
          name="processing-location" 
          value="local"
          checked={processingLocation === 'local'}
          on:change={() => updateProcessingLocation('local')}
          disabled={saving}
        >
        <label for="local-processing">
          <span class="option-title">🔒 Lokale Verarbeitung</span>
          <span class="option-description">Daten werden nur auf diesem Gerät verarbeitet. Langsamer, aber privater.</span>
        </label>
      </div>
      
      <div class="setting-option" class:disabled={!cloudConsentGiven}>
        <input 
          type="radio" 
          id="cloud-processing" 
          name="processing-location" 
          value="cloud"
          checked={processingLocation === 'cloud'}
          on:change={() => updateProcessingLocation('cloud')}
          disabled={!cloudConsentGiven || saving}
        >
        <label for="cloud-processing" class:disabled={!cloudConsentGiven}>
          <span class="option-title">☁️ Cloud-Verarbeitung</span>
          <span class="option-description">Daten werden in der Cloud verarbeitet. Schneller, aber erfordert Datenübertragung.</span>
        </label>
      </div>
      
      <div class="cloud-consent">
        <input 
          type="checkbox" 
          id="cloud-consent" 
          checked={cloudConsentGiven}
          on:change={handleCloudConsentChange}
          disabled={saving}
        >
        <label for="cloud-consent">
          Ich stimme der Verarbeitung meiner Daten in der Cloud zu. Die Daten werden vor der Übertragung anonymisiert.
        </label>
      </div>
    </div>
  </div>
  
  <div class="settings-section">
    <h4>Sicherheitsfunktionen</h4>
    <p class="section-description">
      Diese Sicherheitsfunktionen sind gemäß Schweizer Datenschutzgesetz (nDSG) verpflichtend und können nicht deaktiviert werden.
    </p>
    
    <div class="security-features">
      <!-- [AIU] Anonymisierung ist UNVERÄNDERLICH -->
      <div class="feature-item enabled">
        <div class="feature-header">
          <span class="feature-title">Anonymisierung</span>
          <span class="feature-status">Aktiviert</span>
        </div>
        <p class="feature-description">
          Alle Patientendaten werden automatisch anonymisiert. Diese Funktion ist verpflichtend und kann nicht deaktiviert werden.
        </p>
      </div>
      
      <!-- [SP] SQLCipher Pflicht -->
      <div class="feature-item enabled">
        <div class="feature-header">
          <span class="feature-title">Datenverschlüsselung</span>
          <span class="feature-status">Aktiviert</span>
        </div>
        <p class="feature-description">
          Alle Patientendaten werden mit SQLCipher (AES-256) verschlüsselt. Diese Funktion ist verpflichtend und kann nicht deaktiviert werden.
        </p>
      </div>
      
      <!-- [ATV] Audit-Trail Vollständig -->
      <div class="feature-item enabled">
        <div class="feature-header">
          <span class="feature-title">Audit-Trail</span>
          <span class="feature-status">Aktiviert</span>
        </div>
        <p class="feature-description">
          Alle Datenbankoperationen, Zugriffe und Änderungen werden im Audit-Log erfasst. Diese Funktion ist verpflichtend und kann nicht deaktiviert werden.
        </p>
      </div>
    </div>
  </div>
  
  {#if error}
    <div class="error-message">
      {error}
    </div>
  {/if}
</div>

<style>
  .security-settings {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }
  
  h3 {
    margin: 0 0 1.5rem;
    font-size: 1.25rem;
    font-weight: 600;
    color: #1e293b;
  }
  
  .settings-section {
    margin-bottom: 2rem;
  }
  
  h4 {
    margin: 0 0 0.5rem;
    font-size: 1rem;
    font-weight: 600;
    color: #334155;
  }
  
  .section-description {
    margin: 0 0 1rem;
    font-size: 0.875rem;
    color: #64748b;
  }
  
  .setting-group {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .setting-option {
    display: flex;
    align-items: flex-start;
    gap: 0.75rem;
    padding: 0.75rem;
    border: 1px solid #e5e7eb;
    border-radius: 6px;
    background-color: #f9fafb;
  }
  
  .setting-option.disabled {
    opacity: 0.6;
  }
  
  .setting-option input[type="radio"] {
    margin-top: 0.25rem;
  }
  
  .setting-option label {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    cursor: pointer;
  }
  
  .setting-option label.disabled {
    cursor: not-allowed;
  }
  
  .option-title {
    font-weight: 500;
    color: #1e293b;
  }
  
  .option-description {
    font-size: 0.875rem;
    color: #64748b;
  }
  
  .cloud-consent {
    display: flex;
    align-items: flex-start;
    gap: 0.75rem;
    padding: 0.75rem;
    border: 1px solid #e5e7eb;
    border-radius: 6px;
    background-color: #fffbeb;
  }
  
  .cloud-consent input[type="checkbox"] {
    margin-top: 0.125rem;
  }
  
  .cloud-consent label {
    font-size: 0.875rem;
    color: #92400e;
    line-height: 1.4;
  }
  
  .security-features {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .feature-item {
    padding: 0.75rem;
    border: 1px solid #e5e7eb;
    border-radius: 6px;
  }
  
  .feature-item.enabled {
    background-color: #f0fdf4;
    border-color: #bbf7d0;
  }
  
  .feature-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.25rem;
  }
  
  .feature-title {
    font-weight: 500;
    color: #1e293b;
  }
  
  .feature-status {
    font-size: 0.75rem;
    font-weight: 500;
    padding: 0.125rem 0.375rem;
    border-radius: 4px;
    background-color: #dcfce7;
    color: #16a34a;
  }
  
  .feature-description {
    margin: 0;
    font-size: 0.875rem;
    color: #64748b;
  }
  
  .error-message {
    margin-top: 1rem;
    padding: 0.75rem;
    background-color: #fef2f2;
    border: 1px solid #fecaca;
    border-radius: 6px;
    color: #dc2626;
    font-size: 0.875rem;
  }
</style>
