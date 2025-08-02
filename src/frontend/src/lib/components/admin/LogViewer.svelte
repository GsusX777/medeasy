<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { onMount } from 'svelte';
	
	// [ATV] Log viewer for audit trail compliance
	// [PSF] Patient safety through detailed log inspection
	// [DSC] Swiss compliance log display
	
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
	
	export let logs: LogEntry[];
	export let loading: boolean = false;
	
	let selectedLog: LogEntry | null = null;
	let logContainer: HTMLElement;
	
	const levelConfig = {
		debug: { color: '#6b7280', bg: '#f9fafb', icon: '🔍' },
		info: { color: '#2563eb', bg: '#eff6ff', icon: 'ℹ️' },
		warn: { color: '#d97706', bg: '#fffbeb', icon: '⚠️' },
		error: { color: '#dc2626', bg: '#fef2f2', icon: '❌' }
	};
	
	const serviceConfig = {
		backend: { color: '#059669', icon: '🔧', label: '.NET Backend' },
		'ai-service': { color: '#7c3aed', icon: '🤖', label: 'Python AI' },
		frontend: { color: '#2563eb', icon: '🖥️', label: 'Svelte Frontend' }
	};
	
	function formatTimestamp(timestamp: string): string {
		const date = new Date(timestamp);
		return date.toLocaleString('de-CH', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit',
			second: '2-digit'
		});
	}
	
	function getRelativeTime(timestamp: string): string {
		const now = new Date();
		const logTime = new Date(timestamp);
		const diffMs = now.getTime() - logTime.getTime();
		
		const seconds = Math.floor(diffMs / 1000);
		const minutes = Math.floor(seconds / 60);
		const hours = Math.floor(minutes / 60);
		const days = Math.floor(hours / 24);
		
		if (days > 0) return `vor ${days}d`;
		if (hours > 0) return `vor ${hours}h`;
		if (minutes > 0) return `vor ${minutes}m`;
		return `vor ${seconds}s`;
	}
	
	function selectLog(log: LogEntry) {
		selectedLog = selectedLog?.id === log.id ? null : log;
	}
	
	function copyToClipboard(text: string) {
		navigator.clipboard.writeText(text).then(() => {
			// Could show a toast notification here
			console.log('Copied to clipboard');
		});
	}
	
	function scrollToTop() {
		if (logContainer) {
			logContainer.scrollTop = 0;
		}
	}
	
	onMount(() => {
		// Auto-scroll to top when new logs are loaded
		scrollToTop();
	});
	
	$: if (logs) {
		scrollToTop();
	}
</script>

<div class="log-viewer">
	<div class="viewer-header">
		<div class="log-count">
			{#if loading}
				<span class="loading">🔄 Lade Logs...</span>
			{:else}
				<span>{logs.length} Log-Einträge</span>
			{/if}
		</div>
		
		{#if logs.length > 10}
			<button class="scroll-top-btn" on:click={scrollToTop}>
				⬆️ Nach oben
			</button>
		{/if}
	</div>
	
	<div class="log-container" bind:this={logContainer}>
		{#if loading}
			<div class="loading-state">
				<div class="loading-spinner"></div>
				<p>Logs werden geladen...</p>
			</div>
		{:else if logs.length === 0}
			<div class="empty-state">
				<div class="empty-icon">📋</div>
				<h3>Keine Logs gefunden</h3>
				<p>Keine Logs entsprechen den aktuellen Filterkriterien.</p>
			</div>
		{:else}
			<div class="log-list">
				{#each logs as log (log.id)}
					<div 
						class="log-entry"
						class:selected={selectedLog?.id === log.id}
						class:error={log.level === 'error'}
						class:warn={log.level === 'warn'}
						on:click={() => selectLog(log)}
						role="button"
						tabindex="0"
						on:keydown={(e) => e.key === 'Enter' && selectLog(log)}
					>
						<div class="log-main">
							<div class="log-header">
								<div class="log-meta">
									<span 
										class="level-badge"
										style="background: {levelConfig[log.level].bg}; color: {levelConfig[log.level].color}"
									>
										{levelConfig[log.level].icon} {log.level.toUpperCase()}
									</span>
									
									<span 
										class="service-badge"
										style="color: {serviceConfig[log.service].color}"
									>
										{serviceConfig[log.service].icon} {serviceConfig[log.service].label}
									</span>
									
									<span class="timestamp">
										{getRelativeTime(log.timestamp)}
									</span>
								</div>
								
								{#if log.requestId}
									<button 
										class="request-id"
										on:click|stopPropagation={() => copyToClipboard(log.requestId || '')}
										title="Request ID kopieren"
									>
										📋 {log.requestId}
									</button>
								{/if}
							</div>
							
							<div class="log-message">
								{log.message}
							</div>
						</div>
						
						{#if selectedLog?.id === log.id}
							<div class="log-details">
								<div class="details-grid">
									<div class="detail-item">
										<span class="detail-label">Vollständiger Zeitstempel:</span>
										<span class="detail-value">{formatTimestamp(log.timestamp)}</span>
									</div>
									
									{#if log.userId}
										<div class="detail-item">
											<span class="detail-label">Benutzer:</span>
											<span class="detail-value">{log.userId}</span>
										</div>
									{/if}
									
									{#if log.requestId}
										<div class="detail-item">
											<span class="detail-label">Request ID:</span>
											<span class="detail-value">{log.requestId}</span>
										</div>
									{/if}
									
									{#if log.details}
										<div class="detail-item full-width">
											<span class="detail-label">Details:</span>
											<pre class="detail-json">{JSON.stringify(log.details, null, 2)}</pre>
										</div>
									{/if}
								</div>
								
								<div class="details-actions">
									<button 
										class="copy-btn"
										on:click|stopPropagation={() => copyToClipboard(JSON.stringify(log, null, 2))}
									>
										📋 JSON kopieren
									</button>
									<button 
										class="copy-btn"
										on:click|stopPropagation={() => copyToClipboard(log.message)}
									>
										📝 Nachricht kopieren
									</button>
								</div>
							</div>
						{/if}
					</div>
				{/each}
			</div>
		{/if}
	</div>
</div>

<style>
	.log-viewer {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 12px;
		overflow: hidden;
	}
	
	.viewer-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 1rem 1.5rem;
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		background: var(--bg-secondary, #f8fafc);
	}
	
	.log-count {
		font-weight: 500;
		color: var(--text-primary, #111827);
	}
	
	.loading {
		color: var(--text-secondary, #6b7280);
	}
	
	.scroll-top-btn {
		padding: 0.5rem 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.875rem;
	}
	
	.scroll-top-btn:hover {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.log-container {
		max-height: 600px;
		overflow-y: auto;
	}
	
	.loading-state,
	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 3rem;
		text-align: center;
	}
	
	.loading-spinner {
		width: 32px;
		height: 32px;
		border: 3px solid var(--border-color, #e5e7eb);
		border-top: 3px solid var(--primary-color, #2563eb);
		border-radius: 50%;
		animation: spin 1s linear infinite;
		margin-bottom: 1rem;
	}
	
	@keyframes spin {
		0% { transform: rotate(0deg); }
		100% { transform: rotate(360deg); }
	}
	
	.empty-icon {
		font-size: 3rem;
		margin-bottom: 1rem;
		opacity: 0.5;
	}
	
	.empty-state h3 {
		color: var(--text-primary, #111827);
		margin-bottom: 0.5rem;
	}
	
	.empty-state p {
		color: var(--text-secondary, #6b7280);
	}
	
	.log-list {
		display: flex;
		flex-direction: column;
	}
	
	.log-entry {
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.log-entry:hover {
		background: var(--bg-hover, #f1f5f9);
	}
	
	.log-entry.selected {
		background: var(--primary-bg, #eff6ff);
		border-left: 4px solid var(--primary-color, #2563eb);
	}
	
	.log-entry.error {
		border-left: 4px solid #dc2626;
	}
	
	.log-entry.warn {
		border-left: 4px solid #d97706;
	}
	
	.log-main {
		padding: 1rem 1.5rem;
	}
	
	.log-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 0.5rem;
		flex-wrap: wrap;
		gap: 0.5rem;
	}
	
	.log-meta {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		flex-wrap: wrap;
	}
	
	.level-badge,
	.service-badge {
		padding: 0.25rem 0.5rem;
		border-radius: 4px;
		font-size: 0.75rem;
		font-weight: 500;
		white-space: nowrap;
	}
	
	.service-badge {
		background: var(--bg-secondary, #f8fafc);
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.timestamp {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.request-id {
		padding: 0.25rem 0.5rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 4px;
		background: white;
		cursor: pointer;
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		transition: all 0.2s ease;
	}
	
	.request-id:hover {
		border-color: var(--primary-color, #2563eb);
		color: var(--primary-color, #2563eb);
	}
	
	.log-message {
		color: var(--text-primary, #111827);
		line-height: 1.5;
		word-break: break-word;
	}
	
	.log-details {
		border-top: 1px solid var(--border-color, #e5e7eb);
		padding: 1rem 1.5rem;
		background: var(--bg-secondary, #f8fafc);
	}
	
	.details-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: 1rem;
		margin-bottom: 1rem;
	}
	
	.detail-item {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.detail-item.full-width {
		grid-column: 1 / -1;
	}
	
	.detail-label {
		font-size: 0.75rem;
		font-weight: 500;
		color: var(--text-secondary, #6b7280);
	}
	
	.detail-value {
		font-size: 0.875rem;
		color: var(--text-primary, #111827);
		word-break: break-all;
	}
	
	.detail-json {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 4px;
		padding: 0.75rem;
		font-size: 0.75rem;
		overflow-x: auto;
		color: var(--text-primary, #111827);
		white-space: pre-wrap;
	}
	
	.details-actions {
		display: flex;
		gap: 0.5rem;
		flex-wrap: wrap;
	}
	
	.copy-btn {
		padding: 0.5rem 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.875rem;
	}
	
	.copy-btn:hover {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	@media (max-width: 768px) {
		.log-main {
			padding: 0.75rem 1rem;
		}
		
		.log-details {
			padding: 0.75rem 1rem;
		}
		
		.log-meta {
			flex-direction: column;
			align-items: flex-start;
			gap: 0.5rem;
		}
		
		.details-grid {
			grid-template-columns: 1fr;
		}
	}
</style>
