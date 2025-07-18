<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [SDH] Schweizerdeutsch-Handling - Erkennung von Schweizerdeutsch mit Warnung
-->
<script lang="ts">
  // Props
  export let detected = false;
  export let onDismiss: () => void = () => {};
</script>

{#if detected}
  <div class="swiss-german-alert">
    <div class="alert-content">
      <div class="alert-icon">⚠️</div>
      <div class="alert-text">
        <h4>Schweizerdeutsch erkannt</h4>
        <p>Die Transkription von Schweizerdeutsch hat eine eingeschränkte Genauigkeit. Für optimale Ergebnisse empfehlen wir Hochdeutsch.</p>
      </div>
    </div>
    <div class="alert-actions">
      <span class="beta-badge">BETA</span>
      <button class="dismiss-button" on:click={onDismiss} aria-label="Hinweis schliessen">
        ✕
      </button>
    </div>
  </div>
{/if}

<style>
  .swiss-german-alert {
    display: flex;
    justify-content: space-between;
    background-color: #fff7e6;
    border: 1px solid #ffe7ba;
    border-left: 4px solid #fa8c16;
    border-radius: 6px;
    padding: 0.75rem 1rem;
    margin: 1rem 0;
  }
  
  .alert-content {
    display: flex;
    gap: 0.75rem;
    align-items: flex-start;
  }
  
  .alert-icon {
    font-size: 1.25rem;
    margin-top: 0.125rem;
  }
  
  .alert-text h4 {
    margin: 0 0 0.25rem;
    font-size: 0.9375rem;
    font-weight: 600;
    color: #d46b08;
  }
  
  .alert-text p {
    margin: 0;
    font-size: 0.875rem;
    color: #664d03;
  }
  
  .alert-actions {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  
  .beta-badge {
    background-color: #fa8c16;
    color: white;
    font-size: 0.6875rem;
    font-weight: 600;
    padding: 0.125rem 0.375rem;
    border-radius: 3px;
    letter-spacing: 0.05em;
  }
  
  .dismiss-button {
    background: none;
    border: none;
    color: #d46b08;
    cursor: pointer;
    font-size: 0.875rem;
    padding: 0.25rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }
</style>
