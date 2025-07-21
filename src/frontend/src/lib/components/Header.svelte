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
    
    return () => {
      clearInterval(interval);
    };
  });
</script>

<header class="app-header">
  <div class="header-left">
    <!-- MedEasy Logo -->
    <div class="logo">
      <img src="/logo.svg" alt="MedEasy" class="logo-image" />
    </div>
    
    <!-- Datum/Zeit [SF] -->
    <div class="datetime">
      <span class="date">{formatSwissDate(currentDate)}</span>
      <span class="time">{formatSwissTime(currentTime)}</span>
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
    padding: 0.5rem 1.5rem;
    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
    color: white;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    min-height: 55px;
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
    width: 160px;
    height: 48px;
    /* Original-Logo-Farben beibehalten */
  }
  

  

  

  
  .datetime {
    display: flex;
    gap: 0.5rem;
    font-size: 0.75rem;
    color: rgba(255, 255, 255, 0.8);
    font-family: 'JetBrains Mono', 'Courier New', monospace;
    /* Feste Breite für Layout-Stabilität - verhindert Größenänderungen */
    width: 140px;
    justify-content: center;
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
    background: rgba(239, 68, 68, 0.5);
    border: 1px solid rgba(239, 68, 68, 0.7);
    /* Deutlicher Hover-Effekt für kritische Notfall-Funktion */
    box-shadow: 0 0 10px rgba(239, 68, 68, 0.4);
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
