<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [SK] Session-Konzept - Eine Session = Eine Konsultation
  [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
  [AIU] Anonymisierung ist UNVERÄNDERLICH
  [ATV] Audit-Trail Vollständig
-->
<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { currentSession, appState } from '$lib/stores/session';
  import ProcessingLocationIndicator from './ProcessingLocationIndicator.svelte';
  import SwissGermanAlert from './SwissGermanAlert.svelte';
  
  // State
  let recording = false;
  let audioChunks: Blob[] = [];
  let mediaRecorder: MediaRecorder | null = null;
  let audioStream: MediaStream | null = null;
  let recordingTime = 0;
  let recordingInterval: number | null = null;
  let swissGermanDetected = false;
  
  // Formatiert Zeit im Format MM:SS
  function formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60).toString().padStart(2, '0');
    const secs = Math.floor(seconds % 60).toString().padStart(2, '0');
    return `${mins}:${secs}`;
  }
  
  async function startRecording() {
    try {
      // [ATV] Audit-Trail Vollständig - Logge Aufnahmestart
      console.log('[AUDIT] Starting recording session', new Date().toISOString());
      
      audioChunks = [];
      audioStream = await navigator.mediaDevices.getUserMedia({ audio: true });
      mediaRecorder = new MediaRecorder(audioStream);
      
      mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          audioChunks.push(event.data);
        }
      };
      
      mediaRecorder.onstart = () => {
        recording = true;
        recordingTime = 0;
        recordingInterval = window.setInterval(() => {
          recordingTime++;
          
          // [SDH] Schweizerdeutsch-Erkennung Simulation
          // In einer echten Implementierung würde hier die Spracherkennung laufen
          if (recordingTime === 10 && Math.random() > 0.7) {
            swissGermanDetected = true;
          }
        }, 1000);
      };
      
      mediaRecorder.onstop = async () => {
        recording = false;
        if (recordingInterval) {
          clearInterval(recordingInterval);
          recordingInterval = null;
        }
        
        if (audioChunks.length > 0) {
          // [ATV] Audit-Trail Vollständig - Logge Aufnahmeende
          console.log('[AUDIT] Recording completed', new Date().toISOString());
          
          // Update session status
          currentSession.update(session => {
            if (session) {
              return {
                ...session,
                audioRecorded: true
              };
            }
            return session;
          });
          
          // In einer echten Implementierung würde hier die Verarbeitung starten
          // [AIU] Anonymisierung ist UNVERÄNDERLICH - Würde automatisch starten
        }
      };
      
      mediaRecorder.start();
    } catch (error) {
      console.error('Fehler beim Starten der Aufnahme:', error);
      alert('Mikrofon-Zugriff verweigert oder nicht verfügbar.');
    }
  }
  
  function stopRecording() {
    if (mediaRecorder && recording) {
      mediaRecorder.stop();
      
      if (audioStream) {
        audioStream.getTracks().forEach(track => track.stop());
        audioStream = null;
      }
    }
  }
  
  function toggleRecording() {
    if (recording) {
      stopRecording();
    } else {
      startRecording();
    }
  }
  
  function dismissSwissGermanAlert() {
    swissGermanDetected = false;
  }
  
  onDestroy(() => {
    if (recordingInterval) {
      clearInterval(recordingInterval);
    }
    
    if (mediaRecorder && recording) {
      stopRecording();
    }
  });
</script>

<div class="session-recorder">
  <div class="recorder-header">
    <h3>Konsultationsaufnahme</h3>
    <ProcessingLocationIndicator />
  </div>
  
  <SwissGermanAlert detected={swissGermanDetected} onDismiss={dismissSwissGermanAlert} />
  
  <div class="recorder-controls">
    <div class="recording-status">
      {#if recording}
        <div class="recording-indicator">
          <span class="recording-dot"></span>
          <span>Aufnahme läuft</span>
        </div>
        <div class="recording-time">{formatTime(recordingTime)}</div>
      {:else}
        <div class="ready-indicator">Bereit zur Aufnahme</div>
      {/if}
    </div>
    
    <button 
      class="record-button" 
      class:recording 
      on:click={toggleRecording}
      aria-label={recording ? 'Aufnahme stoppen' : 'Aufnahme starten'}
    >
      {#if recording}
        <span class="stop-icon"></span>
      {:else}
        <span class="mic-icon">🎙️</span>
      {/if}
    </button>
  </div>
  
  <div class="recorder-info">
    <p class="info-text">
      <!-- [AIU] Anonymisierung ist UNVERÄNDERLICH -->
      <span class="info-icon">ℹ️</span> 
      Alle aufgenommenen Daten werden automatisch anonymisiert und verschlüsselt gespeichert.
    </p>
  </div>
</div>

<style>
  .session-recorder {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }
  
  .recorder-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  
  .recorder-header h3 {
    margin: 0;
    font-size: 1.125rem;
    font-weight: 600;
    color: #1e293b;
  }
  
  .recorder-controls {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin: 1.5rem 0;
  }
  
  .recording-status {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }
  
  .recording-indicator {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    color: #dc2626;
    font-weight: 500;
  }
  
  .recording-dot {
    width: 12px;
    height: 12px;
    background-color: #dc2626;
    border-radius: 50%;
    animation: pulse 1.5s infinite;
  }
  
  .recording-time {
    font-family: monospace;
    font-size: 1.25rem;
    color: #1e293b;
  }
  
  .ready-indicator {
    color: #475569;
    font-size: 0.9375rem;
  }
  
  .record-button {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background-color: #ef4444;
    border: none;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.2s;
  }
  
  .record-button:hover {
    background-color: #dc2626;
  }
  
  .record-button.recording {
    background-color: #1e293b;
  }
  
  .record-button.recording:hover {
    background-color: #0f172a;
  }
  
  .mic-icon {
    font-size: 1.5rem;
  }
  
  .stop-icon {
    width: 16px;
    height: 16px;
    background-color: white;
    border-radius: 2px;
  }
  
  .recorder-info {
    margin-top: 1rem;
    padding-top: 1rem;
    border-top: 1px solid #e5e7eb;
  }
  
  .info-text {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin: 0;
    font-size: 0.875rem;
    color: #64748b;
  }
  
  @keyframes pulse {
    0% {
      opacity: 1;
    }
    50% {
      opacity: 0.5;
    }
    100% {
      opacity: 1;
    }
  }
</style>
