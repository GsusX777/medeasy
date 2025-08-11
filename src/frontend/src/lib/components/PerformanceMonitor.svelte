<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [WMM] Whisper Multi-Model - Überwachung der Transkriptions-Performance
  [PSF] Patient Safety First - Medizinisch relevante Performance-Metriken
  [SF] Schweizer Formate - Schweizerdeutsch-Erkennung überwachen
  [TSF] Tauri 1.5 + Svelte 4 Stack
-->
<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { invoke } from '@tauri-apps/api/tauri';
  
  // TypeScript Interface für Whisper Performance [WMM][PSF]
  interface WhisperPerformanceDto {
    // Model Information [WMM]
    activeModel: 'tiny' | 'small' | 'medium' | 'large';
    modelSize: number; // MB
    modelLoadTime: number; // Sekunden
    
    // Processing Performance [PSF]
    transcriptionSpeed: number; // Real-time factor (1.0 = Echtzeit)
    averageLatency: number; // ms pro Audio-Sekunde
    queueLength: number; // Wartende Audio-Chunks
    processingTime: number; // Aktuelle Verarbeitungszeit
    
    // Resource Usage (Whisper-specific) [PSF]
    whisperCpuUsage: number; // CPU nur für Whisper
    whisperRamUsage: number; // RAM nur für Whisper (MB)
    whisperGpuUsage?: number; // GPU nur für Whisper (falls CUDA)
    
    // Medical Context [SF][PSF]
    confidenceScore: number; // Durchschnittliche Erkennungsqualität (0-100)
    swissGermanDetected: boolean; // Schweizerdeutsch erkannt
    medicalTermsCount: number; // Erkannte medizinische Begriffe
    sessionDuration: number; // Aktuelle Session-Dauer in Sekunden
    
    // System Context (minimal) [TSF]
    availableRamMb: number; // Verfügbarer RAM für Model-Switching
    gpuAvailable: boolean; // CUDA/GPU verfügbar
    cpuCores: number; // Für Parallelisierung
    timestamp: string;
  }
  
  // Whisper Performance Metrics [WMM][PSF]
  let activeModel: string = 'Nicht geladen';
  let modelSize = 0;
  let modelLoadTime = 0;
  let transcriptionSpeed = 0;
  let averageLatency = 0;
  let queueLength = 0;
  let processingTime = 0;
  
  // Whisper Resource Usage [PSF]
  let whisperCpuUsage = 0;
  let whisperRamUsage = 0;
  let whisperGpuUsage = 0;
  
  // Medical Context [SF][PSF]
  let confidenceScore = 0;
  let swissGermanDetected = false;
  let medicalTermsCount = 0;
  let sessionDuration = 0;
  
  // System Context (minimal) [TSF]
  let availableRamMb = 0;
  let gpuAvailable = false;
  let cpuCores = 0;
  let systemStatus = 'Unknown';
  let lastUpdate = 'Never';
  
  // Error handling
  let apiError = false;
  let errorMessage = '';
  
  // Update interval
  let updateInterval: NodeJS.Timeout;
  
  // Props
  export let expanded = false;
  
  // API Base URL [TSF] - Desktop App mit lokalem Backend
  const API_BASE_URL = 'http://localhost:5155';
  
  // Get Whisper performance data from Backend API [WMM][PSF]
  async function updateWhisperMetrics() {
    try {
      // TODO: Implement Whisper-specific API endpoint
      const response = await fetch(`${API_BASE_URL}/api/whisper/performance`);
      
      if (!response.ok) {
        throw new Error(`Whisper API Error: ${response.status} ${response.statusText}`);
      }
      
      const metrics: WhisperPerformanceDto = await response.json();
      
      // Update Whisper-specific metrics [WMM][PSF]
      activeModel = metrics.activeModel;
      modelSize = metrics.modelSize;
      modelLoadTime = metrics.modelLoadTime;
      transcriptionSpeed = Math.round(metrics.transcriptionSpeed * 100) / 100;
      averageLatency = Math.round(metrics.averageLatency);
      queueLength = metrics.queueLength;
      processingTime = Math.round(metrics.processingTime);
      
      // Whisper Resource Usage [PSF]
      whisperCpuUsage = Math.round(metrics.whisperCpuUsage * 10) / 10;
      whisperRamUsage = Math.round(metrics.whisperRamUsage);
      whisperGpuUsage = metrics.whisperGpuUsage ? Math.round(metrics.whisperGpuUsage * 10) / 10 : 0;
      
      // Medical Context [SF][PSF]
      confidenceScore = Math.round(metrics.confidenceScore * 10) / 10;
      swissGermanDetected = metrics.swissGermanDetected;
      medicalTermsCount = metrics.medicalTermsCount;
      sessionDuration = metrics.sessionDuration;
      
      // System Context [TSF]
      availableRamMb = metrics.availableRamMb;
      gpuAvailable = metrics.gpuAvailable;
      cpuCores = metrics.cpuCores;
      lastUpdate = new Date().toLocaleTimeString('de-CH');
      
      // Clear error state [FSD]
      apiError = false;
      errorMessage = '';
      
      console.log('✅ Whisper metrics updated:', { activeModel, transcriptionSpeed, confidenceScore, queueLength });
      
    } catch (error) {
      console.error('❌ Fehler beim Laden der Whisper-Daten:', error);
      apiError = true;
      errorMessage = error instanceof Error ? error.message : 'Whisper API nicht verfügbar';
      
      // Fallback zu Mock-Daten für Demo [FSD]
      activeModel = 'small (Mock)';
      modelSize = 74;
      transcriptionSpeed = 0.8 + Math.random() * 0.4; // 0.8-1.2x
      averageLatency = 150 + Math.floor(Math.random() * 100);
      queueLength = Math.floor(Math.random() * 5);
      whisperCpuUsage = 15 + Math.random() * 25;
      whisperRamUsage = 200 + Math.random() * 300;
      confidenceScore = 85 + Math.random() * 10;
      medicalTermsCount = Math.floor(Math.random() * 15);
      lastUpdate = 'Mock-Daten (Demo)';
    }
  }
  
  // Check system health status [PSF][ZTS]
  async function checkSystemHealth() {
    try {
      const response = await fetch(`${API_BASE_URL}/health`);
      
      if (!response.ok) {
        throw new Error(`Health Check Failed: ${response.status}`);
      }
      
      const health = await response.json();
      systemStatus = health.status; // "Healthy"
      
      console.log('✅ System health checked:', health);
      
    } catch (error) {
      console.error('❌ Health check failed:', error);
      systemStatus = 'Unhealthy';
    }
  }
  
  onMount(async () => {
    // Initial Whisper health check [WMM][PSF]
    await checkSystemHealth();
    
    // Update Whisper metrics immediately
    await updateWhisperMetrics();
    
    // Set up interval for regular updates (every 2 seconds for Whisper) [PSF]
    updateInterval = setInterval(async () => {
      await updateWhisperMetrics();
      await checkSystemHealth();
    }, 2000);
    
    console.log('🚀 WhisperMonitor initialized with real API integration');
  });
  
  onDestroy(() => {
    if (updateInterval) {
      clearInterval(updateInterval);
    }
  });
  
  // Get color based on Whisper performance [WMM][PSF]
  function getPerformanceColor(value: number, type: 'speed' | 'confidence' | 'usage'): string {
    if (type === 'speed') {
      if (value >= 1.0) return 'var(--success)'; // Echtzeit oder schneller
      if (value >= 0.7) return 'var(--warning)'; // Langsamer als Echtzeit
      return 'var(--danger)'; // Kritisch langsam
    }
    if (type === 'confidence') {
      if (value >= 90) return 'var(--success)';
      if (value >= 75) return 'var(--warning)';
      return 'var(--danger)'; // Niedrige Konfidenz
    }
    // usage
    if (value < 50) return 'var(--success)';
    if (value < 80) return 'var(--warning)';
    return 'var(--danger)';
  }
  
  // Get performance status text [SF][WMM]
  function getPerformanceStatus(value: number, type: 'speed' | 'confidence' | 'usage'): string {
    if (type === 'speed') {
      if (value >= 1.0) return 'Echtzeit';
      if (value >= 0.7) return 'Verzögert';
      return 'Kritisch';
    }
    if (type === 'confidence') {
      if (value >= 90) return 'Hoch';
      if (value >= 75) return 'Mittel';
      return 'Niedrig';
    }
    // usage
    if (value < 50) return 'Normal';
    if (value < 80) return 'Hoch';
    return 'Kritisch';
  }
</script>

{#if expanded}
  <div class="whisper-performance">
    <!-- Whisper Status Header [WMM][PSF] -->
    <div class="status-header">
      <div class="status-indicator" class:healthy={systemStatus === 'Healthy'} class:unhealthy={systemStatus !== 'Healthy'}>
        <span class="status-dot"></span>
        <span class="status-text">🎤 Whisper: {systemStatus}</span>
      </div>
      <div class="last-update">
        <span class="update-text">Letzte Aktualisierung: {lastUpdate}</span>
      </div>
    </div>
    
    <!-- API Error Warning [FSD] -->
    {#if apiError}
      <div class="error-banner">
        <span class="error-icon">⚠️</span>
        <span class="error-message">Whisper API: {errorMessage}</span>
        <span class="error-fallback">Demo-Modus aktiv</span>
      </div>
    {/if}
    
    <!-- Active Model Info [WMM] -->
    <div class="model-section">
      <div class="model-header">
        <span class="model-label">🤖 Aktives Modell</span>
        <span class="model-name">{activeModel}</span>
      </div>
      <div class="model-details">
        <span class="model-size">{modelSize} MB</span>
        <span class="load-time">Ladezeit: {modelLoadTime}s</span>
      </div>
    </div>
    
    <!-- Transcription Performance [PSF] -->
    <div class="metric">
      <div class="metric-header">
        <span class="metric-label">⏱️ Transkriptions-Geschwindigkeit</span>
        <span class="metric-status" style="color: {getPerformanceColor(transcriptionSpeed, 'speed')}">
          {getPerformanceStatus(transcriptionSpeed, 'speed')}
        </span>
      </div>
      <div class="metric-bar">
        <div 
          class="metric-fill" 
          style="width: {Math.min(transcriptionSpeed * 100, 100)}%; background-color: {getPerformanceColor(transcriptionSpeed, 'speed')}"
        ></div>
      </div>
      <div class="metric-value">{transcriptionSpeed.toFixed(2)}x Echtzeit</div>
    </div>
    
    <!-- Confidence Score [SF][PSF] -->
    <div class="metric">
      <div class="metric-header">
        <span class="metric-label">🎯 Erkennungsqualität</span>
        <span class="metric-status" style="color: {getPerformanceColor(confidenceScore, 'confidence')}">
          {getPerformanceStatus(confidenceScore, 'confidence')}
        </span>
      </div>
      <div class="metric-bar">
        <div 
          class="metric-fill" 
          style="width: {confidenceScore}%; background-color: {getPerformanceColor(confidenceScore, 'confidence')}"
        ></div>
      </div>
      <div class="metric-value">{confidenceScore.toFixed(1)}% Konfidenz</div>
    </div>
    
    <!-- Whisper Resource Usage [PSF] -->
    <div class="resource-section">
      <h4 class="resource-title">📊 Whisper Ressourcen</h4>
      
      <div class="resource-metric">
        <div class="resource-header">
          <span class="resource-label">CPU (Whisper)</span>
          <span class="resource-value">{whisperCpuUsage.toFixed(1)}%</span>
        </div>
        <div class="resource-bar">
          <div 
            class="resource-fill" 
            style="width: {whisperCpuUsage}%; background-color: {getPerformanceColor(whisperCpuUsage, 'usage')}"
          ></div>
        </div>
      </div>
      
      <div class="resource-metric">
        <div class="resource-header">
          <span class="resource-label">RAM (Whisper)</span>
          <span class="resource-value">{whisperRamUsage} MB</span>
        </div>
        <div class="resource-bar">
          <div 
            class="resource-fill" 
            style="width: {Math.min((whisperRamUsage / 1000) * 100, 100)}%; background-color: {getPerformanceColor((whisperRamUsage / 1000) * 100, 'usage')}"
          ></div>
        </div>
      </div>
      
      {#if gpuAvailable && whisperGpuUsage > 0}
        <div class="resource-metric">
          <div class="resource-header">
            <span class="resource-label">GPU (CUDA)</span>
            <span class="resource-value">{whisperGpuUsage.toFixed(1)}%</span>
          </div>
          <div class="resource-bar">
            <div 
              class="resource-fill" 
              style="width: {whisperGpuUsage}%; background-color: {getPerformanceColor(whisperGpuUsage, 'usage')}"
            ></div>
          </div>
        </div>
      {/if}
    </div>
    
    <!-- Medical Context [SF][PSF] -->
    <div class="medical-section">
      <h4 class="medical-title">🏥 Medizinischer Kontext</h4>
      
      <div class="medical-metrics">
        <div class="medical-row">
          <span class="medical-label">Session-Dauer:</span>
          <span class="medical-value">{Math.floor(sessionDuration / 60)}:{(sessionDuration % 60).toString().padStart(2, '0')} min</span>
        </div>
        <div class="medical-row">
          <span class="medical-label">Medizinische Begriffe:</span>
          <span class="medical-value">{medicalTermsCount} erkannt</span>
        </div>
        <div class="medical-row">
          <span class="medical-label">Verarbeitungsqueue:</span>
          <span class="medical-value">{queueLength} Chunks</span>
        </div>
        {#if swissGermanDetected}
          <div class="medical-row swiss-german">
            <span class="medical-label">🇨🇭 Schweizerdeutsch:</span>
            <span class="medical-value">Erkannt (Beta)</span>
          </div>
        {/if}
      </div>
    </div>
    
    <!-- Performance Warning [PSF] -->
    {#if transcriptionSpeed < 0.7 || confidenceScore < 75 || whisperCpuUsage > 80}
      <div class="performance-warning">
        <span class="warning-icon">⚠️</span>
        <span class="warning-text">Whisper Performance-Problem erkannt</span>
      </div>
    {/if}
  </div>
{/if}

<style>
  .whisper-details {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    margin-top: 0.5rem;
  }
  
  /* Status Header [PSF] */
  .status-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.5rem;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 6px;
    margin-bottom: 0.5rem;
  }
  
  .status-indicator {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .status-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: var(--danger);
    animation: pulse 2s infinite;
  }
  
  .status-indicator.healthy .status-dot {
    background: var(--success);
  }
  
  .status-indicator.unhealthy .status-dot {
    background: var(--danger);
  }
  
  .status-text {
    color: rgba(255, 255, 255, 0.9);
    font-size: 0.75rem;
    font-weight: 600;
  }
  
  .last-update {
    display: flex;
    align-items: center;
  }
  
  .update-text {
    color: rgba(255, 255, 255, 0.6);
    font-size: 0.625rem;
  }
  
  /* Error Banner [FSD] */
  .error-banner {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem;
    background: rgba(239, 68, 68, 0.15);
    border: 1px solid rgba(239, 68, 68, 0.4);
    border-radius: 6px;
    margin-bottom: 0.5rem;
  }
  
  .error-icon {
    font-size: 1rem;
  }
  
  .error-message {
    color: #fca5a5;
    font-size: 0.75rem;
    font-weight: 600;
  }
  
  .error-fallback {
    color: rgba(255, 255, 255, 0.6);
    font-size: 0.625rem;
    margin-left: auto;
  }
  
  /* Medical Metrics [PSF] */
  .medical-metrics {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    padding: 0.5rem;
    background: rgba(255, 255, 255, 0.03);
    border-radius: 4px;
    border: 1px solid rgba(255, 255, 255, 0.1);
  }

  /* Whisper Performance Styles werden im HTML nicht verwendet - entfernt für sauberen Code */

  .performance-warning {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem;
    background: rgba(239, 68, 68, 0.1);
    border: 1px solid rgba(239, 68, 68, 0.3);
    border-radius: 4px;
    margin-top: 0.25rem;
  }
  
  .warning-icon {
    font-size: 0.875rem;
  }
  
  .warning-text {
    color: #fca5a5;
    font-size: 0.75rem;
    font-weight: 500;
  }
  
  /* Animations [UX] */
  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.5; }
  }
  
  .metric-fill {
    transition: width 0.3s ease-in-out;
  }
  
  /* Responsive adjustments */
  @media (max-width: 768px) {
    .whisper-details {
      gap: 0.5rem;
    }
    
    .status-header {
      flex-direction: column;
      gap: 0.25rem;
      text-align: center;
    }
    
    .metric-header {
      font-size: 0.625rem;
    }
    
    .metric-bar {
      height: 4px;
    }
    
    .medical-metrics {
      padding: 0.375rem;
    }
    
    .error-banner {
      flex-direction: column;
      text-align: center;
      gap: 0.25rem;
    }
  }
</style>
