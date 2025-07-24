<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [AIU] Anonymisierung ist unveränderlich - immer aktiv
  [MDL] Medical Domain Language - Medizinische Fachsprache
  [SF] Schweizer Formate - Deutsche Sprache
  [UX] User Experience - Konfidenz-Review für niedrige Genauigkeit
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  const dispatch = createEventDispatcher();
  
  export let entries: Array<{
    id: number;
    timestamp: string;
    speaker: string;
    text: string;
    confidence: number;
    isEditable: boolean;
    isAnonymized: boolean;
  }> = [];
  
  // Medizinische Begriffe [MDL]
  const medicalTerms = [
    'kopfschmerz', 'bauchschmerz', 'rückenschmerz', 'brustschmerz',
    'fieber', 'husten', 'schnupfen', 'übelkeit', 'schwindel',
    'blutdruck', 'herzschlag', 'puls', 'atmung', 'allergie',
    'diabetes', 'asthma', 'migräne', 'grippe', 'erkältung'
  ];
  
  // Patienten-Personendaten Begriffe [AIU]
  const personalDataTerms = [
    'name', 'vorname', 'nachname', 'adresse', 'straße',
    'telefon', 'handy', 'email', 'geburtsdatum', 'alter'
  ];
  
  // Kategorisierung der Einträge [AIU]
  $: categorizedEntries = {
    medical: entries.filter(entry => {
      const text = entry.text.toLowerCase();
      return medicalTerms.some(term => text.includes(term)) && entry.confidence < 0.9;
    }),
    personal: entries.filter(entry => {
      const text = entry.text.toLowerCase();
      return personalDataTerms.some(term => text.includes(term)) && entry.confidence < 0.9;
    }),
    other: entries.filter(entry => {
      const text = entry.text.toLowerCase();
      const isMedical = medicalTerms.some(term => text.includes(term));
      const isPersonal = personalDataTerms.some(term => text.includes(term));
      return !isMedical && !isPersonal && entry.confidence < 0.9;
    })
  };
  
  // Aktiver Tab für Kategorien
  let activeCategory: 'medical' | 'personal' | 'other' = 'medical';
  
  // Statistiken [AIU]
  $: totalLowConfidence = entries.filter(e => e.confidence < 0.9).length;
  $: averageConfidence = entries.length > 0 
    ? entries.reduce((sum, entry) => sum + entry.confidence, 0) / entries.length 
    : 0;
  $: criticalEntries = entries.filter(entry => entry.confidence < 0.8);
  
  function approveEntry(entryId: number) {
    dispatch('approve', { id: entryId });
  }
  
  function rejectEntry(entryId: number) {
    dispatch('reject', { id: entryId });
  }
  
  function editEntry(entryId: number, newText: string) {
    dispatch('edit', { id: entryId, text: newText });
  }
  
  function getConfidenceLabel(confidence: number): string {
    if (confidence >= 0.9) return 'Hoch';
    if (confidence >= 0.8) return 'Mittel';
    if (confidence >= 0.7) return 'Niedrig';
    return 'Kritisch';
  }
  
  function getConfidenceColor(confidence: number): string {
    if (confidence >= 0.9) return 'var(--success, #10b981)';
    if (confidence >= 0.8) return 'var(--warning, #f59e0b)';
    if (confidence >= 0.7) return 'var(--danger, #ef4444)';
    return 'var(--danger-dark, #dc2626)';
  }
</script>

<div class="confidence-review-panel">
  <!-- Kategorie-Tabs [AIU] -->
  <div class="category-tabs">
    <button 
      class="category-tab"
      class:active={activeCategory === 'medical'}
      on:click={() => activeCategory = 'medical'}
    >
      🏥 Medizinische Begriffe ({categorizedEntries.medical.length})
    </button>
    <button 
      class="category-tab"
      class:active={activeCategory === 'personal'}
      on:click={() => activeCategory = 'personal'}
    >
      👤 Personendaten ({categorizedEntries.personal.length})
    </button>
    <button 
      class="category-tab"
      class:active={activeCategory === 'other'}
      on:click={() => activeCategory = 'other'}
    >
      ❓ Sonstige ({categorizedEntries.other.length})
    </button>
  </div>
  
  <!-- Statistiken-Header [AIU] -->
  <div class="stats-header">
    <div class="stat-item">
      <div class="stat-value">{totalLowConfidence}</div>
      <div class="stat-label">Gesamt zu prüfen</div>
    </div>
    <div class="stat-item">
      <div class="stat-value">{categorizedEntries[activeCategory].length}</div>
      <div class="stat-label">Aktuelle Kategorie</div>
    </div>
    <div class="stat-item critical">
      <div class="stat-value">{criticalEntries.length}</div>
      <div class="stat-label">Kritisch (&lt;80%)</div>
    </div>
  </div>
  
  {#if categorizedEntries[activeCategory].length === 0}
    <!-- Leer-Zustand [UX] -->
    <div class="empty-state">
      <div class="empty-icon">✅</div>
      <h4>Keine Einträge in dieser Kategorie</h4>
      <p>
        {#if activeCategory === 'medical'}
          Keine medizinischen Begriffe mit niedriger Konfidenz gefunden.
        {:else if activeCategory === 'personal'}
          Keine Personendaten mit niedriger Konfidenz gefunden.
        {:else}
          Keine sonstigen Begriffe mit niedriger Konfidenz gefunden.
        {/if}
      </p>
    </div>
  {:else}
    <!-- Konfidenz-Review-Liste [AIU] -->
    <div class="review-list">
      {#each categorizedEntries[activeCategory].sort((a, b) => a.confidence - b.confidence) as entry (entry.id)}
        <div class="review-entry" class:critical={entry.confidence < 0.8}>
          <!-- Entry-Header -->
          <div class="entry-header">
            <div class="entry-meta">
              <span class="timestamp">{entry.timestamp}</span>
              <span class="speaker">{entry.speaker}</span>
            </div>
            <div 
              class="confidence-badge"
              style="background-color: {getConfidenceColor(entry.confidence)}20; color: {getConfidenceColor(entry.confidence)}"
            >
              {Math.round(entry.confidence * 100)}% • {getConfidenceLabel(entry.confidence)}
            </div>
          </div>
          
          <!-- Original-Text -->
          <div class="original-text">
            <label class="text-label">Original-Transkription:</label>
            <div class="text-content">{entry.text}</div>
          </div>
          
          <!-- Review-Aktionen [AIU] -->
          <div class="review-actions">
            <button 
              class="action-btn approve"
              on:click={() => approveEntry(entry.id)}
              title="Transkription als korrekt bestätigen"
            >
              ✓ Bestätigen
            </button>
            <button 
              class="action-btn edit"
              on:click={() => {
                const newText = prompt('Transkription bearbeiten:', entry.text);
                if (newText && newText !== entry.text) {
                  editEntry(entry.id, newText);
                }
              }}
              title="Transkription bearbeiten"
            >
              ✏️ Bearbeiten
            </button>
            <button 
              class="action-btn reject"
              on:click={() => rejectEntry(entry.id)}
              title="Transkription als fehlerhaft markieren"
            >
              ✕ Ablehnen
            </button>
          </div>
          
          <!-- Anonymisierungs-Status [AIU] -->
          {#if entry.isAnonymized}
            <div class="anonymization-status">
              🔒 Patientendaten wurden automatisch anonymisiert
            </div>
          {/if}
        </div>
      {/each}
    </div>
    
    <!-- Bulk-Aktionen [UX] -->
    <div class="bulk-actions">
      <button 
        class="bulk-btn approve-all"
        on:click={() => {
          entries.forEach(entry => approveEntry(entry.id));
        }}
        disabled={entries.length === 0}
      >
        Alle bestätigen
      </button>
      <button 
        class="bulk-btn review-critical"
        on:click={() => {
          // Scroll zu kritischen Einträgen
          const criticalElement = document.querySelector('.review-entry.critical');
          if (criticalElement) {
            criticalElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
          }
        }}
        disabled={criticalEntries.length === 0}
      >
        Kritische prüfen ({criticalEntries.length})
      </button>
    </div>
  {/if}
</div>

<style>
  .confidence-review-panel {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
  
  .category-tabs {
    display: flex;
    gap: 4px;
    margin-bottom: 16px;
    border-bottom: 1px solid var(--border-light, #e2e8f0);
    padding-bottom: 8px;
  }
  
  .category-tab {
    flex: 1;
    padding: 8px 12px;
    background: var(--bg-secondary, #f8fafc);
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 6px;
    font-size: 11px;
    font-weight: 500;
    color: var(--text-secondary, #64748b);
    cursor: pointer;
    transition: all 0.2s ease;
    text-align: center;
  }
  
  .category-tab:hover {
    background: var(--bg-hover, #f1f5f9);
    border-color: var(--border-medium, #cbd5e1);
  }
  
  .category-tab.active {
    background: var(--primary, #3b82f6);
    color: white;
    border-color: var(--primary, #3b82f6);
  }
  
  .stats-header {
    display: flex;
    gap: 16px;
    padding: 16px 0;
    border-bottom: 1px solid var(--border-light, #e2e8f0);
    margin-bottom: 16px;
  }
  
  .stat-item {
    text-align: center;
    flex: 1;
  }
  
  .stat-item.critical .stat-value {
    color: var(--danger, #ef4444);
  }
  
  .stat-value {
    font-size: 20px;
    font-weight: 700;
    color: var(--text-primary, #1e293b);
    margin-bottom: 4px;
  }
  
  .stat-label {
    font-size: 12px;
    color: var(--text-secondary, #64748b);
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .empty-state {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    color: var(--text-secondary, #64748b);
  }
  
  .empty-icon {
    font-size: 48px;
    margin-bottom: 16px;
  }
  
  .empty-state h4 {
    margin: 0 0 8px 0;
    color: var(--text-primary, #1e293b);
  }
  
  .empty-state p {
    margin: 0;
    font-size: 14px;
  }
  
  .review-list {
    flex: 1;
    overflow-y: auto;
    padding-right: 4px;
  }
  
  .review-entry {
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 12px;
    transition: all 0.2s ease;
  }
  
  .review-entry:hover {
    border-color: var(--border-medium, #cbd5e1);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  }
  
  .review-entry.critical {
    border-color: var(--danger, #ef4444);
    background: var(--danger-light, #fef2f2);
  }
  
  .entry-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 12px;
  }
  
  .entry-meta {
    display: flex;
    gap: 8px;
    align-items: center;
  }
  
  .timestamp {
    font-family: 'SF Mono', Monaco, 'Cascadia Code', monospace;
    font-size: 11px;
    color: var(--text-secondary, #64748b);
    background: var(--bg-secondary, #f8fafc);
    padding: 2px 6px;
    border-radius: 4px;
  }
  
  .speaker {
    font-size: 12px;
    font-weight: 500;
    color: var(--text-primary, #1e293b);
  }
  
  .confidence-badge {
    font-size: 11px;
    font-weight: 600;
    padding: 4px 8px;
    border-radius: 6px;
    border: 1px solid currentColor;
  }
  
  .original-text {
    margin-bottom: 16px;
  }
  
  .text-label {
    display: block;
    font-size: 12px;
    font-weight: 500;
    color: var(--text-secondary, #64748b);
    margin-bottom: 6px;
  }
  
  .text-content {
    font-size: 14px;
    line-height: 1.5;
    color: var(--text-primary, #1e293b);
    background: var(--bg-secondary, #f8fafc);
    padding: 12px;
    border-radius: 6px;
    border: 1px solid var(--border-light, #e2e8f0);
  }
  
  .review-actions {
    display: flex;
    gap: 8px;
    margin-bottom: 12px;
  }
  
  .action-btn {
    padding: 6px 12px;
    border: 1px solid;
    border-radius: 6px;
    font-size: 12px;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    gap: 4px;
  }
  
  .action-btn.approve {
    background: var(--success-light, #f0fdf4);
    color: var(--success-dark, #059669);
    border-color: var(--success, #10b981);
  }
  
  .action-btn.approve:hover {
    background: var(--success, #10b981);
    color: white;
  }
  
  .action-btn.edit {
    background: var(--warning-light, #fffbeb);
    color: var(--warning-dark, #d97706);
    border-color: var(--warning, #f59e0b);
  }
  
  .action-btn.edit:hover {
    background: var(--warning, #f59e0b);
    color: white;
  }
  
  .action-btn.reject {
    background: var(--danger-light, #fef2f2);
    color: var(--danger-dark, #dc2626);
    border-color: var(--danger, #ef4444);
  }
  
  .action-btn.reject:hover {
    background: var(--danger, #ef4444);
    color: white;
  }
  
  .anonymization-status {
    font-size: 11px;
    color: var(--success-dark, #059669);
    background: var(--success-light, #f0fdf4);
    padding: 6px 10px;
    border-radius: 4px;
    border: 1px solid var(--success, #10b981);
    display: flex;
    align-items: center;
    gap: 6px;
  }
  
  .bulk-actions {
    display: flex;
    gap: 8px;
    padding-top: 16px;
    border-top: 1px solid var(--border-light, #e2e8f0);
    margin-top: auto;
  }
  
  .bulk-btn {
    flex: 1;
    padding: 8px 16px;
    border: 1px solid var(--border-medium, #cbd5e1);
    border-radius: 6px;
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1e293b);
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s ease;
  }
  
  .bulk-btn:hover:not(:disabled) {
    background: var(--bg-hover, #f1f5f9);
    border-color: var(--border-dark, #94a3b8);
  }
  
  .bulk-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .bulk-btn.approve-all:hover:not(:disabled) {
    background: var(--success, #10b981);
    color: white;
    border-color: var(--success, #10b981);
  }
  
  .bulk-btn.review-critical:hover:not(:disabled) {
    background: var(--danger, #ef4444);
    color: white;
    border-color: var(--danger, #ef4444);
  }
  
  /* Mobile Responsive [UX] */
  @media (max-width: 768px) {
    .stats-header {
      flex-direction: column;
      gap: 8px;
    }
    
    .stat-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      text-align: left;
    }
    
    .review-actions {
      flex-direction: column;
    }
    
    .bulk-actions {
      flex-direction: column;
    }
  }
</style>
