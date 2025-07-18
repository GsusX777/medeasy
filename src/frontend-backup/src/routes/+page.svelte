<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
  [AIU] Anonymisierung ist UNVERÄNDERLICH
  [SF] Schweizer Formate - Datum: DD.MM.YYYY
  [MFD] Medizinische Fachbegriffe DE-CH
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { currentSession, appState, initializeApp, startNewSession } from '$lib/stores/session';
  import AppLayout from '$lib/components/AppLayout.svelte';
  import ProcessingLocationIndicator from '$lib/components/ProcessingLocationIndicator.svelte';
  import AnonymizationNotice from '$lib/components/AnonymizationNotice.svelte';
  import SessionRecorder from '$lib/components/SessionRecorder.svelte';
  import TranscriptViewer from '$lib/components/TranscriptViewer.svelte';
  import SecuritySettings from '$lib/components/SecuritySettings.svelte';
  
  let loading = true;
  let error: string | null = null;
  let showAnonymizationDetails = false;
  let demoTranscript = "";
  
  // [SF] Schweizer Formate - Datum: DD.MM.YYYY
  function formatSwissDate(date: Date): string {
    return date.toLocaleDateString('de-CH');
  }
  
  onMount(async () => {
    try {
      await initializeApp();
      
      // Demo-Transkript für Vorschau
      demoTranscript = "Guten Tag Herr Müller, wie geht es Ihnen heute? Haben die Medikamente angeschlagen, die ich Ihnen beim letzten Besuch im Spital verschrieben habe?\n\nJa Doktor, die Schmerzen sind besser geworden. Aber ich habe immer noch Probleme beim Treppensteigen. Besonders in meiner Wohnung an der Hauptstrasse 42 ist das ein Problem.\n\nIch verstehe. Lassen Sie mich das in Ihrer Krankengeschichte notieren. Ihre Versicherungsnummer war 756.1234.5678.90, richtig?";
      
      loading = false;
    } catch (err) {
      error = `Fehler beim Laden: ${err}`;
      loading = false;
    }
  });
  
  async function handleNewSession() {
    try {
      await startNewSession();
    } catch (err) {
      error = `Fehler beim Erstellen einer neuen Session: ${err}`;
    }
  }
</script>

<AppLayout>
  {#if loading}
    <div class="loading">
      <div class="spinner"></div>
      <p>MedEasy wird geladen...</p>
    </div>
  {:else if error}
    <div class="error">
      <h2>Fehler beim Laden</h2>
      <p>{error}</p>
      <button on:click={() => window.location.reload()}>Neu laden</button>
    </div>
  {:else}
    <div class="dashboard">
      <section class="welcome">
        <h2>Willkommen bei MedEasy</h2>
        <p>Ihre sichere Lösung für medizinische Dokumentation in der Schweiz</p>
        
        <!-- [AIU] Anonymisierung ist UNVERÄNDERLICH -->
        <AnonymizationNotice bind:showDetails={showAnonymizationDetails} />
      </section>
      
      <div class="dashboard-grid">
        <div class="main-column">
          <section class="session-section">
            {#if $currentSession}
              <div class="session-header">
                <h3>Aktive Konsultation</h3>
                <span class="session-date">Gestartet: {formatSwissDate($currentSession.startTime)}</span>
              </div>
              
              <!-- Recorder Component -->
              <SessionRecorder />
              
              <!-- Transcript Component -->
              <div class="transcript-container">
                <TranscriptViewer 
                  sessionId={$currentSession.id} 
                  transcript={demoTranscript}
                  loading={false}
                />
              </div>
            {:else}
              <div class="no-session">
                <h3>Keine aktive Konsultation</h3>
                <p>Starten Sie eine neue Konsultation, um Aufnahmen zu machen und Transkripte zu erstellen.</p>
                <button class="primary-button" on:click={handleNewSession}>
                  Neue Konsultation starten
                </button>
              </div>
            {/if}
          </section>
        </div>
        
        <div class="side-column">
          <!-- Security Settings Component -->
          <SecuritySettings />
          
          <section class="quick-stats">
            <h3>Statistiken</h3>
            <div class="stats-grid">
              <div class="stat-item">
                <span class="stat-value">24</span>
                <span class="stat-label">Konsultationen</span>
              </div>
              <div class="stat-item">
                <span class="stat-value">12</span>
                <span class="stat-label">Patienten</span>
              </div>
              <div class="stat-item">
                <span class="stat-value">8h</span>
                <span class="stat-label">Aufnahmezeit</span>
              </div>
              <div class="stat-item">
                <span class="stat-value">100%</span>
                <span class="stat-label">Sicher</span>
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  {/if}
</AppLayout>

<style>
  .loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 50vh;
    gap: 1rem;
  }
  
  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid rgba(0, 0, 0, 0.1);
    border-left-color: #2563eb;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }
  
  .error {
    text-align: center;
    padding: 2rem;
    background-color: #fef2f2;
    border-radius: 8px;
    border: 1px solid #fecaca;
    margin: 2rem auto;
    max-width: 500px;
  }
  
  .error h2 {
    color: #dc2626;
    margin-top: 0;
  }
  
  .error button {
    background-color: #dc2626;
    color: white;
    border: none;
    padding: 0.5rem 1rem;
    border-radius: 4px;
    font-weight: 500;
    cursor: pointer;
    margin-top: 1rem;
  }
  
  .dashboard {
    max-width: 1200px;
    margin: 0 auto;
  }
  
  .welcome {
    margin-bottom: 2rem;
  }
  
  .welcome h2 {
    margin-top: 0;
    color: #1e293b;
  }
  
  .dashboard-grid {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 1.5rem;
  }
  
  .session-section {
    margin-bottom: 1.5rem;
  }
  
  .session-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  
  .session-header h3 {
    margin: 0;
  }
  
  .session-date {
    font-size: 0.875rem;
    color: #64748b;
  }
  
  .transcript-container {
    margin-top: 1.5rem;
  }
  
  .no-session {
    background-color: white;
    border-radius: 8px;
    padding: 2rem;
    text-align: center;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }
  
  .no-session h3 {
    margin-top: 0;
  }
  
  .primary-button {
    background-color: #2563eb;
    color: white;
    border: none;
    padding: 0.75rem 1.5rem;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: background-color 0.2s;
  }
  
  .primary-button:hover {
    background-color: #1d4ed8;
  }
  
  .quick-stats {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    margin-top: 1.5rem;
  }
  
  .quick-stats h3 {
    margin-top: 0;
    margin-bottom: 1rem;
    font-size: 1.125rem;
  }
  
  .stats-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
  }
  
  .stat-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 1rem;
    background-color: #f8fafc;
    border-radius: 6px;
  }
  
  .stat-value {
    font-size: 1.5rem;
    font-weight: 600;
    color: #2563eb;
  }
  
  .stat-label {
    font-size: 0.875rem;
    color: #64748b;
    margin-top: 0.25rem;
  }
  
  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
  
  @media (max-width: 768px) {
    .dashboard-grid {
      grid-template-columns: 1fr;
    }
  }
</style>
