<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  
  // [PSF] Live Performance Monitoring für Python AI Service
  // [WMM] Zeigt Whisper-Processing-Impact in Echtzeit
  
  interface PythonPerformanceData {
    timestamp: number;
    process_id: number;
    cpu_usage_percent: number;
    memory_usage_mb: number;
    total_memory_mb: number;
    memory_percent: number;
    gpu_usage_percent: number;
    gpu_memory_mb: number;
    cpu_cores: number;
    status: string;
    service: string;
  }
  
  // Reactive Variables
  let cpuUsage = 0;
  let memoryUsageMb = 0;
  let gpuUsage = 0;
  let lastUpdate = '';
  let isActive = true;
  let errorMessage = '';
  let intervalId: number | null = null;
  
  // Performance History für Charts
  let performanceHistory: Array<{
    timestamp: number;
    cpu: number;
    memory: number;
    gpu: number;
  }> = [];
  
  const MAX_HISTORY_POINTS = 60; // 1 Minute bei 1s Intervall
  
  // Live Performance Update [PSF][WMM]
  async function updatePerformanceMetrics() {
    if (!isActive) return;
    
    try {
      const response = await fetch('http://localhost:8000/performance');
      
      if (!response.ok) {
        throw new Error(`API Error: ${response.status} ${response.statusText}`);
      }
      
      const data: PythonPerformanceData = await response.json();
      
      // Update current values
      cpuUsage = Math.round(data.cpu_usage_percent * 10) / 10;
      memoryUsageMb = Math.round(data.memory_usage_mb * 10) / 10;
      gpuUsage = Math.round(data.gpu_usage_percent * 10) / 10;
      lastUpdate = new Date(data.timestamp * 1000).toLocaleTimeString('de-CH');
      
      // Add to history
      performanceHistory = [
        ...performanceHistory.slice(-MAX_HISTORY_POINTS + 1),
        {
          timestamp: data.timestamp,
          cpu: cpuUsage,
          memory: memoryUsageMb,
          gpu: gpuUsage
        }
      ];
      
      errorMessage = '';
    } catch (error) {
      console.error('Performance monitoring error:', error);
      errorMessage = error instanceof Error ? error.message : 'Unknown error';
    }
  }
  
  // Start/Stop Monitoring [PSF]
  export function startMonitoring() {
    if (intervalId) return;
    
    isActive = true;
    updatePerformanceMetrics(); // Initial update
    intervalId = setInterval(updatePerformanceMetrics, 1000); // Every 1 second
    console.log('🚀 Live Performance Monitor started');
  }
  
  export function stopMonitoring() {
    isActive = false;
    if (intervalId) {
      clearInterval(intervalId);
      intervalId = null;
    }
    console.log('⏹️ Live Performance Monitor stopped');
  }
  
  // Lifecycle
  onMount(() => {
    startMonitoring();
  });
  
  onDestroy(() => {
    stopMonitoring();
  });
  
  // Helper Functions
  function getCpuClass(usage: number): string {
    if (usage > 80) return 'critical';
    if (usage > 60) return 'warning';
    return 'normal';
  }
  
  function getMemoryClass(usage: number): string {
    if (usage > 1000) return 'critical'; // > 1GB
    if (usage > 500) return 'warning';   // > 500MB
    return 'normal';
  }
</script>

<div class="performance-monitor">
  <div class="monitor-header">
    <h3>🔥 Live Performance (Python AI)</h3>
    <div class="status-indicator" class:active={isActive} class:error={!!errorMessage}>
      {isActive ? '🟢 Live' : '🔴 Stopped'}
    </div>
  </div>
  
  {#if errorMessage}
    <div class="error-message">
      ⚠️ {errorMessage}
    </div>
  {/if}
  
  <div class="metrics-grid">
    <!-- CPU Usage -->
    <div class="metric-card">
      <div class="metric-label">CPU</div>
      <div class="metric-value {getCpuClass(cpuUsage)}">{cpuUsage}%</div>
      <div class="metric-bar">
        <div class="metric-fill cpu" style="width: {Math.min(100, cpuUsage)}%"></div>
      </div>
    </div>
    
    <!-- Memory Usage -->
    <div class="metric-card">
      <div class="metric-label">RAM</div>
      <div class="metric-value {getMemoryClass(memoryUsageMb)}">{memoryUsageMb.toFixed(1)} MB</div>
      <div class="metric-bar">
        <div class="metric-fill memory" style="width: {Math.min(100, (memoryUsageMb / 1024) * 100)}%"></div>
      </div>
    </div>
    
    <!-- GPU Usage -->
    <div class="metric-card">
      <div class="metric-label">GPU</div>
      <div class="metric-value">{gpuUsage}%</div>
      <div class="metric-bar">
        <div class="metric-fill gpu" style="width: {Math.min(100, gpuUsage)}%"></div>
      </div>
    </div>
  </div>
  
  <!-- Performance History Chart (Simple) -->
  {#if performanceHistory.length > 5}
    <div class="history-chart">
      <div class="chart-header">Performance History (Last {performanceHistory.length}s)</div>
      <div class="chart-container">
        <svg width="100%" height="60" viewBox="0 0 300 60">
          {#each performanceHistory as point, i}
            {#if i > 0}
              <line
                x1={((i - 1) / (performanceHistory.length - 1)) * 300}
                y1={60 - (performanceHistory[i - 1].cpu / 100) * 60}
                x2={(i / (performanceHistory.length - 1)) * 300}
                y2={60 - (point.cpu / 100) * 60}
                stroke="#ef4444"
                stroke-width="2"
              />
            {/if}
          {/each}
        </svg>
      </div>
    </div>
  {/if}
  
  <div class="last-update">
    Last Update: {lastUpdate}
  </div>
</div>

<style>
  .performance-monitor {
    background: #1f2937;
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 16px;
    border: 1px solid #374151;
  }
  
  .monitor-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }
  
  .monitor-header h3 {
    margin: 0;
    color: #f3f4f6;
    font-size: 16px;
    font-weight: 600;
  }
  
  .status-indicator {
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 500;
  }
  
  .status-indicator.active {
    background: #065f46;
    color: #10b981;
  }
  
  .status-indicator.error {
    background: #7f1d1d;
    color: #ef4444;
  }
  
  .error-message {
    background: #7f1d1d;
    color: #fca5a5;
    padding: 8px 12px;
    border-radius: 4px;
    margin-bottom: 12px;
    font-size: 14px;
  }
  
  .metrics-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 12px;
    margin-bottom: 16px;
  }
  
  .metric-card {
    background: #111827;
    border-radius: 6px;
    padding: 12px;
    border: 1px solid #374151;
  }
  
  .metric-label {
    font-size: 12px;
    color: #9ca3af;
    margin-bottom: 4px;
    font-weight: 500;
  }
  
  .metric-value {
    font-size: 18px;
    font-weight: 700;
    margin-bottom: 8px;
  }
  
  .metric-value.normal {
    color: #10b981;
  }
  
  .metric-value.warning {
    color: #f59e0b;
  }
  
  .metric-value.critical {
    color: #ef4444;
  }
  
  .metric-bar {
    height: 4px;
    background: #374151;
    border-radius: 2px;
    overflow: hidden;
  }
  
  .metric-fill {
    height: 100%;
    transition: width 0.3s ease;
    border-radius: 2px;
  }
  
  .metric-fill.cpu {
    background: linear-gradient(90deg, #10b981, #ef4444);
  }
  
  .metric-fill.memory {
    background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  }
  
  .metric-fill.gpu {
    background: linear-gradient(90deg, #f59e0b, #ef4444);
  }
  
  .history-chart {
    background: #111827;
    border-radius: 6px;
    padding: 12px;
    border: 1px solid #374151;
    margin-bottom: 12px;
  }
  
  .chart-header {
    font-size: 12px;
    color: #9ca3af;
    margin-bottom: 8px;
    font-weight: 500;
  }
  
  .chart-container {
    width: 100%;
    height: 60px;
  }
  
  .last-update {
    text-align: center;
    font-size: 11px;
    color: #6b7280;
    margin-top: 8px;
  }
  
  @media (max-width: 768px) {
    .metrics-grid {
      grid-template-columns: 1fr;
    }
  }
</style>
