<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
  [SF] Schweizer Formate - Datum: DD.MM.YYYY
  [MFD] Medizinische Fachbegriffe DE-CH
  [AIU] Anonymisierung ist unveränderlich
  [SDH] Schweizerdeutsch-Handling
  [DK] Diagnose-Killswitch
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { appState } from '$lib/stores/session';
  import SecurityBadge from './SecurityBadge.svelte';
  // TODO: Import andere Komponenten wenn verfügbar
  // import AnonymizationNotice from './AnonymizationNotice.svelte';
  // import SwissGermanAlert from './SwissGermanAlert.svelte';
  
  // Props
  export let title = 'MedEasy';
  
  // State
  let currentDate = new Date();
  let currentTime = new Date();
  let sessionStatus: 'idle' | 'recording' | 'paused' | 'processing' = 'idle';
  
  // [SF] Schweizer Formate - Datum: DD.MM.YYYY, Zeit: HH:mm:ss
  function formatSwissDate(date: Date): string {
    return date.toLocaleDateString('de-CH'); // DD.MM.YYYY Format
  }
  
  function formatSwissTime(date: Date): string {
    return date.toLocaleTimeString('de-CH', { 
      hour: '2-digit', 
      minute: '2-digit', 
      second: '2-digit' 
    }); // HH:mm:ss Format
  }
  
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
      case 'recording': return 'Aufnahme läuft';
      case 'paused': return 'Pausiert';
      case 'processing': return 'Verarbeitung';
      default: return 'Bereit';
    }
  }
  
  // [DK] Diagnose-Killswitch - Notfall-Funktionen
  function handleEmergencyStop() {
    // Sofortiger Stopp aller AI-Funktionen
    console.log('🚨 NOTFALL-STOPP: Alle AI-Funktionen deaktiviert');
    // TODO: Implementiere Killswitch-Logik
  }
  
  onMount(() => {
    // Aktualisiere Datum und Zeit jede Sekunde
    const interval = setInterval(() => {
      currentDate = new Date();
      currentTime = new Date();
    }, 1000);
    
    // Simuliere Session-Status-Änderungen für Demo
    const statusInterval = setInterval(() => {
      const statuses: typeof sessionStatus[] = ['idle', 'recording', 'paused', 'processing'];
      sessionStatus = statuses[Math.floor(Math.random() * statuses.length)];
    }, 10000);
    
    return () => {
      clearInterval(interval);
      clearInterval(statusInterval);
    };
  });
</script>

<header class="app-header">
  <div class="header-left">
    <!-- MedEasy Logo -->
    <div class="logo">
      <img src="/logo.svg" alt="MedEasy" class="logo-image" />
    </div>
    
    <!-- Session Status [TSF] -->
    <div class="session-status">
      <div class="status-indicator {sessionStatus}">
        <span class="status-icon">{getSessionStatusIcon(sessionStatus)}</span>
        <span class="status-text">{getSessionStatusText(sessionStatus)}</span>
      </div>
      <div class="datetime">
        <span class="date">{formatSwissDate(currentDate)}</span>
        <span class="time">{formatSwissTime(currentTime)}</span>
      </div>
    </div>
  </div>
  
  <div class="header-center">
    <!-- [SDH] Schweizerdeutsch-Warnung - Nur bei Bedarf -->
    <div class="language-status">
      <span class="status-badge language">🇩🇪 Hochdeutsch</span>
    </div>
  </div>
  
  <div class="header-right">
    <!-- [DK] Notfall-Funktionen -->
    <button class="emergency-stop" on:click={handleEmergencyStop} title="Notfall-Stopp: Alle AI-Funktionen deaktivieren">
      🚨
    </button>
    
    <!-- Benutzer-Info [MFD] -->
    <div class="user-menu">
      <span class="user-icon">👨‍⚕️</span>
      <div class="user-info">
        <span class="user-name">Dr. med. Müller</span>
        <span class="user-role">Hausarzt</span>
      </div>
    </div>
  </div>
</header>

<style>
  .app-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 1.5rem;
    background: linear-gradient(135deg, #2c5aa0 0%, #1e3a8a 100%);
    color: white;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    min-height: 70px;
  }
  
  .header-left {
    display: flex;
    align-items: center;
    gap: 2rem;
  }
  
  .logo {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  
  .logo-image {
    width: 200px;
    height: 60px;
    /* Original-Logo-Farben beibehalten */
  }
  

  
  .session-status {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }
  
  .status-indicator {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.875rem;
    font-weight: 500;
    transition: all 0.3s ease;
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
    animation: spin 2s linear infinite;
  }
  
  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.7; }
  }
  
  @keyframes spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
  }
  
  .datetime {
    display: flex;
    gap: 0.5rem;
    font-size: 0.75rem;
    color: rgba(255, 255, 255, 0.8);
  }
  
  .header-center {
    display: flex;
    align-items: center;
    gap: 1rem;
  }
  
  .language-status {
    display: flex;
    align-items: center;
  }
  
  .status-badge {
    padding: 0.25rem 0.75rem;
    border-radius: 15px;
    font-size: 0.75rem;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.375rem;
  }
  
  .status-badge.transcription {
    background: rgba(34, 197, 94, 0.2);
    border: 1px solid rgba(34, 197, 94, 0.3);
    color: rgba(255, 255, 255, 0.9);
  }
  
  .status-badge.language {
    background: rgba(59, 130, 246, 0.2);
    border: 1px solid rgba(59, 130, 246, 0.3);
    color: rgba(255, 255, 255, 0.9);
  }
  
  .header-right {
    display: flex;
    align-items: center;
    gap: 1rem;
  }
  
  .emergency-stop {
    background: rgba(239, 68, 68, 0.2);
    border: 1px solid rgba(239, 68, 68, 0.3);
    color: white;
    padding: 0.5rem;
    border-radius: 50%;
    cursor: pointer;
    font-size: 1.2rem;
    transition: all 0.3s ease;
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  
  .emergency-stop:hover {
    background: rgba(239, 68, 68, 0.3);
    transform: scale(1.1);
  }
  
  .user-menu {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.5rem 1rem;
    border-radius: 25px;
    background: rgba(255, 255, 255, 0.1);
    border: 1px solid rgba(255, 255, 255, 0.2);
    cursor: pointer;
    transition: all 0.3s ease;
  }
  
  .user-menu:hover {
    background: rgba(255, 255, 255, 0.15);
  }
  
  .user-icon {
    font-size: 1.5rem;
  }
  
  .user-info {
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
  }
  
  .user-name {
    font-weight: 600;
    font-size: 0.875rem;
  }
  
  .user-role {
    font-size: 0.75rem;
    color: rgba(255, 255, 255, 0.8);
  }
  
  /* Responsive Design */
  @media (max-width: 768px) {
    .app-header {
      flex-direction: column;
      gap: 1rem;
      padding: 1rem;
    }
    
    .header-left,
    .header-center,
    .header-right {
      width: 100%;
      justify-content: center;
    }
    
    .session-status {
      flex-direction: row;
      align-items: center;
      gap: 1rem;
    }
    
    .datetime {
      flex-direction: column;
      gap: 0.25rem;
    }
  }
</style>
