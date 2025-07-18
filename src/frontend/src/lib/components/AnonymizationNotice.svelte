<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Die Anonymisierungsfunktion kann NIEMALS deaktiviert werden
-->
<script lang="ts">
  // Props
  export let showDetails = false;
</script>

<div class="anonymization-notice">
  <div class="notice-header">
    <span class="icon">🔒</span>
    <span class="title">Anonymisierung aktiv</span>
    <button class="toggle-details" on:click={() => showDetails = !showDetails} aria-label="Details anzeigen">
      {showDetails ? '▲' : '▼'}
    </button>
  </div>
  
  {#if showDetails}
    <div class="notice-details">
      <p>Alle Patientendaten werden automatisch anonymisiert. Diese Funktion ist gemäß Schweizer Datenschutzgesetz (nDSG) verpflichtend und kann nicht deaktiviert werden.</p>
      
      <h4>Anonymisierte Daten:</h4>
      <ul>
        <li>Patientennamen</li>
        <li>Geburtsdaten</li>
        <li>Versicherungsnummern</li>
        <li>Adressen</li>
        <li>Telefonnummern</li>
        <li>E-Mail-Adressen</li>
      </ul>
      
      <p class="disclaimer">Die Anonymisierung erfolgt mit einer Genauigkeit von >98%. Bitte überprüfen Sie kritische Daten dennoch auf Richtigkeit.</p>
    </div>
  {/if}
</div>

<style>
  .anonymization-notice {
    background-color: #f0f9ff;
    border: 1px solid #bae6fd;
    border-radius: 6px;
    padding: 0.75rem;
    margin: 1rem 0;
  }
  
  .notice-header {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .icon {
    font-size: 1.25rem;
  }
  
  .title {
    font-weight: 600;
    color: #0369a1;
    flex-grow: 1;
  }
  
  .toggle-details {
    background: none;
    border: none;
    color: #0369a1;
    cursor: pointer;
    font-size: 0.875rem;
    padding: 0.25rem;
  }
  
  .notice-details {
    margin-top: 0.75rem;
    padding-top: 0.75rem;
    border-top: 1px solid #bae6fd;
    font-size: 0.875rem;
  }
  
  .notice-details p {
    margin: 0.5rem 0;
  }
  
  .notice-details h4 {
    margin: 0.75rem 0 0.25rem;
    font-size: 0.875rem;
    font-weight: 600;
  }
  
  .notice-details ul {
    margin: 0.25rem 0;
    padding-left: 1.5rem;
  }
  
  .disclaimer {
    font-style: italic;
    color: #475569;
    margin-top: 0.75rem !important;
  }
</style>
