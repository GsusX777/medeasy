<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [TSF] Tauri 1.5 + Svelte 4 Stack
  [ZTS] Zero Tolerance Security - Sicherheitsfeatures niemals deaktivierbar
  [AIU] Anonymisierung ist unveränderlich - immer aktiv
  [CT] Cloud-Transparenz - zeigt Verarbeitungsort
  [SF] Schweizer Formate - Deutsche Sprache
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { appState } from '$lib/stores/session';
  import type { AppState } from '$lib/types/app-state';
  
  // System Status Monitoring [UX][PSF]
  let systemExpanded = true;
  let audioExpanded = false;
  let providerExpanded = false;
  let networkExpanded = false;
  
  // Audio Status [CT]
  let audioQuality = 'Gut';
  let micLevel = 0;
  let noiseReduction = true;
  let audioProvider = 'Whisper Large';
  
  // AI Provider Status [PK]
  let currentProvider = 'OpenAI GPT-4';
  let providerLatency = 245;
  let providerStatus = 'Online';
  let fallbackAvailable = true;
  
  // Network Status [CT]
  let networkStatus = 'Verbunden';
  let networkLatency = 23;
  let cloudProcessing = false;
  
  // Session Timer [SF]
  let sessionActive = true;
  let sessionDuration = 0;
  
  // Performance monitoring
  let cpuUsage = 0;
  let ramUsage = 0;
  let gpuUsage = 0;
  let gpuAcceleration = false;
  
  // Version info
  const version = "1.0.0-beta";
  
  // Sidebar collapse state [UX]
  let isCollapsed = false;
  
  // System Status Functions [UX][PSF]
  function toggleSection(section: string) {
    switch(section) {
      case 'system': systemExpanded = !systemExpanded; break;
      case 'audio': audioExpanded = !audioExpanded; break;
      case 'provider': providerExpanded = !providerExpanded; break;
      case 'network': networkExpanded = !networkExpanded; break;
    }
  }
  
  function formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }
  

  
  // System Status Updates [UX][PSF]
  function updateSystemMetrics() {
    // Simulate system data - in real implementation, get from Tauri
    cpuUsage = Math.floor(Math.random() * 100);
    ramUsage = Math.floor(Math.random() * 100);
    if (gpuAcceleration) {
      gpuUsage = Math.floor(Math.random() * 100);
    }
    
    // Update audio metrics
    micLevel = Math.floor(Math.random() * 100);
    
    // Update provider metrics
    providerLatency = 200 + Math.floor(Math.random() * 100);
    
    // Update network metrics
    networkLatency = 10 + Math.floor(Math.random() * 50);
    
    // Update session timer
    if (sessionActive) {
      sessionDuration++;
    }
  }
  
  onMount(() => {
    const systemInterval = setInterval(updateSystemMetrics, 1000);
    
    // Demo: GPU acceleration enabled [TSF]
    gpuAcceleration = true;
    
    return () => {
      clearInterval(systemInterval);
    };
  });
</script>

<aside class="sidebar" class:collapsed={isCollapsed}>
  <!-- Sidebar Toggle [UX] -->
  <div class="sidebar-header">
    <button 
      class="collapse-btn" 
      on:click={() => isCollapsed = !isCollapsed}
      title={isCollapsed ? 'Sidebar erweitern' : 'Sidebar einklappen'}
    >
      {isCollapsed ? '▶' : '◀'}
    </button>
    {#if !isCollapsed}
      <span class="sidebar-title">System Status</span>
    {/if}
  </div>
  
  {#if !isCollapsed}
    <!-- System Performance Monitor [ZTS][PSF] -->
    <div class="status-section">
      <div 
        class="status-header" 
        on:click={() => toggleSection('system')}
        role="button"
        tabindex="0"
        on:keydown={(e) => e.key === 'Enter' && toggleSection('system')}
      >
        <span class="status-icon">📊</span>
        <span class="status-text">System Performance</span>
        <span class="expand-icon" class:expanded={systemExpanded}>▼</span>
      </div>
      
      {#if systemExpanded}
        <div class="status-details">
          <!-- CPU Usage -->
          <div class="metric">
            <div class="metric-label">CPU</div>
            <div class="metric-bar">
              <div class="metric-fill cpu" style="width: {cpuUsage}%"></div>
            </div>
            <div class="metric-value">{cpuUsage}%</div>
          </div>
          
          <!-- RAM Usage -->
          <div class="metric">
            <div class="metric-label">RAM</div>
            <div class="metric-bar">
              <div class="metric-fill ram" style="width: {ramUsage}%"></div>
            </div>
            <div class="metric-value">{ramUsage}%</div>
          </div>
          
          <!-- GPU Usage -->
          {#if gpuAcceleration}
            <div class="metric">
              <div class="metric-label">GPU</div>
              <div class="metric-bar">
                <div class="metric-fill gpu" style="width: {gpuUsage}%"></div>
              </div>
              <div class="metric-value">{gpuUsage}%</div>
            </div>
          {/if}
        </div>
      {/if}
    </div>

    <!-- Audio Status Monitor [CT] -->
    <div class="status-section">
      <div 
        class="status-header" 
        on:click={() => toggleSection('audio')}
        role="button"
        tabindex="0"
        on:keydown={(e) => e.key === 'Enter' && toggleSection('audio')}
      >
        <span class="status-icon">🎤</span>
        <span class="status-text">Audio Status</span>
        <span class="expand-icon" class:expanded={audioExpanded}>▼</span>
      </div>
      
      {#if audioExpanded}
        <div class="status-details">
          <!-- Audio Quality -->
          <div class="status-item">
            <span class="status-label">Qualität:</span>
            <span class="status-value good">{audioQuality}</span>
          </div>
          
          <!-- Microphone Level -->
          <div class="metric">
            <div class="metric-label">Mikrofon</div>
            <div class="metric-bar">
              <div class="metric-fill audio" style="width: {micLevel}%"></div>
            </div>
            <div class="metric-value">{micLevel} dB</div>
          </div>
          
          <!-- Noise Reduction -->
          <div class="status-item">
            <span class="status-label">Rauschunterdrückung:</span>
            <span class="status-value {noiseReduction ? 'good' : 'warning'}">
              {noiseReduction ? 'Aktiv' : 'Inaktiv'}
            </span>
          </div>
          
          <!-- Audio Provider -->
          <div class="status-item">
            <span class="status-label">Provider:</span>
            <span class="status-value">{audioProvider}</span>
          </div>
        </div>
      {/if}
    </div>

    <!-- AI Provider Status [PK] -->
    <div class="status-section">
      <div 
        class="status-header" 
        on:click={() => toggleSection('provider')}
        role="button"
        tabindex="0"
        on:keydown={(e) => e.key === 'Enter' && toggleSection('provider')}
      >
        <span class="status-icon">🤖</span>
        <span class="status-text">KI Provider</span>
        <span class="expand-icon" class:expanded={providerExpanded}>▼</span>
      </div>
      
      {#if providerExpanded}
        <div class="status-details">
          <!-- Current Provider -->
          <div class="status-item">
            <span class="status-label">Aktuell:</span>
            <span class="status-value">{currentProvider}</span>
          </div>
          
          <!-- Provider Status -->
          <div class="status-item">
            <span class="status-label">Status:</span>
            <span class="status-value good">{providerStatus}</span>
          </div>
          
          <!-- Latency -->
          <div class="status-item">
            <span class="status-label">Latenz:</span>
            <span class="status-value">{providerLatency}ms</span>
          </div>
          
          <!-- Fallback Available -->
          <div class="status-item">
            <span class="status-label">Fallback:</span>
            <span class="status-value {fallbackAvailable ? 'good' : 'warning'}">
              {fallbackAvailable ? 'Verfügbar' : 'Nicht verfügbar'}
            </span>
          </div>
        </div>
      {/if}
    </div>

    <!-- Network Status [CT] -->
    <div class="status-section">
      <div 
        class="status-header" 
        on:click={() => toggleSection('network')}
        role="button"
        tabindex="0"
        on:keydown={(e) => e.key === 'Enter' && toggleSection('network')}
      >
        <span class="status-icon">🌐</span>
        <span class="status-text">Netzwerk</span>
        <span class="expand-icon" class:expanded={networkExpanded}>▼</span>
      </div>
      
      {#if networkExpanded}
        <div class="status-details">
          <!-- Network Status -->
          <div class="status-item">
            <span class="status-label">Status:</span>
            <span class="status-value good">{networkStatus}</span>
          </div>
          
          <!-- Network Latency -->
          <div class="status-item">
            <span class="status-label">Latenz:</span>
            <span class="status-value">{networkLatency}ms</span>
          </div>
          
          <!-- Cloud Processing -->
          <div class="status-item">
            <span class="status-label">Cloud:</span>
            <span class="status-value {cloudProcessing ? 'warning' : 'good'}">
              {cloudProcessing ? '☁️ Cloud' : '🔒 Lokal'}
            </span>
          </div>
        </div>
      {/if}
    </div>

    <!-- Session Timer [SF] -->
    <div class="status-section">
      <div class="status-header session-timer">
        <span class="status-icon">⏱️</span>
        <span class="status-text">Session</span>
        <span class="timer-display">{formatDuration(sessionDuration)}</span>
      </div>
    </div>
    <!-- Footer with Version [SF] -->
    <div class="sidebar-footer">
      <div class="app-version">v{version}</div>
    </div>
  {/if}
</aside>

<style>
  :root {
    --primary: #3B82F6;
    --success: #10B981;
    --warning: #F59E0B;
    --danger: #EF4444;
    --neutral: #6B7280;
  }
  
  .sidebar {
    width: 280px;
    height: 100vh;
    background: linear-gradient(180deg, #1e293b 0%, #0f172a 100%);
    border-right: 1px solid rgba(255, 255, 255, 0.1);
    display: flex;
    flex-direction: column;
    overflow-y: auto;
    position: relative;
    padding-bottom: 80px;
    transition: width 0.3s ease;
    /* Slim scrollbar [SF] */
    scrollbar-width: thin;
    scrollbar-color: rgba(255, 255, 255, 0.3) transparent;
  }
  
  .sidebar.collapsed {
    width: 60px;
  }
  
  .sidebar::-webkit-scrollbar {
    width: 4px;
  }
  
  .sidebar::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.1);
  }
  
  .sidebar::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.3);
    border-radius: 2px;
  }
  
  /* Sidebar Header [UX] */
  .sidebar-header {
    display: flex;
    align-items: center;
    padding: 1rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    gap: 0.75rem;
  }
  
  .collapse-btn {
    background: rgba(255, 255, 255, 0.1);
    border: 1px solid rgba(255, 255, 255, 0.2);
    color: white;
    padding: 0.5rem;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    transition: all 0.2s ease;
    min-width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  
  .collapse-btn:hover {
    background: rgba(255, 255, 255, 0.2);
    transform: scale(1.05);
  }
  
  .sidebar-title {
    font-size: 1rem;
    font-weight: 600;
    color: white;
  }
  
  /* Status Sections [UX][PSF] */
  .status-section {
    margin: 0.5rem 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  }
  
  .status-header {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    cursor: pointer;
    transition: background-color 0.2s ease;
    gap: 0.75rem;
  }
  
  .status-header:hover {
    background: rgba(255, 255, 255, 0.05);
  }
  
  .status-header.session-timer {
    cursor: default;
    justify-content: space-between;
  }
  
  .status-header.session-timer:hover {
    background: transparent;
  }
  
  .status-icon {
    font-size: 1rem;
    opacity: 0.9;
    min-width: 20px;
  }
  
  .status-text {
    font-size: 0.85rem;
    font-weight: 500;
    color: rgba(255, 255, 255, 0.9);
    flex: 1;
  }
  
  .expand-icon {
    font-size: 0.7rem;
    color: rgba(255, 255, 255, 0.6);
    transition: transform 0.2s ease;
  }
  
  .expand-icon.expanded {
    transform: rotate(180deg);
  }
  
  .status-details {
    padding: 0 1rem 0.75rem 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .status-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.25rem 0;
  }
  
  .status-label {
    font-size: 0.75rem;
    color: rgba(255, 255, 255, 0.7);
  }
  
  .status-value {
    font-size: 0.75rem;
    font-weight: 500;
    color: rgba(255, 255, 255, 0.9);
  }
  
  .status-value.good {
    color: var(--success);
  }
  
  .status-value.warning {
    color: var(--warning);
  }
  
  .status-value.danger {
    color: var(--danger);
  }
  
  .timer-display {
    font-family: 'JetBrains Mono', 'Courier New', monospace;
    font-size: 0.85rem;
    font-weight: 600;
    color: var(--success);
    background: rgba(16, 185, 129, 0.1);
    padding: 0.25rem 0.5rem;
    border-radius: 6px;
    border: 1px solid rgba(16, 185, 129, 0.3);
  }
  
  .sidebar::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 255, 255, 0.5);
  }
  
  /* Navigation Section */
  .nav-section {
    padding: 1rem 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .nav-menu {
    list-style: none;
    margin: 0;
    padding: 0;
  }
  
  .nav-item {
    margin: 0;
  }
  
  .nav-link {
    display: flex;
    align-items: center;
    padding: 0.75rem 1.5rem;
    color: rgba(255, 255, 255, 0.8);
    text-decoration: none;
    transition: all 0.2s ease;
    width: 100%;
  }
  
  .nav-link:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }
  
  .nav-link.active {
    background: rgba(59, 130, 246, 0.2);
    color: white;
    border-right: 3px solid var(--primary);
  }
  
  .nav-icon {
    font-size: 1.25rem;
    margin-right: 0.75rem;
    width: 1.5rem;
    text-align: center;
  }
  
  .nav-text {
    font-size: 0.875rem;
    font-weight: 500;
  }
  
  /* Recording Section [AIU] */
  .recording-section {
    padding: 1.5rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1rem;
  }
  
  .recording-button {
    width: 80px;
    height: 80px;
    border-radius: 50%; /* Rund für Aufnahmebutton */
    border: none;
    color: white;
    font-size: 2rem;
    cursor: pointer;
    transition: background-color 0.2s ease, box-shadow 0.2s ease;
    margin: 0 auto;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
    background: var(--danger); /* Immer rot */
  }
  
  .recording-button:hover {
    background: #dc2626;
    box-shadow: 0 0 15px rgba(239, 68, 68, 0.4);
    transform: scale(1.05);
  }
  
  .recording-controls {
    display: flex;
    gap: 0.75rem;
  }
  
  .control-button {
    width: 60px;
    height: 60px;
    border-radius: 8px;
    border: none;
    color: white;
    font-size: 1.2rem;
    cursor: pointer;
    transition: background-color 0.2s ease, opacity 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
  }
  
  .control-button.pause {
    background: var(--primary); /* Blau für Pause */
  }
  
  .control-button.resume {
    background: var(--danger); /* Rot für Resume (wie Aufnahme) */
  }
  
  .control-button.stop {
    background: var(--success); /* Grün für Stop */
  }
  
  .control-button:hover {
    opacity: 0.9;
  }
  
  /* Microphone Level [CT] */
  .mic-level-container {
    width: 100%;
    text-align: center;
  }
  
  .mic-level-label {
    color: rgba(255, 255, 255, 0.8);
    font-size: 0.75rem;
    margin-bottom: 0.25rem;
  }
  
  .mic-level-bar {
    width: 100%;
    height: 8px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 4px;
    overflow: hidden;
    margin-bottom: 0.25rem;
  }
  
  .mic-level-fill {
    height: 100%;
    background: var(--neutral);
    transition: width 0.1s ease;
  }
  
  .mic-level-fill.recording {
    background: var(--success);
  }
  
  .mic-level-value {
    color: rgba(255, 255, 255, 0.6);
    font-size: 0.75rem;
    font-family: 'JetBrains Mono', monospace;
  }
  
  /* Performance Monitor [ZTS] */
  .performance-section {
    padding: 1rem 1.5rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .performance-item {
    width: 100%;
    padding: 0.75rem 1rem;
    color: rgba(255, 255, 255, 0.8);
    font-size: 0.875rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.75rem;
    transition: background-color 0.2s ease, color 0.2s ease;
    text-align: left;
    border-radius: 0;
    margin: 0;
    margin-left: -1.5rem; /* Kompensiert Section-Padding */
    margin-right: -1rem;
    padding-left: 2rem; /* Eigenes Padding */
    padding-right: 2rem;
  }
  
  .performance-item:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }
  
  .performance-text {
    flex: 1;
    text-align: left;
  }
  
  .expand-icon {
    transition: transform 0.2s ease;
    font-size: 0.75rem;
  }
  
  .expand-icon.expanded {
    transform: rotate(180deg);
  }
  
  .performance-details {
    margin-top: 0.75rem;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .metric {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .metric-label {
    color: rgba(255, 255, 255, 0.8);
    font-size: 0.75rem;
    width: 35px;
    text-align: left;
  }
  
  .metric-bar {
    flex: 1;
    height: 6px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 3px;
    overflow: hidden;
  }
  
  .metric-fill {
    height: 100%;
    transition: width 0.3s ease;
  }
  
  .metric-fill.cpu {
    background: var(--primary);
  }
  
  .metric-fill.ram {
    background: var(--warning);
  }
  
  .metric-fill.gpu {
    background: var(--success);
  }
  
  .metric-value {
    color: rgba(255, 255, 255, 0.6);
    font-size: 0.75rem;
    font-family: 'JetBrains Mono', monospace;
    width: 35px;
    text-align: right;
  }
  
  /* Footer [SF] */
  .sidebar-footer {
    position: fixed;
    bottom: 0;
    left: 0;
    width: 280px; /* Sidebar-Breite */
    padding: 1rem;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
    text-align: center;
    background: linear-gradient(180deg, rgba(15, 23, 42, 0.8) 0%, #0f172a 100%);
    z-index: 10;
    box-sizing: border-box; /* Padding inbegriffen */
  }
  
  .app-version {
    color: rgba(255, 255, 255, 0.9);
    font-size: 0.875rem;
    font-weight: 500;
  }
  
  .version-number {
    font-family: 'JetBrains Mono', monospace;
    color: rgba(255, 255, 255, 0.8);
  }
  
  /* Responsive Design */
  @media (max-width: 768px) {
    .sidebar {
      width: 240px;
    }
    
    .record-button {
      width: 70px;
      height: 70px;
      font-size: 1.75rem;
    }
    
    .record-button.pause,
    .record-button.stop {
      width: 50px;
      height: 50px;
      font-size: 1.25rem;
    }
  }
</style>
