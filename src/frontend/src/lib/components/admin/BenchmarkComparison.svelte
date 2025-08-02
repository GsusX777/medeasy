<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	// [WMM] Whisper Multi-Model comparison view
	// [ATV] Audit trail for benchmark comparisons
	// [PSF] Patient safety through performance validation
	
	export let selectedBenchmarks: any[] = [];
	export let onClose: () => void;
	
	// ✅ Format functions
	function formatTimestamp(timestamp: string): string {
		return new Date(timestamp).toLocaleString('de-CH', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}
	
	function formatTime(ms: number): string {
		if (ms < 1000) return `${ms}ms`;
		const seconds = Math.round(ms / 1000);
		if (seconds < 60) return `${seconds}s`;
		const minutes = Math.floor(seconds / 60);
		const remainingSeconds = seconds % 60;
		return `${minutes}m ${remainingSeconds}s`;
	}
	
	function formatFileSize(mb: number): string {
		if (mb < 1) return `${Math.round(mb * 1000)}KB`;
		return `${mb.toFixed(1)}MB`;
	}
	
	// ✅ Get best/worst performers
	$: fastestBenchmark = selectedBenchmarks.reduce((fastest, current) => 
		(current.fastestTimeMs || Infinity) < (fastest.fastestTimeMs || Infinity) ? current : fastest
	, selectedBenchmarks[0]);
	
	$: slowestBenchmark = selectedBenchmarks.reduce((slowest, current) => 
		(current.slowestTimeMs || 0) > (slowest.slowestTimeMs || 0) ? current : slowest
	, selectedBenchmarks[0]);
	
	$: bestPerformance = selectedBenchmarks.reduce((best, current) => 
		(current.performanceScore || 0) > (best.performanceScore || 0) ? current : best
	, selectedBenchmarks[0]);
</script>

<div class="comparison-overlay">
	<div class="comparison-modal">
		<!-- Header -->
		<div class="comparison-header">
			<h2>🔍 Benchmark-Vergleich</h2>
			<button class="close-btn" on:click={onClose}>❌</button>
		</div>
		
		<!-- Quick Stats -->
		<div class="comparison-stats">
			<div class="stat-card fastest">
				<div class="stat-icon">🚀</div>
				<div class="stat-content">
					<span class="stat-label">Schnellstes Modell</span>
					<span class="stat-value">{fastestBenchmark?.fastestModel || 'N/A'}</span>
					<span class="stat-detail">{formatTime(fastestBenchmark?.fastestTimeMs || 0)}</span>
				</div>
			</div>
			
			<div class="stat-card performance">
				<div class="stat-icon">⚡</div>
				<div class="stat-content">
					<span class="stat-label">Beste Performance</span>
					<span class="stat-value">{bestPerformance?.fastestModel || 'N/A'}</span>
					<span class="stat-detail">Score: {bestPerformance?.performanceScore?.toFixed(1) || 'N/A'}</span>
				</div>
			</div>
			
			<div class="stat-card count">
				<div class="stat-icon">📊</div>
				<div class="stat-content">
					<span class="stat-label">Verglichene Tests</span>
					<span class="stat-value">{selectedBenchmarks.length}</span>
					<span class="stat-detail">Benchmarks</span>
				</div>
			</div>
		</div>
		
		<!-- ✅ Column-based Comparison Table -->
		<div class="comparison-columns">
			<!-- Row Labels Column -->
			<div class="labels-column">
				<div class="label-row">Modell</div>
				<div class="label-row">Datum</div>
				<div class="label-row">Modellzeit</div>
				<div class="label-row">Performance-Score</div>
				<div class="label-row">CPU-Auslastung</div>
				<div class="label-row">RAM-Verbrauch</div>
				<div class="label-row">Konfidenz</div>
				<div class="label-row">Audio-Datei</div>
				<div class="label-row">Dateigröße</div>
			</div>
			
			<!-- Model Columns -->
			{#each selectedBenchmarks as benchmark, index}
				<div class="model-column" class:fastest={benchmark === fastestBenchmark} class:best-performance={benchmark === bestPerformance}>
					<!-- Model Name -->
					<div class="data-row">
						<div class="model-name">{benchmark.fastestModel || 'N/A'}</div>
						{#if benchmark.slowestModel && benchmark.fastestModel !== benchmark.slowestModel}
							<div class="models-note">+{(benchmark.modelsCount || 1) - 1} weitere</div>
						{/if}
					</div>
					
					<!-- Date -->
					<div class="data-row">
						<div class="date-main">{formatTimestamp(benchmark.timestamp)}</div>
					</div>
					
					<!-- Model Time -->
					<div class="data-row">
						{#if benchmark.status === 'completed'}
							{#if benchmark.fastestTimeMs !== benchmark.slowestTimeMs && benchmark.slowestTimeMs}
								<div class="time-range">
									{formatTime(benchmark.fastestTimeMs)} - {formatTime(benchmark.slowestTimeMs)}
								</div>
							{:else if benchmark.fastestTimeMs}
								<div class="time-single">{formatTime(benchmark.fastestTimeMs)}</div>
							{/if}
						{:else}
							<div class="no-data">N/A</div>
						{/if}
					</div>
					
					<!-- Performance Score -->
					<div class="data-row">
						{#if benchmark.status === 'completed'}
							<div class="performance-score">
								⚡ {benchmark.performanceScore?.toFixed(1) || 'N/A'}
							</div>
						{:else}
							<div class="no-data">N/A</div>
						{/if}
					</div>
					
					<!-- CPU Usage -->
					<div class="data-row">
						{#if benchmark.status === 'completed'}
							<div class="cpu-usage">
								🖥️ {benchmark.cpuUsage?.toFixed(1) || 'N/A'}%
							</div>
						{:else}
							<div class="no-data">N/A</div>
						{/if}
					</div>
					
					<!-- RAM Usage -->
					<div class="data-row">
						{#if benchmark.status === 'completed'}
							<div class="ram-usage">
								💾 {benchmark.averageRamUsageMb}MB
							</div>
						{:else}
							<div class="no-data">N/A</div>
						{/if}
					</div>
					
					<!-- Confidence -->
					<div class="data-row">
						{#if benchmark.status === 'completed' && typeof benchmark.confidenceScore === 'number' && benchmark.confidenceScore > 0}
							<div class="confidence-score">
								🎯 {(benchmark.confidenceScore * 100).toFixed(1)}%
							</div>
						{:else}
							<div class="no-data">N/A</div>
						{/if}
					</div>
					
					<!-- Audio File -->
					<div class="data-row">
						<div class="file-name">{benchmark.audioFileName}</div>
					</div>
					
					<!-- File Size -->
					<div class="data-row">
						<div class="file-size">{formatFileSize(benchmark.audioFileSizeMb)}</div>
					</div>
				</div>
			{/each}
		</div>
		
		<!-- Actions -->
		<div class="comparison-actions">
			<button class="export-btn">📊 Export Vergleich</button>
			<button class="close-btn-secondary" on:click={onClose}>Schließen</button>
		</div>
	</div>
</div>

<style>
	.comparison-overlay {
		position: fixed;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background: rgba(0, 0, 0, 0.5);
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 1000;
		padding: 2rem;
	}
	
	.comparison-modal {
		background: white;
		border-radius: 12px;
		max-width: 1200px;
		width: 100%;
		max-height: 90vh;
		overflow-y: auto;
		box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
	}
	
	.comparison-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 1.5rem 2rem;
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		background: var(--bg-secondary, #f8fafc);
		border-radius: 12px 12px 0 0;
	}
	
	.comparison-header h2 {
		margin: 0;
		color: var(--text-primary, #111827);
		font-size: 1.5rem;
		font-weight: 700;
	}
	
	.close-btn {
		background: none;
		border: none;
		font-size: 1.25rem;
		cursor: pointer;
		padding: 0.5rem;
		border-radius: 6px;
		transition: background-color 0.2s ease;
	}
	
	.close-btn:hover {
		background: var(--error-bg, #fef2f2);
	}
	
	.comparison-stats {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: 1.5rem;
		padding: 2rem;
		background: var(--bg-primary, #ffffff);
	}
	
	.stat-card {
		display: flex;
		align-items: center;
		gap: 1rem;
		padding: 1.5rem;
		border-radius: 8px;
		border: 1px solid var(--border-color, #e5e7eb);
		transition: all 0.2s ease;
	}
	
	.stat-card:hover {
		transform: translateY(-2px);
		box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
	}
	
	.stat-card.fastest {
		border-color: var(--success-color, #10b981);
		background: var(--success-bg, #ecfdf5);
	}
	
	.stat-card.performance {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.stat-card.count {
		border-color: var(--warning-color, #f59e0b);
		background: var(--warning-bg, #fffbeb);
	}
	
	.stat-icon {
		font-size: 2rem;
		line-height: 1;
	}
	
	.stat-content {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.stat-label {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.stat-value {
		font-size: 1.125rem;
		font-weight: 700;
		color: var(--text-primary, #111827);
	}
	
	.stat-detail {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	/* ✅ Column-based Comparison Layout */
	.comparison-columns {
		display: flex;
		margin: 0 2rem 2rem 2rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		overflow: hidden;
		background: white;
		min-height: 600px;
	}
	
	.labels-column {
		flex: 0 0 200px;
		background: var(--bg-secondary, #f8fafc);
		border-right: 2px solid var(--border-color, #e5e7eb);
		display: flex;
		flex-direction: column;
	}
	
	.label-header {
		padding: 1rem;
		background: var(--primary-color, #2563eb);
		color: white;
		font-weight: 700;
		font-size: 0.875rem;
		text-align: center;
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		height: 60px; /* Same height as model headers */
		box-sizing: border-box;
		display: flex;
		align-items: center;
		justify-content: center;
	}
	
	.label-row {
		padding: 1rem;
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		font-weight: 600;
		font-size: 0.875rem;
		color: var(--text-primary, #111827);
		background: var(--bg-secondary, #f8fafc);
		display: flex;
		align-items: center;
		height: 60px; /* Fixed height to match data rows */
		box-sizing: border-box;
	}
	
	.model-column {
		flex: 1;
		border-right: 1px solid var(--border-color, #e5e7eb);
		display: flex;
		flex-direction: column;
		position: relative;
		transition: all 0.2s ease;
	}
	
	.model-column:last-child {
		border-right: none;
	}
	
	.model-column.fastest {
		background: var(--success-bg, #ecfdf5);
		border-top: 3px solid var(--success-color, #10b981);
	}
	
	.model-column.best-performance {
		background: var(--primary-bg, #eff6ff);
		border-top: 3px solid var(--primary-color, #2563eb);
	}
	
	.model-header {
		padding: 1rem;
		background: var(--bg-primary, #ffffff);
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		text-align: center;
		position: relative;
		height: 60px; /* Same height as data rows and labels */
		box-sizing: border-box;
		display: flex;
		align-items: center;
		justify-content: center;
	}
	
	.model-column.fastest .model-header {
		background: var(--success-bg, #ecfdf5);
	}
	
	.model-column.best-performance .model-header {
		background: var(--primary-bg, #eff6ff);
	}
	
	.model-title {
		font-weight: 700;
		font-size: 1.125rem;
		color: var(--text-primary, #111827);
		margin-bottom: 0.5rem;
	}
	
	/* Badges removed - only column color coding remains */
	
	.data-row {
		padding: 1rem;
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		text-align: center;
		height: 60px; /* Fixed height instead of min-height */
		box-sizing: border-box;
		transition: background-color 0.2s ease;
		overflow: hidden; /* Prevent content from expanding height */
	}
	
	.data-row:hover {
		background: rgba(0, 0, 0, 0.02);
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
	
	.date-main {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.benchmark-id {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-family: monospace;
		margin-top: 2px;
	}
	
	.total-time {
		font-weight: 600;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.time-range, .time-single {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		margin-top: 2px;
	}
	
	.performance-score {
		font-weight: 600;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.cpu-usage {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		margin-top: 2px;
	}
	
	.ram-usage {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.gpu-indicator {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		margin-top: 2px;
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
		margin-top: 2px;
	}
	
	.no-data {
		color: var(--text-secondary, #6b7280);
		font-style: italic;
		font-size: 0.875rem;
	}
	
	.comparison-actions {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 1.5rem 2rem;
		border-top: 1px solid var(--border-color, #e5e7eb);
		background: var(--bg-secondary, #f8fafc);
		border-radius: 0 0 12px 12px;
	}
	
	.export-btn {
		padding: 0.75rem 1.5rem;
		background: var(--primary-color, #2563eb);
		color: white;
		border: none;
		border-radius: 6px;
		font-size: 0.875rem;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.export-btn:hover {
		background: var(--primary-hover, #1d4ed8);
		transform: translateY(-1px);
	}
	
	.close-btn-secondary {
		padding: 0.75rem 1.5rem;
		background: var(--bg-primary, #ffffff);
		color: var(--text-primary, #111827);
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		font-size: 0.875rem;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.close-btn-secondary:hover {
		background: var(--bg-secondary, #f8fafc);
		border-color: var(--primary-color, #2563eb);
	}
	
	@media (max-width: 1200px) {
		.comparison-columns {
			flex-direction: column;
			margin: 0 1rem 2rem 1rem;
		}
		
		.labels-column {
			flex: none;
			display: none; /* Hide labels on mobile, use model headers instead */
		}
		
		.model-column {
			border-right: none;
			border-bottom: 2px solid var(--border-color, #e5e7eb);
			margin-bottom: 1rem;
		}
		
		.model-column:last-child {
			border-bottom: none;
			margin-bottom: 0;
		}
		
		.data-row {
			padding: 0.75rem;
			height: 50px; /* Fixed height for tablets */
		}
		
		.model-header {
			padding: 0.75rem;
			height: 50px; /* Same as data rows on tablets */
		}
		
		.model-title {
			font-size: 1rem;
		}
	}
	
	@media (max-width: 768px) {
		.comparison-overlay {
			padding: 0.5rem;
		}
		
		.comparison-modal {
			max-height: 98vh;
			border-radius: 8px;
		}
		
		.comparison-stats {
			grid-template-columns: 1fr;
			gap: 1rem;
			padding: 1rem;
		}
		
		.comparison-columns {
			margin: 0 1rem 1rem 1rem;
		}
		
		.model-column {
			margin-bottom: 1.5rem;
		}
		
		.data-row {
			padding: 0.5rem;
			height: 40px; /* Fixed height for mobile */
			font-size: 0.875rem;
		}
		
		.model-header {
			padding: 0.5rem;
			height: 40px; /* Same as data rows on mobile */
		}
		
		.model-title {
			font-size: 0.875rem;
		}
		
		/* Badges removed */
		
		.comparison-actions {
			flex-direction: column;
			gap: 1rem;
			align-items: stretch;
			padding: 1rem;
		}
	}
</style>
