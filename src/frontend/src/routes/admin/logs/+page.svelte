<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import AdminLayout from '$lib/components/admin/AdminLayout.svelte';
	import LogViewer from '$lib/components/admin/LogViewer.svelte';
	import LogFilters from '$lib/components/admin/LogFilters.svelte';
	
	// [ATV] Audit trail logging viewer
	// [PSF] Patient safety monitoring through logs
	// [DSC] Swiss compliance log tracking
	
	interface LogEntry {
		id: string;
		timestamp: string;
		level: 'info' | 'warn' | 'error' | 'debug';
		service: 'backend' | 'ai-service' | 'frontend';
		message: string;
		requestId?: string;
		userId?: string;
		details?: any;
	}
	
	interface LogFilters {
		level: string[];
		service: string[];
		timeRange: string;
		searchTerm: string;
	}
	
	let logs: LogEntry[] = [];
	let filteredLogs: LogEntry[] = [];
	let loading = true;
	let error: string | null = null;
	let autoRefresh = true;
	let refreshInterval: number;
	
	let filters: LogFilters = {
		level: ['info', 'warn', 'error'],
		service: ['backend', 'ai-service', 'frontend'],
		timeRange: '1h',
		searchTerm: ''
	};
	
	onMount(async () => {
		await loadLogs();
		
		if (autoRefresh) {
			refreshInterval = setInterval(loadLogs, 5000); // Refresh every 5 seconds
		}
	});
	
	onDestroy(() => {
		if (refreshInterval) {
			clearInterval(refreshInterval);
		}
	});
	
	async function loadLogs() {
		try {
			loading = true;
			error = null;
			
			// TODO: Replace with actual API call
			// const response = await fetch('/api/admin/logs');
			// logs = await response.json();
			
			// Mock data for demonstration
			logs = generateMockLogs();
			applyFilters();
		} catch (err) {
			error = 'Failed to load logs: ' + (err as Error).message;
			console.error('Log loading error:', err);
		} finally {
			loading = false;
		}
	}
	
	function generateMockLogs(): LogEntry[] {
		const services: Array<'backend' | 'ai-service' | 'frontend'> = ['backend', 'ai-service', 'frontend'];
		const levels: Array<'info' | 'warn' | 'error' | 'debug'> = ['info', 'warn', 'error', 'debug'];
		const mockLogs: LogEntry[] = [];
		
		for (let i = 0; i < 50; i++) {
			const timestamp = new Date(Date.now() - Math.random() * 3600000).toISOString();
			const service = services[Math.floor(Math.random() * services.length)];
			const level = levels[Math.floor(Math.random() * levels.length)];
			
			let message = '';
			switch (service) {
				case 'backend':
					message = level === 'error' 
						? 'Failed to process transcription request'
						: level === 'warn'
						? 'High memory usage detected'
						: 'Transcription request processed successfully';
					break;
				case 'ai-service':
					message = level === 'error'
						? 'Whisper model loading failed'
						: level === 'warn'
						? 'Model switching took longer than expected'
						: 'Audio transcription completed';
					break;
				case 'frontend':
					message = level === 'error'
						? 'Failed to upload audio file'
						: level === 'warn'
						? 'Slow network connection detected'
						: 'User session started';
					break;
			}
			
			mockLogs.push({
				id: `log-${i}`,
				timestamp,
				level,
				service,
				message,
				requestId: `req-${Math.random().toString(36).substr(2, 9)}`,
				userId: 'admin',
				details: {
					duration: Math.random() * 1000,
					memoryUsage: Math.random() * 500
				}
			});
		}
		
		return mockLogs.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
	}
	
	function applyFilters() {
		filteredLogs = logs.filter(log => {
			// Level filter
			if (!filters.level.includes(log.level)) return false;
			
			// Service filter
			if (!filters.service.includes(log.service)) return false;
			
			// Time range filter
			const logTime = new Date(log.timestamp).getTime();
			const now = Date.now();
			const timeRangeMs = getTimeRangeMs(filters.timeRange);
			if (now - logTime > timeRangeMs) return false;
			
			// Search term filter
			if (filters.searchTerm) {
				const searchLower = filters.searchTerm.toLowerCase();
				return log.message.toLowerCase().includes(searchLower) ||
					   log.requestId?.toLowerCase().includes(searchLower) ||
					   log.service.toLowerCase().includes(searchLower);
			}
			
			return true;
		});
	}
	
	function getTimeRangeMs(range: string): number {
		switch (range) {
			case '15m': return 15 * 60 * 1000;
			case '1h': return 60 * 60 * 1000;
			case '6h': return 6 * 60 * 60 * 1000;
			case '24h': return 24 * 60 * 60 * 1000;
			case '7d': return 7 * 24 * 60 * 60 * 1000;
			default: return 60 * 60 * 1000;
		}
	}
	
	function onFiltersChange(newFilters: LogFilters) {
		filters = newFilters;
		applyFilters();
	}
	
	function toggleAutoRefresh() {
		autoRefresh = !autoRefresh;
		
		if (autoRefresh) {
			refreshInterval = setInterval(loadLogs, 5000);
		} else if (refreshInterval) {
			clearInterval(refreshInterval);
		}
	}
	
	function exportLogs() {
		const csvContent = "data:text/csv;charset=utf-8," 
			+ "Timestamp,Level,Service,Message,RequestID\n"
			+ filteredLogs.map(log => 
				`"${log.timestamp}","${log.level}","${log.service}","${log.message}","${log.requestId || ''}"`
			).join("\n");
		
		const encodedUri = encodeURI(csvContent);
		const link = document.createElement("a");
		link.setAttribute("href", encodedUri);
		link.setAttribute("download", `medeasy-logs-${new Date().toISOString().split('T')[0]}.csv`);
		document.body.appendChild(link);
		link.click();
		document.body.removeChild(link);
	}
</script>

<AdminLayout>
	<div class="logs-page">
		<header class="page-header">
			<div class="header-content">
				<h1>📋 System Logs</h1>
				<p class="subtitle">
					Alle Logs aus Backend, AI-Service und Frontend
				</p>
			</div>
			
			<div class="header-actions">
				<button 
					class="refresh-btn"
					class:auto={autoRefresh}
					on:click={toggleAutoRefresh}
				>
					{autoRefresh ? '⏸️ Auto-Refresh' : '▶️ Auto-Refresh'}
				</button>
				
				<button 
					class="export-btn"
					on:click={exportLogs}
				>
					📥 Export CSV
				</button>
				
				<button 
					class="manual-refresh-btn"
					on:click={loadLogs}
					disabled={loading}
				>
					🔄 Aktualisieren
				</button>
			</div>
		</header>
		
		<LogFilters 
			{filters}
			on:filtersChange={(e) => onFiltersChange(e.detail)}
		/>
		
		<div class="logs-stats">
			<div class="stat">
				<span class="stat-label">Gesamt:</span>
				<span class="stat-value">{logs.length}</span>
			</div>
			<div class="stat">
				<span class="stat-label">Gefiltert:</span>
				<span class="stat-value">{filteredLogs.length}</span>
			</div>
			<div class="stat">
				<span class="stat-label">Letzte Aktualisierung:</span>
				<span class="stat-value">{new Date().toLocaleTimeString('de-CH')}</span>
			</div>
		</div>
		
		{#if error}
			<div class="error-message">
				❌ {error}
			</div>
		{/if}
		
		<LogViewer 
			logs={filteredLogs}
			{loading}
		/>
	</div>
</AdminLayout>

<style>
	.logs-page {
		padding: 2rem;
		max-width: 1400px;
		margin: 0 auto;
	}
	
	.page-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		margin-bottom: 2rem;
		flex-wrap: wrap;
		gap: 1rem;
	}
	
	.header-content h1 {
		font-size: 2rem;
		color: var(--text-primary, #111827);
		margin-bottom: 0.5rem;
	}
	
	.subtitle {
		color: var(--text-secondary, #6b7280);
		font-size: 1rem;
	}
	
	.header-actions {
		display: flex;
		gap: 0.75rem;
		flex-wrap: wrap;
	}
	
	.refresh-btn,
	.export-btn,
	.manual-refresh-btn {
		padding: 0.5rem 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.875rem;
		white-space: nowrap;
	}
	
	.refresh-btn:hover,
	.export-btn:hover,
	.manual-refresh-btn:hover:not(:disabled) {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.refresh-btn.auto {
		background: var(--primary-color, #2563eb);
		color: white;
		border-color: var(--primary-color, #2563eb);
	}
	
	.manual-refresh-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
	
	.logs-stats {
		display: flex;
		gap: 2rem;
		margin-bottom: 1.5rem;
		padding: 1rem;
		background: white;
		border-radius: 8px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.stat {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.stat-label {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.stat-value {
		font-size: 1.25rem;
		font-weight: 600;
		color: var(--text-primary, #111827);
	}
	
	.error-message {
		background: #fef2f2;
		border: 1px solid #fecaca;
		color: #dc2626;
		padding: 1rem;
		border-radius: 8px;
		margin-bottom: 1.5rem;
	}
	
	@media (max-width: 768px) {
		.page-header {
			flex-direction: column;
			align-items: stretch;
		}
		
		.header-actions {
			justify-content: stretch;
		}
		
		.header-actions button {
			flex: 1;
		}
		
		.logs-stats {
			flex-direction: column;
			gap: 1rem;
		}
	}
</style>
