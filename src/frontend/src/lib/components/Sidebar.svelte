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
  
  // Recording state
  let isRecording = false;
  let isPaused = false;
  let micLevel = 0;
  
  // Performance monitoring
  let performanceExpanded = false;
  let cpuUsage = 0;
  let ramUsage = 0;
  let gpuUsage = 0;
  let gpuAcceleration = false;
  
  // Version info
  const version = "1.0.0-beta";
  
  // Navigation items [SF] - Deutsche Sprache
  const navItems = [
    { id: 'dashboard', label: 'Dashboard', icon: '📊', href: '/' },
    { id: 'patients', label: 'Patienten', href: '/patients', icon: '👥' },
    { id: 'consultations', label: 'Konsultationen', href: '/consultations', icon: '🩺' },
    { id: 'settings', label: 'Einstellungen', href: '/settings', icon: '⚙️' }
  ];
  
  let activeItem = 'dashboard';
  
  // Recording functions [AIU] - Anonymisierung immer aktiv
  function startRecording() {
    if (!isRecording) {
      isRecording = true;
      isPaused = false;
      // TODO: Integrate with SessionRecorder component
      console.log('Recording started with anonymization enabled');
    }
  }
  
  function pauseRecording() {
    if (isRecording && !isPaused) {
      isPaused = true;
      // TODO: Pause recording logic
      console.log('Recording paused');
    }
  }
  
  function resumeRecording() {
    if (isRecording && isPaused) {
      isPaused = false;
      // TODO: Resume recording logic
      console.log('Recording resumed');
    }
  }
  
  function stopRecording() {
    if (isRecording) {
      isRecording = false;
      isPaused = false;
      console.log('Recording stopped');
    }
  }
  

  
  // Microphone level simulation [CT]
  function updateMicLevel() {
    if (isRecording && !isPaused) {
      micLevel = Math.floor(Math.random() * 100);
    } else {
      micLevel = 0;
    }
  }
  
  // Performance monitoring [ZTS]
  function updatePerformanceMetrics() {
    // Simulate performance data - in real implementation, get from Tauri
    cpuUsage = Math.floor(Math.random() * 100);
    ramUsage = Math.floor(Math.random() * 100);
    if (gpuAcceleration) {
      gpuUsage = Math.floor(Math.random() * 100);
    }
  }
  
  // Keyboard shortcuts [PSF]
  function handleKeydown(event: KeyboardEvent) {
    // Space bar for recording control
    if (event.code === 'Space' && !event.repeat) {
      // Prevent default only if not in input field
      const target = event.target as HTMLElement;
      if (!target.matches('input, textarea, [contenteditable]')) {
        event.preventDefault();
        
        if (!isRecording) {
          startRecording();
        } else if (isPaused) {
          resumeRecording();
        } else {
          pauseRecording();
        }
      }
    }
  }
  
  onMount(() => {
    const micInterval = setInterval(updateMicLevel, 100);
    const performanceInterval = setInterval(updatePerformanceMetrics, 2000);
    
    // Demo: GPU acceleration enabled [TSF]
    gpuAcceleration = true;
    
    // Add global keyboard listener [PSF]
    window.addEventListener('keydown', handleKeydown);
    
    return () => {
      clearInterval(micInterval);
      clearInterval(performanceInterval);
      window.removeEventListener('keydown', handleKeydown);
    };
  });
</script>

<aside class="sidebar">
  <!-- Navigation Menu [SF] -->
  <nav class="nav-section">
    <ul class="nav-menu">
      {#each navItems as item}
        <li class="nav-item">
          <a 
            href={item.href} 
            class="nav-link"
            class:active={activeItem === item.id}
            on:click={() => activeItem = item.id}
          >
            <span class="nav-icon">{item.icon}</span>
            <span class="nav-text">{item.label}</span>
          </a>
        </li>
      {/each}
    </ul>
  </nav>
  
  <!-- Recording Controls [AIU] -->
  <div class="recording-section">
    {#if !isRecording}
      <!-- Start Recording Button -->
      <button 
        class="recording-button start" 
        on:click={startRecording}
      >
        ●
      </button>
    {:else}
      <!-- Recording Active - Show Pause/Stop -->
      <div class="recording-controls">
        {#if isPaused}
          <button 
            class="control-button resume" 
            on:click={resumeRecording}
          >
            ●
          </button>
        {:else}
          <button 
            class="control-button pause" 
            on:click={pauseRecording}
          >
            ❚❚
          </button>
        {/if}
        <button 
          class="control-button stop" 
          on:click={stopRecording}
        >
          ■
        </button>
      </div>
    {/if}
    
    <!-- Microphone Level Display [CT] -->
    <div class="mic-level-container">
      <div class="mic-level-label">Mikrofon</div>
      <div class="mic-level-bar">
        <div 
          class="mic-level-fill" 
          style="width: {micLevel}%"
          class:recording={isRecording && !isPaused}
        ></div>
      </div>
      <div class="mic-level-value">{micLevel} dB</div>
    </div>
  </div>
  
  <!-- Performance Monitor [ZTS] -->
  <div class="performance-section">
    <div 
      class="menu-item performance-item" 
      on:click={() => performanceExpanded = !performanceExpanded}
      role="button"
      tabindex="0"
      on:keydown={(e) => e.key === 'Enter' && (performanceExpanded = !performanceExpanded)}
    >
      <span class="menu-icon">📊</span>
      <span class="menu-text">Performance</span>
      <span class="expand-icon" class:expanded={performanceExpanded}>▼</span>
    </div>
    
    {#if performanceExpanded}
      <div class="performance-details">
        <!-- CPU Usage -->
        <div class="metric">
          <div class="metric-label">CPU</div>
          <div class="metric-bar">
            <div 
              class="metric-fill cpu" 
              style="width: {cpuUsage}%"
            ></div>
          </div>
          <div class="metric-value">{cpuUsage}%</div>
        </div>
        
        <!-- RAM Usage -->
        <div class="metric">
          <div class="metric-label">RAM</div>
          <div class="metric-bar">
            <div 
              class="metric-fill ram" 
              style="width: {ramUsage}%"
            ></div>
          </div>
          <div class="metric-value">{ramUsage}%</div>
        </div>
        
        <!-- GPU Usage (only if acceleration enabled) -->
        {#if gpuAcceleration}
          <div class="metric">
            <div class="metric-label">GPU</div>
            <div class="metric-bar">
              <div 
                class="metric-fill gpu" 
                style="width: {gpuUsage}%"
              ></div>
            </div>
            <div class="metric-value">{gpuUsage}%</div>
          </div>
        {/if}
      </div>
    {/if}
  </div>
  
  <!-- Footer with Version [SF] -->
  <div class="sidebar-footer">
    <div class="app-version">v{version}</div>
  </div>
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
    position: relative; /* Für absoluten Footer */
    padding-bottom: 80px; /* Platz für Footer */
    /* Slim scrollbar [SF] */
    scrollbar-width: thin;
    scrollbar-color: rgba(255, 255, 255, 0.3) transparent;
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
