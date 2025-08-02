<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { onMount, createEventDispatcher } from 'svelte';
	import BenchmarkComparison from './BenchmarkComparison.svelte';
	
	// Event dispatcher for parent communication
	const dispatch = createEventDispatcher();
	
	// [WMM] Whisper Multi-Model benchmark history tracking
	// [ATV] Audit trail for benchmark history
	// [PSF] Patient safety through performance validation
	
	// ✅ Updated interface to match actual API response structure
	interface BenchmarkHistoryItem {
		benchmarkId: string;
		timestamp: string;
		modelsCount: number;
		totalTimeMs: number;
		fastestModel: string;
		fastestTimeMs: number;
		slowestModel: string;
		slowestTimeMs: number;
		averageRamUsageMb: number;
		cudaUsed: boolean;
		status: 'completed' | 'failed' | 'partial';
		audioFileName: string; // Maps to AudioFilename from backend
		audioFileSizeMb: number;
		confidenceScore?: number; // [WMM] Whisper confidence score
		// ✅ Additional fields from API response
		performanceScore?: number;
		cpuUsage?: number;
		ramUsage?: number;
	}
	
	export let refreshTrigger: number = 0;
	export let history: any[] = []; // Accept any benchmark data structure
	
	// Internal state
	let loading = false;
	let error: string | null = null;
	let selectedPeriod = '7d';
	let sortBy = 'timestamp';
	let sortOrder: 'asc' | 'desc' = 'desc';
	
	// ✅ Pagination state
	let currentPage = 1;
	let pageSize = 10;
	let totalPages = 1;
	
	const pageSizeOptions = [
		{ value: 10, label: '10 pro Seite' },
		{ value: 20, label: '20 pro Seite' },
		{ value: 50, label: '50 pro Seite' },
		{ value: 100, label: '100 pro Seite' }
	];
	
	// ✅ Comparison functionality
	let selectedForComparison: Set<string> = new Set();
	let showComparison = false;
	const MAX_COMPARISON_ITEMS = 4;
	
	// ✅ Toggle comparison selection
	function toggleComparison(benchmarkId: string) {
		if (selectedForComparison.has(benchmarkId)) {
			selectedForComparison.delete(benchmarkId);
		} else if (selectedForComparison.size < MAX_COMPARISON_ITEMS) {
			selectedForComparison.add(benchmarkId);
		}
		selectedForComparison = new Set(selectedForComparison); // Trigger reactivity
	}
	
	// ✅ Clear comparison selection
	function clearComparison() {
		selectedForComparison.clear();
		selectedForComparison = new Set();
		showComparison = false;
	}
	
	// ✅ Start comparison view
	function startComparison() {
		if (selectedForComparison.size >= 2) {
			showComparison = true;
		}
	}
	
	// ✅ Pagination calculations
	$: totalPages = Math.ceil(filteredHistory.length / pageSize);
	$: paginatedHistory = filteredHistory.slice((currentPage - 1) * pageSize, currentPage * pageSize);
	
	// ✅ Reset to first page when filters change
	$: if (selectedPeriod || sortBy || sortOrder) {
		currentPage = 1;
	}
	
	// ✅ Get selected benchmarks for comparison
	$: selectedBenchmarks = filteredHistory.filter(item => selectedForComparison.has(item.benchmarkId));
	
	// ✅ Pagination functions
	function goToPage(page: number) {
		if (page >= 1 && page <= totalPages) {
			currentPage = page;
		}
	}
	
	function changePageSize(newSize: number) {
		pageSize = newSize;
		currentPage = 1; // Reset to first page
	}
	
	// ✅ Get page numbers for pagination
	function getPageNumbers(): number[] {
		const pages: number[] = [];
		const maxVisible = 5;
		
		if (totalPages <= maxVisible) {
			for (let i = 1; i <= totalPages; i++) {
				pages.push(i);
			}
		} else {
			const start = Math.max(1, currentPage - 2);
			const end = Math.min(totalPages, start + maxVisible - 1);
			
			for (let i = start; i <= end; i++) {
				pages.push(i);
			}
		}
		
		return pages;
	}
	
	// ✅ Close comparison view
	function closeComparison() {
		showComparison = false;
	}
	
	const periods = [
		{ value: '1d', label: 'Letzter Tag' },
		{ value: '7d', label: 'Letzte Woche' },
		{ value: '30d', label: 'Letzter Monat' },
		{ value: '90d', label: 'Letzte 3 Monate' },
		{ value: 'all', label: 'Alle' }
	];
	
	const sortOptions = [
		{ value: 'timestamp', label: 'Datum' },
		{ value: 'totalTimeMs', label: 'Gesamtzeit' },
		{ value: 'modelsCount', label: 'Anzahl Modelle' },
		{ value: 'fastestTimeMs', label: 'Schnellste Zeit' }
	];
	
	// ✅ REMOVED: Mock data - now using real data from backend API
	// Real data is loaded by parent component and passed as props
	
	async function loadHistory() {
		// ✅ FIX: Use real data passed as props instead of mock data
		// The parent component already loads real data from API
		loading = true;
		error = null;
		
		try {
			// Use the real history data passed as props
			// No need for additional API call - parent already loads data
			console.log('✅ BenchmarkHistory: Using real data from props:', history.length, 'entries');
			// 🔍 Debug: Check confidenceScore values in received data
			history.slice(0, 3).forEach((item, index) => {
				console.log(`[DEBUG] Item ${index}: confidenceScore =`, item.confidenceScore, typeof item.confidenceScore);
				console.log(`[DEBUG] Item ${index}: confidence =`, item.confidence, typeof item.confidence);
				console.log(`[DEBUG] Item ${index}: ALL FIELDS =`, Object.keys(item));
			});
			
			// 🔍 Debug logging for status mapping issues
			if (history.length > 0) {
				console.log('🔍 First item debug:', {
					status: history[0].status,
					statusType: typeof history[0].status,
					statusIcon: getStatusIcon(history[0].status),
					statusLabel: getStatusLabel(history[0].status),
					fastestModel: history[0].fastestModel,
					totalTimeMs: history[0].totalTimeMs,
					performanceScore: history[0].performanceScore
				});
			}
			
			// Simulate small delay for UX
			await new Promise(resolve => setTimeout(resolve, 100));
			
			// history is already set via props - no need to reassign
		} catch (err) {
			error = err instanceof Error ? err.message : 'Fehler beim Laden der Benchmark-Historie';
			console.error('Error loading benchmark history:', err);
		} finally {
			loading = false;
		}
	}
	
	function formatTime(ms: number): string {
		if (ms === 0) return 'N/A';
		if (ms < 1000) return `${ms.toFixed(0)}ms`;
		const seconds = ms / 1000;
		if (seconds < 60) return `${seconds.toFixed(1)}s`;
		const minutes = Math.floor(seconds / 60);
		const remainingSeconds = seconds % 60;
		return `${minutes}:${remainingSeconds.toFixed(0).padStart(2, '0')}min`;
	}
	
	function formatTimestamp(timestamp: string): string {
		return new Date(timestamp).toLocaleString('de-CH', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}
	
	function formatFileSize(sizeMb: number | undefined): string {
		if (sizeMb === undefined || sizeMb === null || isNaN(sizeMb)) return 'N/A';
		if (sizeMb === 0) return '0MB';
		if (sizeMb < 1) return `${(sizeMb * 1024).toFixed(0)}KB`;
		return `${sizeMb.toFixed(1)}MB`;
	}
	
	function getStatusColor(status: string): string {
		switch (status) {
			case 'completed': return '#059669';
			case 'failed': return '#dc2626';
			case 'partial': return '#d97706';
			default: return '#6b7280';
		}
	}
	
	function getStatusIcon(status: string): string {
		switch (status) {
			case 'completed': return '✅';
			case 'failed': return '❌';
			case 'partial': return '⚠️';
			default: return '❓';
		}
	}
	
	function getStatusLabel(status: string): string {
		switch (status) {
			case 'completed': return 'Erfolgreich';
			case 'failed': return 'Fehlgeschlagen';
			case 'partial': return 'Teilweise';
			default: return 'Unbekannt';
		}
	}
	
	function sortHistory(items: any[]): any[] {
		return [...items].sort((a: any, b: any) => {
			let aVal = a[sortBy];
			let bVal = b[sortBy];
			
			// Handle string comparisons
			if (typeof aVal === 'string' && typeof bVal === 'string') {
				aVal = aVal.toLowerCase();
				bVal = bVal.toLowerCase();
			}
			
			let comparison = 0;
			if (aVal < bVal) {
				comparison = -1;
			} else if (aVal > bVal) {
				comparison = 1;
			}
			
			return sortOrder === 'desc' ? -comparison : comparison;
		});
	}
	
	function filterByPeriod(items: BenchmarkHistoryItem[]): BenchmarkHistoryItem[] {
		if (selectedPeriod === 'all') return items;
		
		const now = new Date();
		const periodMs = {
			'1d': 24 * 60 * 60 * 1000,
			'7d': 7 * 24 * 60 * 60 * 1000,
			'30d': 30 * 24 * 60 * 60 * 1000,
			'90d': 90 * 24 * 60 * 60 * 1000
		}[selectedPeriod] || 0;
		
		const cutoffTime = new Date(now.getTime() - periodMs);
		
		return items.filter(item => new Date(item.timestamp) >= cutoffTime);
	}
	
	function exportHistory() {
		const csvContent = [
			'Benchmark ID,Datum,Modelle,Gesamtzeit (ms),Schnellstes Modell,Schnellste Zeit (ms),Langsamstes Modell,Langsamste Zeit (ms),Durchschn. RAM (MB),CUDA,Status,Audio-Datei,Dateigröße (MB)',
			...filteredHistory.map(item => [
				item.benchmarkId,
				new Date(item.timestamp).toLocaleDateString('de-CH'),
				item.modelsCount,
				item.totalTimeMs,
				item.fastestModel,
				item.fastestTimeMs,
				item.slowestModel,
				item.slowestTimeMs,
				item.averageRamUsageMb,
				item.cudaUsed ? 'Ja' : 'Nein',
				item.status,
				item.audioFileName,
				item.audioFileSizeMb.toFixed(2)
			].join(','))
		].join('\n');
		
		const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
		const link = document.createElement('a');
		link.href = URL.createObjectURL(blob);
		link.download = `benchmark_history_${new Date().toISOString().split('T')[0]}.csv`;
		link.click();
	}
	
	/**
	 * Opens transcription comparison for selected benchmarks [NEW]
	 */
	function compareTranscriptions() {
		const selectedBenchmarks = Array.from(selectedForComparison).map(id => 
			history.find(item => item.benchmarkId === id)
		).filter(Boolean);
		
		if (selectedBenchmarks.length >= 2) {
			dispatch('compareTranscriptions', {
				selectedBenchmarks: selectedBenchmarks
			});
		}
	}
	
	$: filteredHistory = sortHistory(filterByPeriod(history));
	$: completedBenchmarks = history.filter(h => h.status === 'completed');
	$: averageTime = completedBenchmarks.length > 0 
		? completedBenchmarks.reduce((sum, h) => sum + h.totalTimeMs, 0) / completedBenchmarks.length 
		: 0;
	
	onMount(() => {
		loadHistory();
	});
	
	// Reload when refreshTrigger changes
	$: if (refreshTrigger > 0) {
		loadHistory();
	}
</script>

<div class="benchmark-history">
	<!-- Header with Controls -->
	<div class="history-header">
		<div class="header-info">
			<h3>📈 Benchmark-Historie</h3>
			<div class="stats-summary">
				<span class="stat">
					{history.length} Benchmarks
				</span>
				<span class="stat">
					{completedBenchmarks.length} erfolgreich
				</span>
				{#if averageTime > 0}
					<span class="stat">
						⌀ {formatTime(averageTime)}
					</span>
				{/if}
			</div>
		</div>
		
		<!-- ✅ Header Controls -->
		<div class="header-controls">
			<div class="control-group">
				<label for="period-select">Zeitraum:</label>
				<select id="period-select" bind:value={selectedPeriod}>
					{#each periods as period}
						<option value={period.value}>{period.label}</option>
					{/each}
				</select>
			</div>
			
			<div class="control-group">
				<label for="sort-select">Sortierung:</label>
				<select id="sort-select" bind:value={sortBy}>
					{#each sortOptions as option}
						<option value={option.value}>{option.label}</option>
					{/each}
				</select>
				<button 
					class="sort-order-btn" 
					on:click={() => sortOrder = sortOrder === 'asc' ? 'desc' : 'asc'}
					title={sortOrder === 'asc' ? 'Aufsteigend' : 'Absteigend'}
				>
					{sortOrder === 'asc' ? '↑' : '↓'}
				</button>
			</div>
			
			<!-- ✅ Page Size Control -->
			<div class="control-group">
				<label for="page-size-select">Einträge:</label>
				<select id="page-size-select" bind:value={pageSize} on:change={() => changePageSize(pageSize)}>
					{#each pageSizeOptions as option}
						<option value={option.value}>{option.label}</option>
					{/each}
				</select>
			</div>
			
			<!-- ✅ Comparison Controls -->
			<div class="comparison-controls">
				<div class="comparison-info">
					<span class="selection-count">{selectedForComparison.size}/{MAX_COMPARISON_ITEMS} ausgewählt</span>
					<button 
						class="compare-btn" 
						on:click={startComparison}
						disabled={selectedForComparison.size < 2}
						title={selectedForComparison.size < 2 ? 'Mindestens 2 Einträge zum Vergleichen auswählen' : 'Ausgewählte Einträge vergleichen'}
					>
						🔍 Vergleichen
					</button>
					<button 
						class="text-compare-btn" 
						on:click={compareTranscriptions}
						disabled={selectedForComparison.size < 2}
						title={selectedForComparison.size < 2 ? 'Mindestens 2 Einträge für Textvergleich auswählen' : 'Transkribierte Texte vergleichen'}
					>
						📝 Text vergleichen
					</button>
					<button 
						class="clear-btn" 
						on:click={clearComparison}
						disabled={selectedForComparison.size === 0}
						title={selectedForComparison.size === 0 ? 'Keine Auswahl zum Löschen' : 'Auswahl löschen'}
					>
						❌ Löschen
					</button>
				</div>
			</div>
		</div>
	</div>
	
	<!-- Loading State -->
	{#if loading}
		<div class="loading-state">
			<div class="loading-spinner">🔄</div>
			<span>Lade Benchmark-Historie...</span>
		</div>
	{/if}
	
	<!-- Error State -->
	{#if error}
		<div class="error-state">
			<div class="error-icon">❌</div>
			<span class="error-message">{error}</span>
			<button class="retry-btn" on:click={loadHistory}>
				Erneut versuchen
			</button>
		</div>
	{/if}
	
	<!-- History Content -->
	{#if !loading && !error}
		{#if filteredHistory.length === 0}
			<div class="empty-state">
				<div class="empty-icon">📊</div>
				<h4>Keine Benchmark-Historie gefunden</h4>
				<p>
					{selectedPeriod === 'all' 
						? 'Es wurden noch keine Benchmarks durchgeführt.' 
						: 'Keine Benchmarks im ausgewählten Zeitraum gefunden.'}
				</p>
			</div>
		{:else}
			<div class="history-table">
				<div class="table-header">
					<div class="col-checkbox">Auswahl</div>
					<div class="col-status">Status</div>
					<div class="col-timestamp">Datum & Zeit</div>
					<div class="col-models">Modell</div>
					<div class="col-time">Zeit</div>
					<div class="col-performance">Performance</div>
					<div class="col-confidence">Konfidenz</div>
					<div class="col-file">Audio-Datei</div>
				</div>
				
				{#each paginatedHistory as item}
					<div class="table-row" class:failed={item.status === 'failed'} class:selected={selectedForComparison.has(item.benchmarkId)}>
						<!-- ✅ Checkbox for comparison -->
						<div class="col-checkbox">
							<input 
								type="checkbox" 
								checked={selectedForComparison.has(item.benchmarkId)}
								on:change={() => toggleComparison(item.benchmarkId)}
								disabled={!selectedForComparison.has(item.benchmarkId) && selectedForComparison.size >= MAX_COMPARISON_ITEMS}
							/>
						</div>
						<!-- Status -->
						<div class="col-status">
							<div class="status-badge" style="color: {getStatusColor(item.status)}">
								<span class="status-icon">{getStatusIcon(item.status)}</span>
								<span class="status-text">{getStatusLabel(item.status)}</span>
							</div>
						</div>
						
						<!-- Timestamp -->
						<div class="col-timestamp">
							<div class="timestamp-main">{formatTimestamp(item.timestamp)}</div>
							<div class="benchmark-id">{item.benchmarkId}</div>
						</div>
						
						<!-- ✅ Model (fixed display) -->
						<div class="col-models">
							{#if item.status === 'completed' && item.fastestModel}
								<div class="model-name">{item.fastestModel}</div>
								{#if item.slowestModel && item.fastestModel !== item.slowestModel}
									<div class="models-note">+{(item.modelsCount || 1) - 1} weitere</div>
								{/if}
							{:else if item.fastestModel}
								<div class="model-name">{item.fastestModel}</div>
							{:else}
								<div class="model-name">N/A</div>
							{/if}
						</div>
						
						<!-- ✅ Time (fixed display) -->
						<div class="col-time">
							{#if item.status === 'completed'}
								<div class="total-time">{formatTime(item.totalTimeMs)}</div>
								{#if item.fastestTimeMs !== item.slowestTimeMs && item.slowestTimeMs}
									<div class="time-range">
										{formatTime(item.fastestTimeMs)} - {formatTime(item.slowestTimeMs)}
									</div>
								{:else if item.fastestTimeMs}
									<div class="time-single">{formatTime(item.fastestTimeMs)}</div>
								{/if}
							{:else}
								<div class="no-data">N/A</div>
							{/if}
						</div>
						
						<!-- Performance -->
						<div class="col-performance">
							{#if item.status === 'completed'}
								<div class="performance-score">
									⚡ {item.performanceScore?.toFixed(1) || 'N/A'}
								</div>
								<div class="ram-usage">
									💾 {item.averageRamUsageMb}MB
								</div>
								<div class="cpu-usage">
									🖥️ {item.cpuUsage?.toFixed(1) || 'N/A'}%
								</div>
							{:else}
								<div class="no-data">N/A</div>
							{/if}
						</div>
						
						<!-- Confidence -->
						<div class="col-confidence">
							{#if item.status === 'completed' && typeof item.confidenceScore === 'number' && item.confidenceScore > 0}
								<div class="confidence-score">
									🎯 {(item.confidenceScore * 100).toFixed(1)}%
								</div>
							{:else}
								<div class="no-data">N/A</div>
							{/if}
						</div>
						
						<!-- File -->
						<div class="col-file">
							<div class="file-name">{item.audioFileName}</div>
							<div class="file-size">{formatFileSize(item.audioFileSizeMb)}</div>
						</div>

					</div>
				{/each}
			</div>
			
			<!-- ✅ Pagination Controls -->
			{#if totalPages > 1}
				<div class="pagination">
					<div class="pagination-info">
						<span>Seite {currentPage} von {totalPages} ({filteredHistory.length} Einträge)</span>
					</div>
					
					<div class="pagination-controls">
						<!-- First Page -->
						<button 
							class="page-btn" 
							on:click={() => goToPage(1)}
							disabled={currentPage === 1}
							title="Erste Seite"
						>
							«
						</button>
						
						<!-- Previous Page -->
						<button 
							class="page-btn" 
							on:click={() => goToPage(currentPage - 1)}
							disabled={currentPage === 1}
							title="Vorherige Seite"
						>
							‹
						</button>
						
						<!-- Page Numbers -->
						{#each getPageNumbers() as pageNum}
							<button 
								class="page-btn" 
								class:active={pageNum === currentPage}
								on:click={() => goToPage(pageNum)}
							>
								{pageNum}
							</button>
						{/each}
						
						<!-- Next Page -->
						<button 
							class="page-btn" 
							on:click={() => goToPage(currentPage + 1)}
							disabled={currentPage === totalPages}
							title="Nächste Seite"
						>
							›
						</button>
						
						<!-- Last Page -->
						<button 
							class="page-btn" 
							on:click={() => goToPage(totalPages)}
							disabled={currentPage === totalPages}
							title="Letzte Seite"
						>
							»
						</button>
					</div>
				</div>
			{/if}
		{/if}
	{/if}
	
	<!-- ✅ Comparison View -->
	{#if showComparison && selectedBenchmarks.length >= 2}
		<BenchmarkComparison 
			selectedBenchmarks={selectedBenchmarks}
			onClose={closeComparison}
		/>
	{/if}
</div>

<style>
	.benchmark-history {
		display: flex;
		flex-direction: column;
		gap: 1.5rem;
	}
	
	.history-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		gap: 1rem;
		flex-wrap: wrap;
	}
	
	.header-info h3 {
		margin-bottom: 0.5rem;
		color: var(--text-primary, #111827);
	}
	
	.stats-summary {
		display: flex;
		gap: 1rem;
		flex-wrap: wrap;
	}
	
	.stat {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		padding: 0.25rem 0.5rem;
		background: var(--bg-secondary, #f8fafc);
		border-radius: 4px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.header-controls {
		display: flex;
		gap: 1.5rem;
		align-items: center;
		flex-wrap: wrap;
	}
	
	.control-group {
		display: flex;
		align-items: center;
		gap: 0.5rem;
	}
	
	.control-group label {
		font-size: 0.875rem;
		font-weight: 500;
		color: var(--text-primary, #111827);
		white-space: nowrap;
	}
	
	.control-group select,
	#period-select,
	#sort-select {
		padding: 0.5rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		font-size: 0.875rem;
		cursor: pointer;
		color: var(--text-primary, #111827);
		transition: all 0.2s ease;
		min-width: 120px;
	}
	
	.control-group select:hover,
	.period-select:hover,
	.sort-select:hover {
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	.control-group select:focus,
	.period-select:focus,
	#period-select:focus,
	#sort-select:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	.period-select,
	.sort-select {
		padding: 0.5rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		font-size: 0.875rem;
		cursor: pointer;
		color: var(--text-primary, #111827);
		transition: all 0.2s ease;
	}
	
	.sort-order-btn,
	.export-btn,
	.refresh-btn {
		padding: 0.5rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		cursor: pointer;
		font-size: 0.875rem;
		transition: all 0.2s ease;
		color: var(--text-primary, #111827);
		font-weight: 500;
	}
	
	.sort-order-btn {
		min-width: 40px;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 1rem;
	}
	
	.sort-order-btn:hover,
	.export-btn:hover,
	.refresh-btn:hover {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.sort-order-btn:disabled,
	.export-btn:disabled,
	.refresh-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
	
	/* ✅ Comparison Controls */
	.comparison-controls {
		display: flex;
		align-items: center;
		gap: 1rem;
		margin-left: auto;
	}
	
	.comparison-info {
		display: flex;
		align-items: center;
		gap: 0.5rem;
	}
	
	.selection-count {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.compare-btn {
		padding: 0.5rem 1rem;
		background: var(--primary-color, #2563eb);
		color: white;
		border: none;
		border-radius: 6px;
		font-size: 0.875rem;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.compare-btn:hover {
		background: var(--primary-hover, #1d4ed8);
		transform: translateY(-1px);
	}
	
	.text-compare-btn {
		padding: 0.5rem 1rem;
		background: var(--success-color, #059669);
		color: white;
		border: none;
		border-radius: 6px;
		font-size: 0.875rem;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.text-compare-btn:hover {
		background: var(--success-hover, #047857);
		transform: translateY(-1px);
	}
	
	.text-compare-btn:disabled,
	.compare-btn:disabled,
	.clear-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
		transform: none;
	}
	
	.clear-btn {
		padding: 0.5rem 1rem;
		background: var(--error-color, #dc2626);
		color: white;
		border: none;
		border-radius: 6px;
		font-size: 0.875rem;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.clear-btn:hover {
		background: var(--error-hover, #b91c1c);
	}
	
	.comparison-hint {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		font-style: italic;
	}
	
	/* ✅ Pagination Styles */
	.pagination {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 1.5rem 2rem;
		border-top: 1px solid var(--border-color, #e5e7eb);
		background: var(--bg-secondary, #f8fafc);
		border-radius: 0 0 8px 8px;
		margin-top: auto;
	}
	
	.pagination-info {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.pagination-controls {
		display: flex;
		align-items: center;
		gap: 0.25rem;
	}
	
	.page-btn {
		padding: 0.5rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
		min-width: 40px;
		display: flex;
		align-items: center;
		justify-content: center;
	}
	
	.page-btn:hover:not(:disabled) {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
		color: var(--primary-color, #2563eb);
	}
	
	.page-btn.active {
		background: var(--primary-color, #2563eb);
		color: white;
		border-color: var(--primary-color, #2563eb);
		font-weight: 600;
	}
	
	.page-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
		background: var(--bg-secondary, #f8fafc);
		color: var(--text-secondary, #6b7280);
	}
	
	.loading-state,
	.error-state,
	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 1rem;
		padding: 3rem 1rem;
		text-align: center;
	}
	
	.loading-spinner,
	.error-icon,
	.empty-icon {
		font-size: 3rem;
		opacity: 0.5;
	}
	
	.loading-spinner {
		animation: spin 1s linear infinite;
	}
	
	@keyframes spin {
		from { transform: rotate(0deg); }
		to { transform: rotate(360deg); }
	}
	
	.error-message {
		color: #dc2626;
		font-weight: 500;
	}
	
	.retry-btn {
		padding: 0.5rem 1rem;
		border: 1px solid #dc2626;
		border-radius: 6px;
		background: white;
		color: #dc2626;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.retry-btn:hover {
		background: #dc2626;
		color: white;
	}
	
	.empty-state h4 {
		margin: 0;
		color: var(--text-primary, #111827);
	}
	
	.empty-state p {
		margin: 0;
		color: var(--text-secondary, #6b7280);
	}
	
	.history-table {
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		overflow: hidden;
		background: white;
	}
	
	.table-header {
		display: grid;
		grid-template-columns: 60px 100px 180px 140px 120px 160px 100px 140px;
		gap: 1rem;
		padding: 1rem;
		background: var(--bg-secondary, #f8fafc);
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		font-weight: 600;
		font-size: 0.875rem;
		color: var(--text-primary, #111827);
	}
	
	.table-row {
		display: grid;
		grid-template-columns: 60px 100px 180px 140px 120px 160px 100px 140px;
		gap: 1rem;
		padding: 1rem;
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		transition: background-color 0.2s ease;
	}
	
	/* ✅ Selected row styling */
	.table-row.selected {
		background: var(--primary-bg, #eff6ff);
		border-left: 3px solid var(--primary-color, #2563eb);
	}
	
	.table-row:hover {
		background: var(--bg-secondary, #f8fafc);
	}
	
	.table-row.selected:hover {
		background: var(--primary-bg, #eff6ff);
	}
	
	/* ✅ Column-specific styling */
	.col-checkbox {
		display: flex;
		align-items: center;
		justify-content: center;
	}
	
	.col-checkbox input[type="checkbox"] {
		width: 18px;
		height: 18px;
		cursor: pointer;
		accent-color: var(--primary-color, #2563eb);
	}
	
	.col-checkbox input[type="checkbox"]:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
	
	.model-name {
		font-weight: 600;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.models-note {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		margin-top: 2px;
	}
	
	.time-single {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		margin-top: 2px;
	}
	
	.table-row.failed {
		background: #fef2f2;
	}
	
	.table-row:last-child {
		border-bottom: none;
	}
	
	.status-badge {
		display: flex;
		align-items: center;
		gap: 0.25rem;
		font-size: 0.875rem;
		font-weight: 500;
	}
	
	.timestamp-main {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.benchmark-id {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-family: monospace;
	}
	
	.models-count {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.models-range {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.total-time {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.time-range {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.ram-usage,
	.cpu-usage,
	.cuda-status {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.performance-score {
		font-weight: 600;
		color: var(--success-600, #059669);
		font-size: 0.875rem;
		margin-bottom: 0.25rem;
	}
	
	.file-name {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
		word-break: break-all;
	}
	
	.file-size {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.confidence-score {
		font-weight: 600;
		color: var(--success-600, #059669);
		font-size: 0.875rem;
		display: flex;
		align-items: center;
		gap: 0.25rem;
	}
	
	.hardware-indicator {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		margin-bottom: 0.25rem;
	}
	
	.hardware-stats {
		display: flex;
		flex-direction: column;
		gap: 0.125rem;
	}
	
	.cpu-stat,
	.ram-stat {
		font-size: 0.7rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.no-data {
		color: var(--text-secondary, #6b7280);
		font-style: italic;
		font-size: 0.875rem;
	}
	
	.pagination {
		display: flex;
		flex-direction: row;
		align-items: center;
		gap: 0.5rem;
		padding: 1rem;
		background: var(--bg-secondary, #f8fafc);
		border-top: 1px solid var(--border-color, #e5e7eb);
	}
	
	.pagination-controls {
		display: flex;
		flex-wrap: wrap;
		justify-content: center;
		gap: 0.25rem;
	}
	
	.page-btn {
		min-width: 35px;
		padding: 0.4rem 0.6rem;
		font-size: 0.8rem;
		background: var(--bg-secondary, #f8fafc);
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 4px;
		cursor: pointer;
	}
	
	.page-btn:hover {
		background: var(--primary-bg, #eff6ff);
		border-color: var(--primary-color, #2563eb);
	}
	
	.page-btn.selected {
		background: var(--primary-color, #2563eb);
		color: white;
		border-color: var(--primary-color, #2563eb);
	}
	
	.page-info {
		font-size: 0.8rem;
		color: var(--text-secondary, #6b7280);
		margin-left: 1rem;
	}
	
	@media (max-width: 1200px) {
		.table-header,
		.table-row {
			grid-template-columns: 50px 90px 140px 100px 90px 120px 80px 100px;
			gap: 0.5rem;
			padding: 0.75rem;
		}
		
		.header-controls {
			flex-direction: column;
			align-items: stretch;
		}
	}
	
	@media (max-width: 768px) {
		.history-header {
			flex-direction: column;
			align-items: stretch;
		}
		
		.stats-summary {
			justify-content: center;
		}
		
		.header-controls {
			flex-direction: row;
			justify-content: center;
			flex-wrap: wrap;
		}
		
		.table-header {
			display: none;
		}
		
		.table-row {
			display: flex;
			flex-direction: column;
			gap: 0.5rem;
			padding: 1rem;
		}
		
		.table-row > div {
			display: flex;
			justify-content: space-between;
			align-items: center;
			padding: 0.25rem 0;
		}
		
		.table-row > div::before {
			content: attr(data-label);
			font-weight: 600;
			color: var(--text-secondary, #6b7280);
			font-size: 0.75rem;
		}
		
		.col-status::before { content: 'Status: '; }
		.col-timestamp::before { content: 'Datum: '; }
		.col-models::before { content: 'Modelle: '; }
		.col-time::before { content: 'Zeit: '; }
		.col-performance::before { content: 'Performance: '; }
		.col-file::before { content: 'Datei: '; }
		.col-hardware::before { content: 'Hardware: '; }
	}
</style>
