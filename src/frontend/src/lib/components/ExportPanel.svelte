<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [SF] Schweizer Formate - Deutsche Sprache, DD.MM.YYYY
  [AIU] Anonymisierung ist unveränderlich - immer aktiv
  [MDL] Medical Domain Language - Medizinische Fachsprache
  [UX] User Experience - Export-Funktionen für Transkripte
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  const dispatch = createEventDispatcher();
  
  export let transcriptEntries: Array<{
    id: number;
    timestamp: string;
    speaker: string;
    text: string;
    confidence: number;
    isEditable: boolean;
    isAnonymized: boolean;
  }> = [];
  
  export let sessionDate: string = '';
  export let sessionDuration: string = '';
  
  // Export-Optionen [SF]
  let exportFormat = 'pdf';
  let includeTimestamps = true;
  let includeSpeakers = true;
  let includeConfidence = false;
  let includeMetadata = true;
  let anonymizeExport = true; // Immer aktiviert [AIU]
  
  // PDF-spezifische Optionen [SF]
  let pdfTemplate = 'medical';
  let includeHeader = true;
  let includeFooter = true;
  let pageNumbers = true;
  
  // Statistiken für Export [MDL]
  $: exportStats = {
    totalEntries: transcriptEntries.length,
    totalWords: transcriptEntries.reduce((sum, entry) => 
      sum + entry.text.split(/\s+/).filter(word => word.length > 0).length, 0),
    averageConfidence: transcriptEntries.length > 0 
      ? Math.round((transcriptEntries.reduce((sum, entry) => sum + entry.confidence, 0) / transcriptEntries.length) * 100)
      : 0,
    speakers: [...new Set(transcriptEntries.map(entry => entry.speaker))],
    estimatedPages: Math.ceil(transcriptEntries.length / 20) // Geschätzt 20 Einträge pro Seite
  };
  
  function handleExport() {
    const exportData = {
      format: exportFormat,
      entries: transcriptEntries,
      options: {
        includeTimestamps,
        includeSpeakers,
        includeConfidence,
        includeMetadata,
        anonymizeExport,
        pdfTemplate,
        includeHeader,
        includeFooter,
        pageNumbers
      },
      metadata: {
        sessionDate,
        sessionDuration,
        exportDate: new Date().toLocaleDateString('de-CH'),
        exportTime: new Date().toLocaleTimeString('de-CH'),
        stats: exportStats
      }
    };
    
    dispatch('export', exportData);
  }
  
  function previewExport() {
    // Vorschau-Funktionalität
    dispatch('preview', { format: exportFormat, options: { includeTimestamps, includeSpeakers } });
  }
  
  function getFormatIcon(format: string): string {
    switch (format) {
      case 'pdf': return '📄';
      case 'docx': return '📝';
      case 'txt': return '📋';
      case 'json': return '🔧';
      default: return '📁';
    }
  }
  
  function getEstimatedFileSize(): string {
    const baseSize = transcriptEntries.reduce((sum, entry) => sum + entry.text.length, 0);
    const multiplier = exportFormat === 'pdf' ? 3 : exportFormat === 'docx' ? 2 : 1;
    const sizeKB = Math.ceil((baseSize * multiplier) / 1024);
    
    if (sizeKB > 1024) {
      return `~${(sizeKB / 1024).toFixed(1)} MB`;
    }
    return `~${sizeKB} KB`;
  }
</script>

<div class="export-panel">
  <!-- Export-Format Auswahl [SF] -->
  <div class="export-section">
    <h4 class="section-title">📁 Export-Format</h4>
    <div class="format-options">
      <label class="format-option">
        <input type="radio" bind:group={exportFormat} value="pdf" />
        <div class="format-card">
          <span class="format-icon">📄</span>
          <span class="format-name">PDF</span>
          <span class="format-desc">Professioneller Bericht</span>
        </div>
      </label>
      <label class="format-option">
        <input type="radio" bind:group={exportFormat} value="docx" />
        <div class="format-card">
          <span class="format-icon">📝</span>
          <span class="format-name">Word</span>
          <span class="format-desc">Editierbar</span>
        </div>
      </label>
      <label class="format-option">
        <input type="radio" bind:group={exportFormat} value="txt" />
        <div class="format-card">
          <span class="format-icon">📋</span>
          <span class="format-name">Text</span>
          <span class="format-desc">Einfach</span>
        </div>
      </label>
      <label class="format-option">
        <input type="radio" bind:group={exportFormat} value="json" />
        <div class="format-card">
          <span class="format-icon">🔧</span>
          <span class="format-name">JSON</span>
          <span class="format-desc">Technisch</span>
        </div>
      </label>
    </div>
  </div>
  
  <!-- Export-Optionen [UX] -->
  <div class="export-section">
    <h4 class="section-title">⚙️ Export-Optionen</h4>
    <div class="options-list">
      <label class="option-item">
        <input type="checkbox" bind:checked={includeTimestamps} />
        <span class="option-label">Zeitstempel einschließen</span>
        <span class="option-desc">Zeigt wann jeder Eintrag aufgenommen wurde</span>
      </label>
      
      <label class="option-item">
        <input type="checkbox" bind:checked={includeSpeakers} />
        <span class="option-label">Speaker-Kennzeichnung</span>
        <span class="option-desc">Zeigt wer gesprochen hat (Arzt/Patient)</span>
      </label>
      
      <label class="option-item">
        <input type="checkbox" bind:checked={includeConfidence} />
        <span class="option-label">Konfidenz-Werte anzeigen</span>
        <span class="option-desc">Technische Genauigkeits-Information</span>
      </label>
      
      <label class="option-item">
        <input type="checkbox" bind:checked={includeMetadata} />
        <span class="option-label">Session-Metadaten</span>
        <span class="option-desc">Datum, Dauer und Statistiken</span>
      </label>
      
      <!-- Anonymisierung - Immer aktiviert [AIU] -->
      <div class="option-item disabled">
        <input type="checkbox" checked disabled />
        <span class="option-label">Anonymisierung</span>
        <span class="option-desc">🔒 Patientendaten werden automatisch anonymisiert</span>
      </div>
    </div>
  </div>
  
  <!-- PDF-spezifische Optionen [SF] -->
  {#if exportFormat === 'pdf'}
    <div class="export-section">
      <h4 class="section-title">📄 PDF-Einstellungen</h4>
      <div class="pdf-options">
        <div class="option-group">
          <label class="option-label">Vorlage:</label>
          <select bind:value={pdfTemplate} class="template-select">
            <option value="medical">Medizinischer Bericht</option>
            <option value="simple">Einfaches Layout</option>
            <option value="detailed">Detailliert mit Analyse</option>
          </select>
        </div>
        
        <div class="checkbox-group">
          <label class="option-item">
            <input type="checkbox" bind:checked={includeHeader} />
            <span class="option-label">Kopfzeile mit Logo</span>
          </label>
          <label class="option-item">
            <input type="checkbox" bind:checked={includeFooter} />
            <span class="option-label">Fußzeile mit Datum</span>
          </label>
          <label class="option-item">
            <input type="checkbox" bind:checked={pageNumbers} />
            <span class="option-label">Seitenzahlen</span>
          </label>
        </div>
      </div>
    </div>
  {/if}
  
  <!-- Export-Vorschau [UX] -->
  <div class="export-section">
    <h4 class="section-title">📊 Export-Vorschau</h4>
    <div class="preview-stats">
      <div class="stat-row">
        <span class="stat-label">Format:</span>
        <span class="stat-value">{getFormatIcon(exportFormat)} {exportFormat.toUpperCase()}</span>
      </div>
      <div class="stat-row">
        <span class="stat-label">Einträge:</span>
        <span class="stat-value">{exportStats.totalEntries}</span>
      </div>
      <div class="stat-row">
        <span class="stat-label">Wörter:</span>
        <span class="stat-value">{exportStats.totalWords}</span>
      </div>
      <div class="stat-row">
        <span class="stat-label">Geschätzte Größe:</span>
        <span class="stat-value">{getEstimatedFileSize()}</span>
      </div>
      {#if exportFormat === 'pdf'}
        <div class="stat-row">
          <span class="stat-label">Geschätzte Seiten:</span>
          <span class="stat-value">{exportStats.estimatedPages}</span>
        </div>
      {/if}
      <div class="stat-row">
        <span class="stat-label">Ø Konfidenz:</span>
        <span class="stat-value">{exportStats.averageConfidence}%</span>
      </div>
    </div>
  </div>
  
  <!-- Datenschutz-Hinweis [AIU] -->
  <div class="export-section privacy-notice">
    <h4 class="section-title">🔒 Datenschutz-Hinweis</h4>
    <div class="privacy-content">
      <div class="privacy-item">
        <span class="privacy-icon">✅</span>
        <span class="privacy-text">Alle Patientendaten werden automatisch anonymisiert</span>
      </div>
      <div class="privacy-item">
        <span class="privacy-icon">🇨🇭</span>
        <span class="privacy-text">Export entspricht Schweizer Datenschutzgesetzen (nDSG)</span>
      </div>
      <div class="privacy-item">
        <span class="privacy-icon">🔐</span>
        <span class="privacy-text">Keine personenbezogenen Daten im Export enthalten</span>
      </div>
    </div>
  </div>
  
  <!-- Export-Aktionen [UX] -->
  <div class="export-actions">
    <button 
      class="action-btn preview"
      on:click={previewExport}
      disabled={transcriptEntries.length === 0}
    >
      👁️ Vorschau
    </button>
    <button 
      class="action-btn export"
      on:click={handleExport}
      disabled={transcriptEntries.length === 0}
    >
      {getFormatIcon(exportFormat)} Export starten
    </button>
  </div>
</div>

<style>
  .export-panel {
    height: 100%;
    overflow-y: auto;
    padding-right: 4px;
  }
  
  .export-section {
    margin-bottom: 24px;
    padding: 16px;
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 8px;
  }
  
  .export-section.privacy-notice {
    background: var(--success-light, #f0fdf4);
    border-color: var(--success, #10b981);
  }
  
  .section-title {
    margin: 0 0 16px 0;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary, #1e293b);
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .format-options {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 8px;
  }
  
  .format-option {
    cursor: pointer;
  }
  
  .format-option input[type="radio"] {
    display: none;
  }
  
  .format-card {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 12px 8px;
    border: 2px solid var(--border-light, #e2e8f0);
    border-radius: 8px;
    transition: all 0.2s ease;
    text-align: center;
  }
  
  .format-option input[type="radio"]:checked + .format-card {
    border-color: var(--primary, #3b82f6);
    background: var(--primary-light, #dbeafe);
  }
  
  .format-card:hover {
    border-color: var(--border-medium, #cbd5e1);
    background: var(--bg-hover, #f1f5f9);
  }
  
  .format-icon {
    font-size: 20px;
    margin-bottom: 4px;
  }
  
  .format-name {
    font-size: 12px;
    font-weight: 600;
    color: var(--text-primary, #1e293b);
    margin-bottom: 2px;
  }
  
  .format-desc {
    font-size: 10px;
    color: var(--text-secondary, #64748b);
  }
  
  .options-list {
    space-y: 12px;
  }
  
  .option-item {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    margin-bottom: 12px;
    cursor: pointer;
  }
  
  .option-item.disabled {
    opacity: 0.7;
    cursor: not-allowed;
  }
  
  .option-item input[type="checkbox"] {
    margin-top: 2px;
  }
  
  .option-item input[type="checkbox"]:disabled {
    cursor: not-allowed;
  }
  
  .option-label {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-primary, #1e293b);
    display: block;
    margin-bottom: 2px;
  }
  
  .option-desc {
    font-size: 11px;
    color: var(--text-secondary, #64748b);
    display: block;
  }
  
  .pdf-options {
    space-y: 16px;
  }
  
  .option-group {
    margin-bottom: 16px;
  }
  
  .template-select {
    width: 100%;
    padding: 6px 8px;
    border: 1px solid var(--border-medium, #cbd5e1);
    border-radius: 4px;
    font-size: 12px;
    margin-top: 4px;
  }
  
  .checkbox-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  
  .preview-stats {
    space-y: 8px;
  }
  
  .stat-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 6px 0;
    border-bottom: 1px solid var(--border-light, #e2e8f0);
    margin-bottom: 8px;
  }
  
  .stat-row:last-child {
    border-bottom: none;
    margin-bottom: 0;
  }
  
  .stat-label {
    font-size: 12px;
    color: var(--text-secondary, #64748b);
  }
  
  .stat-value {
    font-size: 12px;
    font-weight: 500;
    color: var(--text-primary, #1e293b);
  }
  
  .privacy-content {
    space-y: 8px;
  }
  
  .privacy-item {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 8px;
  }
  
  .privacy-icon {
    font-size: 14px;
  }
  
  .privacy-text {
    font-size: 12px;
    color: var(--success-dark, #059669);
  }
  
  .export-actions {
    display: flex;
    gap: 8px;
    padding: 16px;
    background: var(--bg-secondary, #f8fafc);
    border-radius: 8px;
    border: 1px solid var(--border-light, #e2e8f0);
  }
  
  .action-btn {
    flex: 1;
    padding: 10px 16px;
    border: 1px solid;
    border-radius: 6px;
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
  }
  
  .action-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .action-btn.preview {
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1e293b);
    border-color: var(--border-medium, #cbd5e1);
  }
  
  .action-btn.preview:hover:not(:disabled) {
    background: var(--bg-hover, #f1f5f9);
    border-color: var(--border-dark, #94a3b8);
  }
  
  .action-btn.export {
    background: var(--primary, #3b82f6);
    color: white;
    border-color: var(--primary, #3b82f6);
  }
  
  .action-btn.export:hover:not(:disabled) {
    background: var(--primary-dark, #2563eb);
    border-color: var(--primary-dark, #2563eb);
  }
  
  /* Mobile Responsive [UX] */
  @media (max-width: 768px) {
    .format-options {
      grid-template-columns: 1fr;
    }
    
    .export-actions {
      flex-direction: column;
    }
    
    .checkbox-group {
      gap: 12px;
    }
  }
</style>
