<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [MDL] Medical Domain Language - Medizinische Fachsprache
  [AIU] Anonymisierung ist unveränderlich - immer aktiv
  [SF] Schweizer Formate - Deutsche Sprache, DD.MM.YYYY
  [UX] User Experience - Editierbare Transkript-Einträge
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  const dispatch = createEventDispatcher();
  
  export let entry: {
    id: number;
    timestamp: string;
    speaker: string;
    text: string;
    confidence: number;
    isEditable: boolean;
    isAnonymized: boolean;
  };
  
  let isEditing = false;
  let editText = entry.text;
  let textArea: HTMLTextAreaElement;
  
  // Konfidenz-Level für Styling [AIU]
  $: confidenceLevel = entry.confidence >= 0.95 ? 'high' : 
                      entry.confidence >= 0.85 ? 'medium' : 'low';
  
  function startEdit() {
    if (!entry.isEditable) return;
    isEditing = true;
    editText = entry.text;
    
    // Focus und Select nach DOM-Update
    setTimeout(() => {
      if (textArea) {
        textArea.focus();
        textArea.select();
      }
    }, 10);
  }
  
  function saveEdit() {
    if (editText.trim() !== entry.text) {
      dispatch('edit', { 
        id: entry.id, 
        text: editText.trim() 
      });
    }
    isEditing = false;
  }
  
  function cancelEdit() {
    editText = entry.text;
    isEditing = false;
  }
  
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      saveEdit();
    } else if (event.key === 'Escape') {
      event.preventDefault();
      cancelEdit();
    }
  }
  
  // Speaker-Icon basierend auf Rolle [MDL]
  function getSpeakerIcon(speaker: string): string {
    switch (speaker.toLowerCase()) {
      case 'arzt':
      case 'doctor':
      case 'dr.':
        return '👨‍⚕️';
      case 'patient':
      case 'patientin':
        return '🧑‍🦱';
      case 'system':
        return '🤖';
      default:
        return '💬';
    }
  }
  
  // Konfidenz-Farbe [AIU]
  function getConfidenceColor(level: string): string {
    switch (level) {
      case 'high': return 'var(--success, #10b981)';
      case 'medium': return 'var(--warning, #f59e0b)';
      case 'low': return 'var(--danger, #ef4444)';
      default: return 'var(--text-secondary, #64748b)';
    }
  }
</script>

<div class="transcript-entry" class:low-confidence={confidenceLevel === 'low'}>
  <!-- Timestamp und Speaker [SF] -->
  <div class="entry-header">
    <div class="timestamp">{entry.timestamp}</div>
    <div class="speaker">
      <span class="speaker-icon">{getSpeakerIcon(entry.speaker)}</span>
      <span class="speaker-name">{entry.speaker}</span>
    </div>
    <div class="confidence-indicator" style="color: {getConfidenceColor(confidenceLevel)}">
      {Math.round(entry.confidence * 100)}%
    </div>
  </div>
  
  <!-- Transkript-Text [MDL] -->
  <div class="entry-content">
    {#if isEditing}
      <div class="edit-mode">
        <textarea
          bind:this={textArea}
          bind:value={editText}
          on:keydown={handleKeydown}
          class="edit-textarea"
          rows="3"
          placeholder="Transkript-Text bearbeiten..."
        ></textarea>
        <div class="edit-actions">
          <button class="save-btn" on:click={saveEdit}>
            ✓ Speichern
          </button>
          <button class="cancel-btn" on:click={cancelEdit}>
            ✕ Abbrechen
          </button>
        </div>
      </div>
    {:else}
      <div 
        class="text-content" 
        class:editable={entry.isEditable}
        on:click={startEdit}
        on:keydown={(e) => e.key === 'Enter' && startEdit()}
        role={entry.isEditable ? 'button' : 'text'}
        tabindex={entry.isEditable ? 0 : -1}
        title={entry.isEditable ? 'Klicken zum Bearbeiten' : ''}
      >
        {entry.text}
        {#if entry.isEditable}
          <span class="edit-hint">✏️</span>
        {/if}
      </div>
    {/if}
    
    <!-- Anonymisierungs-Indikator [AIU] -->
    {#if entry.isAnonymized}
      <div class="anonymization-badge">
        🔒 Anonymisiert
      </div>
    {/if}
  </div>
</div>

<style>
  .transcript-entry {
    margin-bottom: 16px;
    padding: 12px 16px;
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 8px;
    transition: all 0.2s ease;
  }
  
  .transcript-entry:hover {
    border-color: var(--border-medium, #cbd5e1);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  }
  
  .transcript-entry.low-confidence {
    border-color: var(--warning, #f59e0b);
    background: var(--warning-light, #fffbeb);
  }
  
  .entry-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 8px;
    font-size: 12px;
  }
  
  .timestamp {
    font-family: 'SF Mono', Monaco, 'Cascadia Code', monospace;
    color: var(--text-secondary, #64748b);
    background: var(--bg-secondary, #f8fafc);
    padding: 2px 6px;
    border-radius: 4px;
    min-width: 60px;
    text-align: center;
  }
  
  .speaker {
    display: flex;
    align-items: center;
    gap: 4px;
    font-weight: 500;
    color: var(--text-primary, #1e293b);
  }
  
  .speaker-icon {
    font-size: 14px;
  }
  
  .speaker-name {
    font-size: 12px;
  }
  
  .confidence-indicator {
    margin-left: auto;
    font-family: 'SF Mono', Monaco, 'Cascadia Code', monospace;
    font-size: 11px;
    font-weight: 600;
    padding: 2px 6px;
    border-radius: 4px;
    background: var(--bg-secondary, #f8fafc);
  }
  
  .entry-content {
    position: relative;
  }
  
  .text-content {
    line-height: 1.5;
    color: var(--text-primary, #1e293b);
    font-size: 14px;
    position: relative;
  }
  
  .text-content.editable {
    cursor: pointer;
    padding: 8px;
    border-radius: 6px;
    transition: background-color 0.2s ease;
  }
  
  .text-content.editable:hover {
    background: var(--bg-hover, #f1f5f9);
  }
  
  .text-content.editable:focus {
    outline: 2px solid var(--primary, #3b82f6);
    outline-offset: 2px;
    background: var(--bg-hover, #f1f5f9);
  }
  
  .edit-hint {
    opacity: 0;
    transition: opacity 0.2s ease;
    margin-left: 8px;
    font-size: 12px;
  }
  
  .text-content.editable:hover .edit-hint,
  .text-content.editable:focus .edit-hint {
    opacity: 0.6;
  }
  
  .edit-mode {
    margin-top: 4px;
  }
  
  .edit-textarea {
    width: 100%;
    padding: 8px 12px;
    border: 1px solid var(--border-medium, #cbd5e1);
    border-radius: 6px;
    font-size: 14px;
    line-height: 1.5;
    resize: vertical;
    min-height: 60px;
    font-family: inherit;
  }
  
  .edit-textarea:focus {
    outline: 2px solid var(--primary, #3b82f6);
    outline-offset: 2px;
    border-color: var(--primary, #3b82f6);
  }
  
  .edit-actions {
    display: flex;
    gap: 8px;
    margin-top: 8px;
  }
  
  .save-btn,
  .cancel-btn {
    padding: 4px 12px;
    border: 1px solid;
    border-radius: 4px;
    font-size: 12px;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  
  .save-btn {
    background: var(--success, #10b981);
    color: white;
    border-color: var(--success, #10b981);
  }
  
  .save-btn:hover {
    background: var(--success-dark, #059669);
    border-color: var(--success-dark, #059669);
  }
  
  .cancel-btn {
    background: var(--bg-primary, #ffffff);
    color: var(--text-secondary, #64748b);
    border-color: var(--border-medium, #cbd5e1);
  }
  
  .cancel-btn:hover {
    background: var(--bg-hover, #f1f5f9);
    border-color: var(--border-dark, #94a3b8);
  }
  
  .anonymization-badge {
    position: absolute;
    top: -4px;
    right: -4px;
    font-size: 10px;
    color: var(--success-dark, #059669);
    background: var(--success-light, #f0fdf4);
    padding: 2px 6px;
    border-radius: 4px;
    border: 1px solid var(--success, #10b981);
  }
  
  /* Accessibility [UX] */
  @media (prefers-reduced-motion: reduce) {
    .transcript-entry,
    .text-content,
    .edit-hint {
      transition: none;
    }
  }
  
  /* Mobile Responsive [UX] */
  @media (max-width: 768px) {
    .entry-header {
      flex-wrap: wrap;
      gap: 8px;
    }
    
    .confidence-indicator {
      margin-left: 0;
      order: -1;
      flex-basis: 100%;
    }
    
    .text-content {
      font-size: 16px; /* Bessere Lesbarkeit auf Mobile */
    }
  }
</style>
