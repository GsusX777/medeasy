<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [ZTS] Zero Tolerance Security - Performance-Monitoring für Sicherheit
  [PSF] Patient Safety First - Performance-Überwachung für Systemstabilität
  [TSF] Tauri 1.5 + Svelte 4 Stack
-->
<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { invoke } from '@tauri-apps/api/tauri';
  
  // Performance metrics
  let cpuUsage = 0;
  let ramUsage = 0;
  let gpuUsage = 0;
  let gpuAcceleration = false;
  
  // Update interval
  let updateInterval: NodeJS.Timeout;
  
  // Props
  export let expanded = false;
  
  // Get performance data from Tauri backend [ZTS]
  async function updatePerformanceMetrics() {
    try {
      // TODO: Implement Tauri commands for performance monitoring
      // For now, simulate data
      cpuUsage = Math.floor(Math.random() * 100);
      ramUsage = Math.floor(Math.random() * 100);
      
      // Check if GPU acceleration is enabled
      if (gpuAcceleration) {
        gpuUsage = Math.floor(Math.random() * 100);
      }
      
      // In real implementation:
      // const metrics = await invoke('get_performance_metrics');
      // cpuUsage = metrics.cpu;
      // ramUsage = metrics.ram;
      // gpuUsage = metrics.gpu;
      // gpuAcceleration = metrics.gpu_enabled;
      
    } catch (error) {
      console.error('Failed to get performance metrics:', error);
    }
  }
  
  // Check GPU acceleration status [ZTS]
  async function checkGpuAcceleration() {
    try {
      // TODO: Implement Tauri command to check GPU acceleration
      // For now, simulate
      gpuAcceleration = Math.random() > 0.5;
      
      // In real implementation:
      // gpuAcceleration = await invoke('is_gpu_acceleration_enabled');
      
    } catch (error) {
      console.error('Failed to check GPU acceleration:', error);
      gpuAcceleration = false;
    }
  }
  
  onMount(async () => {
    // Initial check for GPU acceleration
    await checkGpuAcceleration();
    
    // Update metrics immediately
    await updatePerformanceMetrics();
    
    // Set up interval for regular updates (every 2 seconds)
    updateInterval = setInterval(updatePerformanceMetrics, 2000);
  });
  
  onDestroy(() => {
    if (updateInterval) {
      clearInterval(updateInterval);
    }
  });
  
  // Get color based on usage percentage [PSF]
  function getUsageColor(usage: number): string {
    if (usage < 50) return 'var(--success)';
    if (usage < 80) return 'var(--warning)';
    return 'var(--danger)';
  }
  
  // Get usage status text [SF] - Deutsche Sprache
  function getUsageStatus(usage: number): string {
    if (usage < 50) return 'Normal';
    if (usage < 80) return 'Hoch';
    return 'Kritisch';
  }
</script>

{#if expanded}
  <div class="performance-details">
    <!-- CPU Usage [PSF] -->
    <div class="metric">
      <div class="metric-header">
        <span class="metric-label">CPU</span>
        <span class="metric-status" style="color: {getUsageColor(cpuUsage)}">
          {getUsageStatus(cpuUsage)}
        </span>
      </div>
      <div class="metric-bar">
        <div 
          class="metric-fill" 
          style="width: {cpuUsage}%; background-color: {getUsageColor(cpuUsage)}"
        ></div>
      </div>
      <div class="metric-value">{cpuUsage}%</div>
    </div>
    
    <!-- RAM Usage [PSF] -->
    <div class="metric">
      <div class="metric-header">
        <span class="metric-label">RAM</span>
        <span class="metric-status" style="color: {getUsageColor(ramUsage)}">
          {getUsageStatus(ramUsage)}
        </span>
      </div>
      <div class="metric-bar">
        <div 
          class="metric-fill" 
          style="width: {ramUsage}%; background-color: {getUsageColor(ramUsage)}"
        ></div>
      </div>
      <div class="metric-value">{ramUsage}%</div>
    </div>
    
    <!-- GPU Usage (only if acceleration enabled) [ZTS] -->
    {#if gpuAcceleration}
      <div class="metric">
        <div class="metric-header">
          <span class="metric-label">GPU</span>
          <span class="metric-status" style="color: {getUsageColor(gpuUsage)}">
            {getUsageStatus(gpuUsage)}
          </span>
        </div>
        <div class="metric-bar">
          <div 
            class="metric-fill" 
            style="width: {gpuUsage}%; background-color: {getUsageColor(gpuUsage)}"
          ></div>
        </div>
        <div class="metric-value">{gpuUsage}%</div>
      </div>
    {:else}
      <div class="gpu-disabled">
        <span class="gpu-status">GPU-Beschleunigung deaktiviert</span>
      </div>
    {/if}
    
    <!-- Performance Warning [PSF] -->
    {#if cpuUsage > 80 || ramUsage > 80 || (gpuAcceleration && gpuUsage > 80)}
      <div class="performance-warning">
        <span class="warning-icon">⚠️</span>
        <span class="warning-text">Hohe Systemlast erkannt</span>
      </div>
    {/if}
  </div>
{/if}

<style>
  .performance-details {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    margin-top: 0.5rem;
  }
  
  .metric {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }
  
  .metric-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .metric-label {
    color: rgba(255, 255, 255, 0.8);
    font-size: 0.75rem;
    font-weight: 500;
  }
  
  .metric-status {
    font-size: 0.625rem;
    font-weight: 600;
    text-transform: uppercase;
  }
  
  .metric-bar {
    height: 6px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 3px;
    overflow: hidden;
    position: relative;
  }
  
  .metric-fill {
    height: 100%;
    transition: width 0.3s ease, background-color 0.3s ease;
    border-radius: 3px;
  }
  
  .metric-value {
    color: rgba(255, 255, 255, 0.6);
    font-size: 0.75rem;
    font-family: 'JetBrains Mono', monospace;
    text-align: right;
  }
  
  .gpu-disabled {
    display: flex;
    justify-content: center;
    padding: 0.5rem;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 4px;
  }
  
  .gpu-status {
    color: rgba(255, 255, 255, 0.6);
    font-size: 0.75rem;
    font-style: italic;
  }
  
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
  
  /* Responsive adjustments */
  @media (max-width: 768px) {
    .performance-details {
      gap: 0.5rem;
    }
    
    .metric-header {
      font-size: 0.625rem;
    }
    
    .metric-bar {
      height: 4px;
    }
  }
</style>
