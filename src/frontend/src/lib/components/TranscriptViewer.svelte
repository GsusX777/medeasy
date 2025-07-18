<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [AIU] Anonymisierung ist UNVERÄNDERLICH
  [ARQ] Anonymisierungs-Review-Queue
  [MFD] Medizinische Fachbegriffe DE-CH
-->
<script lang="ts">
  import { onMount } from 'svelte';
  
  // Props
  export let sessionId: string | null = null;
  export let transcript: string = '';
  export let loading: boolean = false;
  
  // State
  let anonymizedText: string = '';
  let reviewItems: {text: string, confidence: number, type: string}[] = [];
  
  // Simulierte Anonymisierungsfunktion
  // In einer echten Implementierung würde dies vom Backend kommen
  function processAnonymization(text: string): {text: string, reviewItems: any[]} {
    // [AIU] Anonymisierung ist UNVERÄNDERLICH - Kann nicht deaktiviert werden
    if (!text) return {text: '', reviewItems: []};
    
    // Einfache Simulation der Anonymisierung
    let processed = text;
    const items = [];
    
    // Patientennamen anonymisieren
    processed = processed.replace(/\b(Hans|Peter|Anna|Maria|Müller|Schmidt|Weber)\b/g, '[PATIENT]');
    
    // Adressen anonymisieren
    processed = processed.replace(/\b(Hauptstrasse|Bahnhofstrasse|Dorfstrasse)\s+\d+\b/g, '[ADRESSE]');
    
    // Telefonnummern anonymisieren
    processed = processed.replace(/\b0\d{2}\s?\d{3}\s?\d{2}\s?\d{2}\b/g, '[TELEFON]');
    
    // [ARQ] Anonymisierungs-Review-Queue - Unsichere Erkennungen
    // Beispiel für Elemente mit niedriger Konfidenz (<80%)
    if (text.includes('Spital')) {
      items.push({
        text: 'Spital',
        confidence: 75,
        type: 'Medizinische Einrichtung'
      });
    }
    
    if (text.includes('Doktor')) {
      items.push({
        text: 'Doktor',
        confidence: 65,
        type: 'Titel'
      });
    }
    
    return {
      text: processed,
      reviewItems: items
    };
  }
  
  // Verarbeitet den Transkripttext, wenn er sich ändert
  $: {
    if (transcript) {
      const result = processAnonymization(transcript);
      anonymizedText = result.text;
      reviewItems = result.reviewItems;
    } else {
      anonymizedText = '';
      reviewItems = [];
    }
  }
  
  // [MFD] Medizinische Fachbegriffe DE-CH - Hervorhebung
  function highlightMedicalTerms(text: string): string {
    if (!text) return '';
    
    // Schweizer medizinische Fachbegriffe
    const terms = [
      'Spital', 'Doktor', 'Krankenkasse', 'Konsultation', 'Medikament',
      'Krankengeschichte', 'Befund', 'Diagnose', 'Therapie', 'Überweisung'
    ];
    
    let result = text;
    terms.forEach(term => {
      const regex = new RegExp(`\\b${term}\\b`, 'g');
      result = result.replace(regex, `<span class="medical-term">${term}</span>`);
    });
    
    return result;
  }
</script>

<div class="transcript-viewer">
  <div class="viewer-header">
    <h3>Transkript</h3>
    {#if sessionId}
      <span class="session-id">Session: {sessionId}</span>
    {/if}
  </div>
  
  <div class="transcript-content">
    {#if loading}
      <div class="loading-state">
        <div class="spinner"></div>
        <p>Transkript wird verarbeitet...</p>
      </div>
    {:else if anonymizedText}
      <div class="transcript-text">
        <!-- [AIU] Anonymisierung ist UNVERÄNDERLICH - Immer aktiv -->
        <div class="anonymization-badge">
          <span class="icon">🔒</span>
          <span>Anonymisiert</span>
        </div>
        
        <!-- [MFD] Medizinische Fachbegriffe DE-CH - Hervorhebung -->
        <div class="text-content">
          {@html highlightMedicalTerms(anonymizedText)}
        </div>
      </div>
      
      <!-- [ARQ] Anonymisierungs-Review-Queue -->
      {#if reviewItems.length > 0}
        <div class="review-section">
          <h4>Prüfung erforderlich ({reviewItems.length})</h4>
          <p class="review-info">Die folgenden Begriffe wurden mit niedriger Konfidenz erkannt:</p>
          
          <ul class="review-items">
            {#each reviewItems as item}
              <li class="review-item">
                <div class="item-header">
                  <span class="item-text">{item.text}</span>
                  <span class="item-confidence" class:low={item.confidence < 70}>
                    {item.confidence}%
                  </span>
                </div>
                <div class="item-type">{item.type}</div>
                <div class="item-actions">
                  <button class="action-button approve">Beibehalten</button>
                  <button class="action-button anonymize">Anonymisieren</button>
                </div>
              </li>
            {/each}
          </ul>
        </div>
      {/if}
    {:else}
      <div class="empty-state">
        <p>Kein Transkript verfügbar</p>
      </div>
    {/if}
  </div>
</div>

<style>
  .transcript-viewer {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }
  
  .viewer-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  
  .viewer-header h3 {
    margin: 0;
    font-size: 1.125rem;
    font-weight: 600;
    color: #1e293b;
  }
  
  .session-id {
    font-size: 0.875rem;
    color: #64748b;
    font-family: monospace;
  }
  
  .transcript-content {
    border: 1px solid #e5e7eb;
    border-radius: 6px;
    background-color: #f9fafb;
  }
  
  .loading-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 2rem;
    gap: 1rem;
  }
  
  .spinner {
    width: 32px;
    height: 32px;
    border: 3px solid #e5e7eb;
    border-top-color: #2563eb;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }
  
  .loading-state p {
    margin: 0;
    color: #64748b;
  }
  
  .transcript-text {
    padding: 1rem;
    position: relative;
  }
  
  .anonymization-badge {
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
    display: flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.25rem 0.5rem;
    background-color: #f0f9ff;
    border: 1px solid #bae6fd;
    border-radius: 4px;
    font-size: 0.75rem;
    color: #0369a1;
  }
  
  .text-content {
    font-size: 0.9375rem;
    line-height: 1.6;
    color: #1e293b;
    white-space: pre-wrap;
  }
  
  /* [MFD] Medizinische Fachbegriffe DE-CH */
  :global(.medical-term) {
    background-color: #e0f2fe;
    border-radius: 3px;
    padding: 0 0.25rem;
    font-weight: 500;
    color: #0369a1;
  }
  
  .empty-state {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 2rem;
    color: #94a3b8;
    font-style: italic;
  }
  
  /* [ARQ] Anonymisierungs-Review-Queue */
  .review-section {
    padding: 1rem;
    border-top: 1px solid #e5e7eb;
  }
  
  .review-section h4 {
    margin: 0 0 0.5rem;
    font-size: 1rem;
    color: #dc2626;
  }
  
  .review-info {
    margin: 0 0 1rem;
    font-size: 0.875rem;
    color: #64748b;
  }
  
  .review-items {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .review-item {
    background-color: white;
    border: 1px solid #e5e7eb;
    border-radius: 6px;
    padding: 0.75rem;
  }
  
  .item-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.25rem;
  }
  
  .item-text {
    font-weight: 500;
    color: #1e293b;
  }
  
  .item-confidence {
    font-size: 0.875rem;
    color: #059669;
    font-weight: 500;
  }
  
  .item-confidence.low {
    color: #dc2626;
  }
  
  .item-type {
    font-size: 0.8125rem;
    color: #64748b;
    margin-bottom: 0.75rem;
  }
  
  .item-actions {
    display: flex;
    gap: 0.5rem;
  }
  
  .action-button {
    flex: 1;
    padding: 0.375rem 0.5rem;
    font-size: 0.8125rem;
    border-radius: 4px;
    border: none;
    cursor: pointer;
    font-weight: 500;
  }
  
  .action-button.approve {
    background-color: #f0f9ff;
    color: #0369a1;
    border: 1px solid #bae6fd;
  }
  
  .action-button.anonymize {
    background-color: #fef2f2;
    color: #dc2626;
    border: 1px solid #fecaca;
  }
  
  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
