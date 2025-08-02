<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import AdminLayout from '$lib/components/admin/AdminLayout.svelte';
	import { ADMIN_ENDPOINTS, SYSTEM_ENDPOINTS, apiRequest, validateApiResponse } from '$lib/config/api';
	
	// [PSF] Patient safety first - performance monitoring for system reliability
	// [ATV] Audit trail for performance monitoring access
	// [WMM] Whisper Multi-Model performance tracking
	
	interface PerformanceMetrics {
		timestamp: string;
		cpuUsage: number;
		ramUsage: number;
		ramTotal: number;
		diskUsage: number;
		diskTotal: number;
		gpuUsage?: number;
		vramUsage?: number;
		vramTotal?: number;
		activeProcesses: number;
		systemLoad: number;
	}
	
	interface SystemHealth {
		status: 'healthy' | 'warning' | 'critical';
		issues: string[];
		uptime: number;
		lastRestart: string;
	}
	
	let performanceHistory: PerformanceMetrics[] = [];
	let currentMetrics: PerformanceMetrics | null = null;
	let systemHealth: SystemHealth | null = null;
	let liveUpdatesEnabled = false;
	let isMonitoring = false;
	let updateInterval: number;
	let refreshInterval: number;
	let error: string | null = null;
	const UPDATE_INTERVAL_MS = 500; // 0.5 seconds for ultra-responsive monitoring [PSF]
	
	// Mock data for development
	const mockMetrics: PerformanceMetrics = {
		timestamp: new Date().toISOString(),
		cpuUsage: Math.random() * 80 + 10,
		ramUsage: Math.random() * 16 + 4,
		ramTotal: 32,
		diskUsage: Math.random() * 500 + 100,
		diskTotal: 1000,
		gpuUsage: Math.random() * 60 + 20,
		vramUsage: Math.random() * 8 + 2,
		vramTotal: 16,
		activeProcesses: Math.floor(Math.random() * 200 + 150),
		systemLoad: Math.random() * 3 + 0.5
	};
	
	const mockHealth: SystemHealth = {
		status: 'healthy',
		issues: [],
		uptime: 3600000 * 24 * 3, // 3 days
		lastRestart: new Date(Date.now() - 3600000 * 24 * 3).toISOString()
	};
	
	onMount(() => {
		loadCurrentMetrics();
		startMonitoring();
		
		// [ATV] Log performance monitoring access
		console.log('Performance monitoring accessed', {
			timestamp: new Date().toISOString(),
			user: 'admin',
			action: 'performance_monitor_view'
		});
	});
	
	onDestroy(() => {
		stopMonitoring();
	});
	
	async function loadCurrentMetrics() {
		try {
			// Load real performance metrics from backend
			const [performanceResponse, healthResponse] = await Promise.all([
				apiRequest(SYSTEM_ENDPOINTS.PERFORMANCE),
				apiRequest(SYSTEM_ENDPOINTS.STATS)
			]);
			
			const performanceData = await validateApiResponse<any>(performanceResponse);
			const healthData = await validateApiResponse<any>(healthResponse);
			
			// Map backend data to frontend format
			currentMetrics = {
				timestamp: performanceData.timestamp,
				cpuUsage: performanceData.cpuUsage || 0,
				ramUsage: performanceData.ramUsage || 0,
				ramTotal: performanceData.ramTotal || 32,
				diskUsage: performanceData.diskUsage || 0,
				diskTotal: performanceData.diskTotal || 1000,
				gpuUsage: performanceData.gpuUsage,
				vramUsage: performanceData.vramUsage,
				vramTotal: performanceData.vramTotal,
				activeProcesses: performanceData.activeProcesses || 0,
				systemLoad: performanceData.systemLoad || 0
			};
			
			systemHealth = {
				status: healthData.systemHealth || 'unknown',
				issues: [],
				uptime: healthData.uptime || 0,
				lastRestart: healthData.lastBenchmark || new Date().toISOString()
			};
			
			// Add to history
			if (performanceHistory.length >= 60) {
				performanceHistory = performanceHistory.slice(1);
			}
			performanceHistory = [...performanceHistory, currentMetrics];
			
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load performance metrics';
			console.error('Error loading performance metrics:', err);
		}
	}
	
	function startMonitoring() {
		if (isMonitoring) return;
		
		isMonitoring = true;
		refreshInterval = setInterval(loadCurrentMetrics, 5000); // Update every 5 seconds
	}
	
	function stopMonitoring() {
		if (!isMonitoring) return;
		
		isMonitoring = false;
		if (refreshInterval) {
			clearInterval(refreshInterval);
		}
	}
	
	function toggleMonitoring() {
		if (isMonitoring) {
			stopMonitoring();
		} else {
			startMonitoring();
		}
	}
	
	function formatUptime(ms: number): string {
		const days = Math.floor(ms / (1000 * 60 * 60 * 24));
		const hours = Math.floor((ms % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
		const minutes = Math.floor((ms % (1000 * 60 * 60)) / (1000 * 60));
		
		if (days > 0) return `${days}d ${hours}h ${minutes}m`;
		if (hours > 0) return `${hours}h ${minutes}m`;
		return `${minutes}m`;
	}
	
	function formatTimestamp(timestamp: string): string {
		return new Date(timestamp).toLocaleTimeString('de-CH');
	}
	
	function getHealthColor(status: string): string {
		switch (status) {
			case 'healthy': return '#059669';
			case 'warning': return '#d97706';
			case 'critical': return '#dc2626';
			default: return '#6b7280';
		}
	}
	
	function getUsageColor(percentage: number): string {
		if (percentage < 50) return '#059669';
		if (percentage < 80) return '#d97706';
		return '#dc2626';
	}
	
	$: cpuPercentage = currentMetrics ? currentMetrics.cpuUsage : 0;
	$: ramPercentage = currentMetrics ? (currentMetrics.ramUsage / currentMetrics.ramTotal) * 100 : 0;
	$: diskPercentage = currentMetrics ? (currentMetrics.diskUsage / currentMetrics.diskTotal) * 100 : 0;
	$: gpuPercentage = currentMetrics?.gpuUsage || 0;
	$: vramPercentage = currentMetrics && currentMetrics.vramTotal ? (currentMetrics.vramUsage! / currentMetrics.vramTotal) * 100 : 0;
</script>

<AdminLayout>
	<div class="performance-monitor">
		<!-- Header -->
		<div class="monitor-header">
			<div class="header-info">
				<h2>📊 Performance Monitor</h2>
				<p class="header-description">
					Echtzeit-Überwachung der System-Performance für optimale MedEasy-Leistung
				</p>
			</div>
			
			<div class="header-controls">
				<button 
					class="monitor-toggle"
					class:active={isMonitoring}
					on:click={toggleMonitoring}
				>
					{isMonitoring ? '⏸️ Pausieren' : '▶️ Starten'}
				</button>
				
				<button class="refresh-btn" on:click={loadCurrentMetrics}>
					🔄 Aktualisieren
				</button>
			</div>
		</div>
		
		<!-- System Health Overview -->
		{#if systemHealth}
			<div class="health-overview">
				<div class="health-status" style="border-left-color: {getHealthColor(systemHealth.status)}">
					<div class="health-indicator">
						<span class="health-icon">
							{systemHealth.status === 'healthy' ? '✅' : systemHealth.status === 'warning' ? '⚠️' : '❌'}
						</span>
						<span class="health-text" style="color: {getHealthColor(systemHealth.status)}">
							{systemHealth.status === 'healthy' ? 'System Gesund' : 
							 systemHealth.status === 'warning' ? 'Warnung' : 'Kritisch'}
						</span>
					</div>
					
					<div class="health-details">
						<span class="uptime">⏱️ Uptime: {formatUptime(systemHealth.uptime)}</span>
						<span class="last-restart">
							🔄 Letzter Neustart: {formatTimestamp(systemHealth.lastRestart)}
						</span>
					</div>
				</div>
				
				{#if systemHealth.issues.length > 0}
					<div class="health-issues">
						<h4>⚠️ Aktuelle Probleme:</h4>
						<ul>
							{#each systemHealth.issues as issue}
								<li>{issue}</li>
							{/each}
						</ul>
					</div>
				{/if}
			</div>
		{/if}
		
		<!-- Current Metrics -->
		{#if currentMetrics}
			<div class="metrics-grid">
				<!-- CPU Usage -->
				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-icon">🖥️</span>
						<span class="metric-title">CPU-Auslastung</span>
					</div>
					<div class="metric-value" style="color: {getUsageColor(cpuPercentage)}">
						{cpuPercentage.toFixed(1)}%
					</div>
					<div class="metric-bar">
						<div 
							class="metric-fill" 
							style="width: {cpuPercentage}%; background-color: {getUsageColor(cpuPercentage)}"
						></div>
					</div>
					<div class="metric-details">
						System Load: {currentMetrics.systemLoad.toFixed(2)}
					</div>
				</div>
				
				<!-- RAM Usage -->
				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-icon">💾</span>
						<span class="metric-title">RAM-Verbrauch</span>
					</div>
					<div class="metric-value" style="color: {getUsageColor(ramPercentage)}">
						{currentMetrics.ramUsage.toFixed(1)} GB
					</div>
					<div class="metric-bar">
						<div 
							class="metric-fill" 
							style="width: {ramPercentage}%; background-color: {getUsageColor(ramPercentage)}"
						></div>
					</div>
					<div class="metric-details">
						{ramPercentage.toFixed(1)}% von {currentMetrics.ramTotal} GB
					</div>
				</div>
				
				<!-- Disk Usage -->
				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-icon">💿</span>
						<span class="metric-title">Festplatte</span>
					</div>
					<div class="metric-value" style="color: {getUsageColor(diskPercentage)}">
						{currentMetrics.diskUsage.toFixed(0)} GB
					</div>
					<div class="metric-bar">
						<div 
							class="metric-fill" 
							style="width: {diskPercentage}%; background-color: {getUsageColor(diskPercentage)}"
						></div>
					</div>
					<div class="metric-details">
						{diskPercentage.toFixed(1)}% von {currentMetrics.diskTotal} GB
					</div>
				</div>
				
				<!-- GPU Usage (if available) -->
				{#if currentMetrics.gpuUsage !== undefined}
					<div class="metric-card">
						<div class="metric-header">
							<span class="metric-icon">🎮</span>
							<span class="metric-title">GPU-Auslastung</span>
						</div>
						<div class="metric-value" style="color: {getUsageColor(gpuPercentage)}">
							{gpuPercentage.toFixed(1)}%
						</div>
						<div class="metric-bar">
							<div 
								class="metric-fill" 
								style="width: {gpuPercentage}%; background-color: {getUsageColor(gpuPercentage)}"
							></div>
						</div>
						<div class="metric-details">
							VRAM: {currentMetrics.vramUsage?.toFixed(1)} / {currentMetrics.vramTotal} GB
						</div>
					</div>
				{/if}
				
				<!-- Active Processes -->
				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-icon">⚙️</span>
						<span class="metric-title">Aktive Prozesse</span>
					</div>
					<div class="metric-value">
						{currentMetrics.activeProcesses}
					</div>
					<div class="metric-details">
						Laufende System-Prozesse
					</div>
				</div>
				
				<!-- Last Update -->
				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-icon">🕐</span>
						<span class="metric-title">Letzte Aktualisierung</span>
					</div>
					<div class="metric-value">
						{formatTimestamp(currentMetrics.timestamp)}
					</div>
					<div class="metric-details">
						{isMonitoring ? 'Live-Monitoring aktiv' : 'Monitoring pausiert'}
					</div>
				</div>
			</div>
		{/if}
		
		<!-- Performance History Chart -->
		{#if performanceHistory.length > 0}
			<div class="performance-chart">
				<h3>📈 Performance-Verlauf (letzte 5 Minuten)</h3>
				<div class="chart-container">
					<div class="chart-legend">
						<span class="legend-item cpu">🖥️ CPU</span>
						<span class="legend-item ram">💾 RAM</span>
						<span class="legend-item disk">💿 Disk</span>
						{#if currentMetrics?.gpuUsage !== undefined}
							<span class="legend-item gpu">🎮 GPU</span>
						{/if}
					</div>
					
					<div class="simple-chart">
						{#each performanceHistory as metrics, index}
							<div class="chart-point" style="left: {(index / (performanceHistory.length - 1)) * 100}%">
								<div class="point-tooltip">
									<div>Zeit: {formatTimestamp(metrics.timestamp)}</div>
									<div>CPU: {metrics.cpuUsage.toFixed(1)}%</div>
									<div>RAM: {((metrics.ramUsage / metrics.ramTotal) * 100).toFixed(1)}%</div>
									<div>Disk: {((metrics.diskUsage / metrics.diskTotal) * 100).toFixed(1)}%</div>
									{#if metrics.gpuUsage !== undefined}
										<div>GPU: {metrics.gpuUsage.toFixed(1)}%</div>
									{/if}
								</div>
							</div>
						{/each}
					</div>
				</div>
			</div>
		{/if}
		
		<!-- Error Display -->
		{#if error}
			<div class="error-display">
				<span class="error-icon">❌</span>
				<span class="error-message">{error}</span>
				<button class="error-retry" on:click={() => { error = null; loadCurrentMetrics(); }}>
					Erneut versuchen
				</button>
			</div>
		{/if}
	</div>
</AdminLayout>

<style>
	.performance-monitor {
		display: flex;
		flex-direction: column;
		gap: 2rem;
		padding: 1rem;
	}
	
	.monitor-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		gap: 1rem;
		flex-wrap: wrap;
	}
	
	.header-info h2 {
		margin-bottom: 0.5rem;
		color: var(--text-primary, #111827);
	}
	
	.header-description {
		color: var(--text-secondary, #6b7280);
		margin: 0;
	}
	
	.header-controls {
		display: flex;
		gap: 0.5rem;
		align-items: center;
	}
	
	.monitor-toggle,
	.refresh-btn {
		padding: 0.5rem 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.875rem;
	}
	
	.monitor-toggle.active {
		background: var(--primary-color, #2563eb);
		color: white;
		border-color: var(--primary-color, #2563eb);
	}
	
	.monitor-toggle:hover,
	.refresh-btn:hover {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.health-overview {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		padding: 1.5rem;
	}
	
	.health-status {
		display: flex;
		justify-content: space-between;
		align-items: center;
		border-left: 4px solid;
		padding-left: 1rem;
		margin-bottom: 1rem;
	}
	
	.health-indicator {
		display: flex;
		align-items: center;
		gap: 0.5rem;
	}
	
	.health-icon {
		font-size: 1.25rem;
	}
	
	.health-text {
		font-weight: 600;
		font-size: 1.125rem;
	}
	
	.health-details {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.health-issues {
		background: #fef2f2;
		border: 1px solid #fecaca;
		border-radius: 6px;
		padding: 1rem;
	}
	
	.health-issues h4 {
		margin-bottom: 0.5rem;
		color: #dc2626;
	}
	
	.health-issues ul {
		margin: 0;
		padding-left: 1.5rem;
		color: #dc2626;
	}
	
	.metrics-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: 1rem;
	}
	
	.metric-card {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		padding: 1.5rem;
	}
	
	.metric-header {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		margin-bottom: 1rem;
	}
	
	.metric-icon {
		font-size: 1.25rem;
	}
	
	.metric-title {
		font-weight: 500;
		color: var(--text-primary, #111827);
	}
	
	.metric-value {
		font-size: 2rem;
		font-weight: 700;
		margin-bottom: 0.75rem;
	}
	
	.metric-bar {
		width: 100%;
		height: 8px;
		background: var(--bg-secondary, #f8fafc);
		border-radius: 4px;
		overflow: hidden;
		margin-bottom: 0.5rem;
	}
	
	.metric-fill {
		height: 100%;
		transition: width 0.3s ease;
		border-radius: 4px;
	}
	
	.metric-details {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.performance-chart {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		padding: 1.5rem;
	}
	
	.performance-chart h3 {
		margin-bottom: 1rem;
		color: var(--text-primary, #111827);
	}
	
	.chart-legend {
		display: flex;
		gap: 1rem;
		margin-bottom: 1rem;
		flex-wrap: wrap;
	}
	
	.legend-item {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.simple-chart {
		position: relative;
		height: 100px;
		background: var(--bg-secondary, #f8fafc);
		border-radius: 4px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.chart-point {
		position: absolute;
		top: 50%;
		transform: translateY(-50%);
		width: 8px;
		height: 8px;
		background: var(--primary-color, #2563eb);
		border-radius: 50%;
		cursor: pointer;
	}
	
	.point-tooltip {
		position: absolute;
		bottom: 100%;
		left: 50%;
		transform: translateX(-50%);
		background: rgba(0, 0, 0, 0.8);
		color: white;
		padding: 0.5rem;
		border-radius: 4px;
		font-size: 0.75rem;
		white-space: nowrap;
		opacity: 0;
		pointer-events: none;
		transition: opacity 0.2s ease;
		z-index: 10;
	}
	
	.chart-point:hover .point-tooltip {
		opacity: 1;
	}
	
	.error-display {
		display: flex;
		align-items: center;
		gap: 1rem;
		padding: 1rem;
		background: #fef2f2;
		border: 1px solid #fecaca;
		border-radius: 8px;
		color: #dc2626;
	}
	
	.error-icon {
		font-size: 1.25rem;
	}
	
	.error-message {
		flex: 1;
		font-weight: 500;
	}
	
	.error-retry {
		padding: 0.5rem 1rem;
		border: 1px solid #dc2626;
		border-radius: 4px;
		background: white;
		color: #dc2626;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.error-retry:hover {
		background: #dc2626;
		color: white;
	}
	
	@media (max-width: 768px) {
		.monitor-header {
			flex-direction: column;
			align-items: stretch;
		}
		
		.header-controls {
			justify-content: center;
		}
		
		.health-status {
			flex-direction: column;
			align-items: flex-start;
			gap: 1rem;
		}
		
		.metrics-grid {
			grid-template-columns: 1fr;
		}
		
		.chart-legend {
			justify-content: center;
		}
	}
</style>
