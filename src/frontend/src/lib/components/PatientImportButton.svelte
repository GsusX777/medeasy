<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [ZTS] Zero Tolerance Security - Import-Funktion für später vorbereitet
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Import muss Anonymisierung respektieren
  [SF] Schweizer Formate - Import muss Schweizer Datenformate unterstützen
-->
<script lang="ts">
  export let disabled = true;
  
  function handleImportClick() {
    if (disabled) return;
    
    // Zukünftige Implementierung:
    // - Datei-Upload Dialog
    // - CSV/Excel Import
    // - Datenvalidierung [SF]
    // - Automatische Anonymisierung [AIU]
    // - Verschlüsselung [EIV]
    console.log('Patient Import - Feature kommt später');
  }
</script>

<button 
  class="import-button"
  class:disabled
  on:click={handleImportClick}
  {disabled}
  title={disabled ? 'Import-Funktion wird in einer späteren Version verfügbar sein' : 'Patienten aus Datei importieren'}
>
  <span class="icon">📥</span>
  <span class="label">Import</span>
  {#if disabled}
    <span class="beta-badge">Bald</span>
  {/if}
</button>

<style>
  .import-button {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 16px;
    background-color: #f9fafb;
    color: #6b7280;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: all 0.2s ease;
    position: relative;
  }
  
  .import-button:not(.disabled):hover {
    background-color: #f3f4f6;
    color: #374151;
  }
  
  .import-button.disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
  
  .icon {
    font-size: 16px;
  }
  
  .label {
    font-weight: 500;
  }
  
  .beta-badge {
    background-color: #fbbf24;
    color: #92400e;
    font-size: 10px;
    font-weight: 600;
    padding: 2px 6px;
    border-radius: 8px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  /* Responsive Design [PSF] */
  @media (max-width: 768px) {
    .import-button {
      padding: 6px 12px;
      font-size: 13px;
    }
    
    .label {
      display: none;
    }
  }
</style>
