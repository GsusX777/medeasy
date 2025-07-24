<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [TSF] Tauri 1.5 + Svelte 4 Stack
  [MDL] Medical Domain Language - Medizinische Fachsprache
  [AIU] Anonymisierung ist unveränderlich - immer aktiv
  [UX] User Experience - Split-View für Transkript und Analyse
  [SF] Schweizer Formate - Deutsche Sprache, DD.MM.YYYY
-->
<script lang="ts">
  import { onMount, afterUpdate } from 'svelte';
  import TranscriptEntry from './TranscriptEntry.svelte';
  import ConfidenceReviewPanel from './ConfidenceReviewPanel.svelte';
  import LiveAnalysisPanel from './LiveAnalysisPanel_Simple.svelte';
  import ExportPanel from './ExportPanel.svelte';
  
  // Mock-Daten für Demonstration [MDL]
  let transcriptEntries = [
    {
      id: 1,
      timestamp: '14:32:15',
      speaker: 'Patient',
      text: 'Guten Tag, Herr Doktor. Ich habe seit drei Tagen Kopfschmerzen.',
      confidence: 0.95,
      isEditable: true,
      isAnonymized: true
    },
    {
      id: 2,
      timestamp: '14:32:28',
      speaker: 'Arzt',
      text: 'Guten Tag. Können Sie mir die Kopfschmerzen genauer beschreiben?',
      confidence: 0.98,
      isEditable: true,
      isAnonymized: true
    },
    {
      id: 3,
      timestamp: '14:32:45',
      speaker: 'Patient',
      text: 'Es ist ein dumpfer Schmerz im Bereich der Stirn und Schläfen.',
      confidence: 0.87,
      isEditable: true,
      isAnonymized: true
    },
    {
      id: 4,
      timestamp: '14:33:02',
      speaker: 'Arzt',
      text: 'Haben Sie in letzter Zeit viel Stress gehabt oder wenig geschlafen?',
      confidence: 0.92,
      isEditable: true,
      isAnonymized: true
    }
  ];
  
  // Split-View State [UX]
  let splitRatio = 60; // 60% Transkript, 40% Analyse
  let isResizing = false;
  let transcriptContainer: HTMLElement;
  let analysisContainer: HTMLElement;
  
  // Live-Transkript State [MDL]
  let isRecording = false;
  let autoScroll = true;
  let currentSpeaker = 'Patient';
  
  // Analyse-Panel State [AIU]
  let activeAnalysisTab = 'confidence'; // confidence | analysis | export
  let lowConfidenceEntries = transcriptEntries.filter(entry => entry.confidence < 0.9);
  
  // Session-Info [SF]
  let sessionDate = new Date().toLocaleDateString('de-CH');
  let sessionDuration = '00:15:32';
  
  function handleSplitResize(event: MouseEvent) {
    if (!isResizing) return;
    
    const container = event.currentTarget as HTMLElement;
    const rect = container.getBoundingClientRect();
    const newRatio = ((event.clientX - rect.left) / rect.width) * 100;
    
    // Begrenze Split-Ratio zwischen 30% und 80% [UX]
    splitRatio = Math.max(30, Math.min(80, newRatio));
  }
  
  function startResize() {
    isResizing = true;
  }
  
  function stopResize() {
    isResizing = false;
  }
  
  function scrollToLatest() {
    if (autoScroll && transcriptContainer) {
      transcriptContainer.scrollTop = transcriptContainer.scrollHeight;
    }
  }
  
  function addTranscriptEntry(text: string, speaker: string = currentSpeaker) {
    const newEntry = {
      id: transcriptEntries.length + 1,
      timestamp: new Date().toLocaleTimeString('de-CH'),
      speaker,
      text,
      confidence: 0.85 + Math.random() * 0.15, // Simulierte Konfidenz
      isEditable: true,
      isAnonymized: true
    };
    
    transcriptEntries = [...transcriptEntries, newEntry];
    
    // Update low confidence entries [AIU]
    if (newEntry.confidence < 0.9) {
      lowConfidenceEntries = [...lowConfidenceEntries, newEntry];
    }
  }
  
  function switchAnalysisTab(tab: string) {
    activeAnalysisTab = tab;
  }
  
  function toggleAutoScroll() {
    autoScroll = !autoScroll;
  }
  
  function exportTranscript() {
    // Export-Funktionalität wird in ExportPanel implementiert
    console.log('Export-Funktion aufgerufen');
  }
  
  // Auto-Scroll nach Updates [UX]
  afterUpdate(() => {
    scrollToLatest();
  });
  
  onMount(() => {
    // Event-Listener für Split-Resize [UX]
    document.addEventListener('mousemove', handleSplitResize);
    document.addEventListener('mouseup', stopResize);
    
    return () => {
      document.removeEventListener('mousemove', handleSplitResize);
      document.removeEventListener('mouseup', stopResize);
    };
  });
</script>

<div class="transcript-split-view" class:resizing={isResizing}>
  <!-- Live-Transkript-Bereich (Links) [MDL] -->
  <div 
    class="transcript-panel" 
    style="width: {splitRatio}%"
    bind:this={transcriptContainer}
  >
    <div class="panel-header">
      <h3>Live-Transkription</h3>
      <div class="transcript-controls">
        <button 
          class="control-btn"
          class:active={autoScroll}
          on:click={toggleAutoScroll}
          title="Auto-Scroll aktivieren/deaktivieren"
        >
          📜
        </button>
        <div class="session-info">
          {sessionDate} • {sessionDuration}
        </div>
      </div>
    </div>
    
    <div class="transcript-content">
      {#each transcriptEntries as entry (entry.id)}
        <TranscriptEntry 
          {entry}
          on:edit={(e) => {
            const index = transcriptEntries.findIndex(t => t.id === entry.id);
            if (index !== -1) {
              transcriptEntries[index].text = e.detail.text;
              transcriptEntries = [...transcriptEntries];
            }
          }}
        />
      {/each}
      
      {#if isRecording}
        <div class="recording-indicator">
          <span class="pulse-dot"></span>
          Aufnahme läuft...
        </div>
      {/if}
    </div>
  </div>
  
  <!-- Split-Resize-Handle [UX] -->
  <div 
    class="split-handle"
    on:mousedown={startResize}
    role="separator"
    aria-label="Größe der Bereiche anpassen"
  >
    <div class="split-handle-line"></div>
  </div>
  
  <!-- Analyse-Bereich (Rechts) [AIU] -->
  <div 
    class="analysis-panel" 
    style="width: {100 - splitRatio}%"
    bind:this={analysisContainer}
  >
    <div class="panel-header">
      <h3>Analyse & Review</h3>
      <div class="analysis-tabs">
        <button 
          class="tab-btn"
          class:active={activeAnalysisTab === 'confidence'}
          on:click={() => switchAnalysisTab('confidence')}
        >
          Konfidenz ({lowConfidenceEntries.length})
        </button>
        <button 
          class="tab-btn"
          class:active={activeAnalysisTab === 'analysis'}
          on:click={() => switchAnalysisTab('analysis')}
        >
          Live-Analyse
        </button>
        <button 
          class="tab-btn"
          class:active={activeAnalysisTab === 'export'}
          on:click={() => switchAnalysisTab('export')}
        >
          Export
        </button>
      </div>
    </div>
    
    <div class="analysis-content">
      {#if activeAnalysisTab === 'confidence'}
        <ConfidenceReviewPanel entries={lowConfidenceEntries} />
      {:else if activeAnalysisTab === 'analysis'}
        <LiveAnalysisPanel {transcriptEntries} />
      {:else if activeAnalysisTab === 'export'}
        <ExportPanel {transcriptEntries} {sessionDate} {sessionDuration} />
      {/if}
    </div>
  </div>
</div>

<style>
  .transcript-split-view {
    display: flex;
    height: 100%;
    background: var(--bg-primary, #ffffff);
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .transcript-split-view.resizing {
    cursor: col-resize;
    user-select: none;
  }
  
  .transcript-panel,
  .analysis-panel {
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;
  }
  
  .panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px 20px;
    background: var(--bg-secondary, #f8fafc);
    border-bottom: 1px solid var(--border-light, #e2e8f0);
    flex-shrink: 0;
  }
  
  .panel-header h3 {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary, #1e293b);
  }
  
  .transcript-controls {
    display: flex;
    align-items: center;
    gap: 12px;
  }
  
  .control-btn {
    padding: 6px 8px;
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.2s ease;
  }
  
  .control-btn:hover {
    background: var(--bg-hover, #f1f5f9);
    border-color: var(--border-medium, #cbd5e1);
  }
  
  .control-btn.active {
    background: var(--primary, #3b82f6);
    color: white;
    border-color: var(--primary, #3b82f6);
  }
  
  .session-info {
    font-size: 12px;
    color: var(--text-secondary, #64748b);
    font-family: 'SF Mono', Monaco, 'Cascadia Code', monospace;
  }
  
  .transcript-content {
    flex: 1;
    overflow-y: auto;
    padding: 16px 20px;
    scroll-behavior: smooth;
  }
  
  .recording-indicator {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 16px;
    background: var(--success-light, #f0fdf4);
    border: 1px solid var(--success, #10b981);
    border-radius: 8px;
    margin-top: 12px;
    font-size: 14px;
    color: var(--success-dark, #059669);
  }
  
  .pulse-dot {
    width: 8px;
    height: 8px;
    background: var(--success, #10b981);
    border-radius: 50%;
    animation: pulse 1.5s ease-in-out infinite;
  }
  
  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.3; }
  }
  
  .split-handle {
    width: 4px;
    background: var(--border-light, #e2e8f0);
    cursor: col-resize;
    position: relative;
    flex-shrink: 0;
    transition: background-color 0.2s ease;
  }
  
  .split-handle:hover {
    background: var(--primary, #3b82f6);
  }
  
  .split-handle-line {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 2px;
    height: 40px;
    background: var(--border-medium, #cbd5e1);
    transform: translate(-50%, -50%);
    border-radius: 1px;
  }
  
  .split-handle:hover .split-handle-line {
    background: white;
  }
  
  .analysis-tabs {
    display: flex;
    gap: 4px;
  }
  
  .tab-btn {
    padding: 6px 12px;
    background: transparent;
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 6px;
    cursor: pointer;
    font-size: 12px;
    color: var(--text-secondary, #64748b);
    transition: all 0.2s ease;
  }
  
  .tab-btn:hover {
    background: var(--bg-hover, #f1f5f9);
    border-color: var(--border-medium, #cbd5e1);
  }
  
  .tab-btn.active {
    background: var(--primary, #3b82f6);
    color: white;
    border-color: var(--primary, #3b82f6);
  }
  
  .analysis-content {
    flex: 1;
    overflow-y: auto;
    padding: 16px 20px;
  }
  
  /* Responsive Design [UX] */
  @media (max-width: 768px) {
    .transcript-split-view {
      flex-direction: column;
    }
    
    .transcript-panel,
    .analysis-panel {
      width: 100% !important;
      height: 50%;
    }
    
    .split-handle {
      width: 100%;
      height: 4px;
      cursor: row-resize;
    }
    
    .split-handle-line {
      width: 40px;
      height: 2px;
    }
  }
</style>
