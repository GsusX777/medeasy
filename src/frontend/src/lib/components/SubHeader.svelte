<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
  import { onMount } from 'svelte';
  
  // Session Status [TSF]
  let sessionStatus: 'idle' | 'recording' | 'paused' | 'processing' = 'idle';
  let sessionTimer = 0;
  let timerInterval: number | null = null;
  
  // Aktueller Patient [MFD][AIU] - Anonymisierte Anzeige
  let currentPatient = {
    anonymizedName: 'Müller, H.',
    birthYear: '1965',
    sessionDate: '22.07.2025',
    sessionId: 'S-2025-001'
  };
  
  // Session Status Icons [TSF]
  function getSessionStatusIcon(status: typeof sessionStatus): string {
    switch (status) {
      case 'recording': return '🔴';
      case 'paused': return '⏸️';
      case 'processing': return '⚙️';
      default: return '✅';
    }
  }
  
  function getSessionStatusText(status: typeof sessionStatus): string {
    switch (status) {
      case 'recording': return 'Aufnahme';
      case 'paused': return 'Pausiert';
      case 'processing': return 'Verarbeitung';
      default: return 'Bereit';
    }
  }
  
  // Timer-Formatierung [SF]
  function formatTimer(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }
  
  // Aufnahme-Controls [MDL][PSF]
  function startRecording() {
    if (sessionStatus === 'idle' || sessionStatus === 'paused') {
      sessionStatus = 'recording';
      if (!timerInterval) {
        timerInterval = setInterval(() => {
          sessionTimer++;
        }, 1000);
      }
      console.log('🔴 Aufnahme gestartet');
    }
  }
  
  function pauseRecording() {
    if (sessionStatus === 'recording') {
      sessionStatus = 'paused';
      if (timerInterval) {
        clearInterval(timerInterval);
        timerInterval = null;
      }
      console.log('⏸️ Aufnahme pausiert');
    }
  }
  
  function stopRecording() {
    sessionStatus = 'processing';
    if (timerInterval) {
      clearInterval(timerInterval);
      timerInterval = null;
    }
    console.log('⏹️ Aufnahme beendet');
    
    // Simuliere Verarbeitung
    setTimeout(() => {
      sessionStatus = 'idle';
      sessionTimer = 0;
    }, 2000);
  }
  
  onMount(() => {
    // Simuliere Session-Status-Änderungen für Demo
    const statusInterval = setInterval(() => {
      const statuses: typeof sessionStatus[] = ['idle', 'recording', 'paused', 'processing'];
      sessionStatus = statuses[Math.floor(Math.random() * statuses.length)];
    }, 10000);
    
    return () => {
      clearInterval(statusInterval);
    };
  });
</script>

<div class="sub-header">
  <div class="sub-header-content">
    <!-- Aktueller Patient [MFD][AIU] - Anonymisierte Anzeige -->
    <div class="patient-info">
      <span class="patient-icon">👤</span>
      <div class="patient-details">
        <span class="patient-name">{currentPatient.anonymizedName}</span>
        <span class="patient-meta">*{currentPatient.birthYear}</span>
      </div>
    </div>
    
    <!-- Aufnahme-Controls [MDL][PSF] -->
    <div class="recording-controls">
      <button 
        class="control-btn record {sessionStatus === 'recording' ? 'active' : ''}" 
        on:click={startRecording}
        disabled={sessionStatus === 'processing'}
        title="Aufnahme starten"
      >
        🔴
      </button>
      
      <button 
        class="control-btn pause {sessionStatus === 'paused' ? 'active' : ''}" 
        on:click={pauseRecording}
        disabled={sessionStatus !== 'recording'}
        title="Aufnahme pausieren"
      >
        ⏸️
      </button>
      
      <button 
        class="control-btn stop" 
        on:click={stopRecording}
        disabled={sessionStatus === 'idle' || sessionStatus === 'processing'}
        title="Aufnahme beenden"
      >
        ⏹️
      </button>
      
      <div class="status-indicator {sessionStatus}">
        <span class="status-icon">{getSessionStatusIcon(sessionStatus)}</span>
        <span class="status-text">{getSessionStatusText(sessionStatus)}</span>
      </div>
    </div>
    
    <!-- Session Info & Timer [TSF][SF] -->
    <div class="session-info">
      <span class="session-icon">📋</span>
      <div class="session-details">
        <span class="session-main-line">{currentPatient.sessionDate} • {currentPatient.sessionId} • {formatTimer(sessionTimer)}</span>
      </div>
    </div>
    
    <!-- [SDH] Schweizerdeutsch-Warnung - Nur bei Bedarf -->
    <div class="language-status">
      <span class="status-badge language">🇩🇪 Hochdeutsch</span>
    </div>
  </div>
</div>

<style>
  .sub-header {
    background: linear-gradient(135deg, #1e293b 0%, #334155 100%);
    border-bottom: 1px solid rgba(255, 255, 255, 0.15);
    /* Leichter Schatten zur Abgrenzung */
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.15);
    /* Full-width aber Content erst nach Sidebar [SF] */
    width: 100%;
    position: relative;
  }
  
  .sub-header-content {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.75rem 1.5rem;
    height: 35px; /* Höher für größere Buttons */
    /* Content beginnt erst nach Sidebar-Breite [SF] */
    margin-left: 280px;
    gap: 1.5rem;
  }
  
  .patient-info {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  
  .patient-icon {
    font-size: 1rem;
    color: rgba(255, 255, 255, 0.8);
  }
  
  .patient-details {
    display: flex;
    flex-direction: column;
    gap: 0.125rem;
  }
  
  .patient-name {
    font-size: 0.8rem;
    font-weight: 600;
    color: white;
  }
  
  .patient-meta {
    font-size: 0.7rem;
    color: rgba(255, 255, 255, 0.7);
    font-family: 'JetBrains Mono', 'Courier New', monospace;
  }
  
  .session-status {
    display: flex;
    align-items: center;
  }
  
  .status-indicator {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.8rem;
    font-weight: 500;
    transition: all 0.3s ease;
    /* Feste Breite für Layout-Stabilität */
    width: 120px;
    justify-content: center;
    color: white; /* Weiße Schrift für bessere Lesbarkeit */
  }
  
  .status-indicator.idle {
    background: rgba(34, 197, 94, 0.2);
    border: 1px solid rgba(34, 197, 94, 0.3);
  }
  
  .status-indicator.recording {
    background: rgba(239, 68, 68, 0.2);
    border: 1px solid rgba(239, 68, 68, 0.3);
    animation: pulse 2s infinite;
  }
  
  .status-indicator.paused {
    background: rgba(251, 191, 36, 0.2);
    border: 1px solid rgba(251, 191, 36, 0.3);
  }
  
  .status-indicator.processing {
    background: rgba(59, 130, 246, 0.2);
    border: 1px solid rgba(59, 130, 246, 0.3);
  }
  
  /* Session Info & Timer [SF] */
  .session-info {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .session-icon {
    font-size: 1rem;
    opacity: 0.8;
  }
  
  .session-details {
    display: flex;
    flex-direction: column;
  }
  
  .session-main-line {
    font-size: 0.8rem;
    color: rgba(255, 255, 255, 0.9);
    font-weight: 500;
    font-family: 'JetBrains Mono', 'Courier New', monospace;
  }
  
  /* Recording Controls [MDL][PSF] */
  .recording-controls {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .control-btn {
    background: transparent;
    border: none;
    color: white;
    padding: 0;
    cursor: pointer;
    font-size: 2.5rem;
    transition: all 0.2s ease;
    width: 48px;
    height: 48px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: normal;
  }
  
  .control-btn:hover:not(:disabled) {
    filter: brightness(1.3);
  }
  
  .control-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .control-btn.record.active {
    color: #ef4444;
    animation: pulse 2s infinite;
  }
  
  .control-btn.pause.active {
    color: #fbbf24;
  }
  
  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.7; }
  }
  
  .language-status {
    display: flex;
    align-items: center;
  }
  
  .status-badge {
    background: rgba(255, 255, 255, 0.1);
    border: 1px solid rgba(255, 255, 255, 0.2);
    color: rgba(255, 255, 255, 0.9);
    padding: 0.25rem 0.75rem;
    border-radius: 15px;
    font-size: 0.7rem;
    font-weight: 500;
  }
  
  .status-badge.language {
    background: rgba(34, 197, 94, 0.2);
    border: 1px solid rgba(34, 197, 94, 0.3);
    color: rgba(255, 255, 255, 0.9);
  }
</style>
