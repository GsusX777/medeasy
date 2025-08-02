<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	// [WMM] Whisper Multi-Model benchmark results display
	// [ATV] Audit trail for benchmark results
	// [PSF] Patient safety through performance validation
	
	interface BenchmarkResult {
		requestId: string;
		benchmarkId: string;
		timestamp: string;
		totalBenchmarkTimeMs: number;
		results: ModelBenchmarkResult[];
		hardwareInfo: HardwareInfo;
		recommendation: ModelRecommendation;
	}
	
	interface ModelBenchmarkResult {
		modelName: string;
		averageProcessingTimeMs: number;
		minProcessingTimeMs: number;
		maxProcessingTimeMs: number;
		averageAccuracy: number;
		averageConfidence: number;
		cudaUsed: boolean;
		averageCpuUsage: number;
		averageGpuUsage: number;
		averageRamUsageMb: number;
		averageVramUsageMb: number;
		performanceScore: number;
		accuracyScore: number;
		success: boolean;
		errorMessage: string;
	}
	
	interface HardwareInfo {
		cudaAvailable: boolean;
		vramGb: number;
		ramGb: number;
		cpuInfo: string | null;
		gpuInfo: string | null;
		cpuCores: number;
	}
	
	interface ModelRecommendation {
		recommendedForPerformance: string | null;
		recommendedForAccuracy: string | null;
		recommendedForBalance: string | null;
		reasoning: string | null;
		hardwareLimitations: string[];
	}
	
	export let benchmark: BenchmarkResult;
	
	function formatTime(ms: number): string {
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
	
	function getModelColor(modelName: string | undefined): string {
		if (!modelName) return '#6b7280';
		
		const colors: Record<string, string> = {
			'base': '#059669',
			'small': '#2563eb', 
			'medium': '#d97706',
			'large-v3': '#dc2626'
		};
		return colors[modelName] || '#6b7280';
	}
	
	function getPerformanceRating(timeMs: number): { rating: string; color: string } {
		if (timeMs < 10000) return { rating: 'Sehr schnell', color: '#059669' };
		if (timeMs < 20000) return { rating: 'Schnell', color: '#2563eb' };
		if (timeMs < 30000) return { rating: 'Mittel', color: '#d97706' };
		return { rating: 'Langsam', color: '#dc2626' };
	}
	
	// ✅ Fix TypeError: Add null checks for benchmark.results
	$: sortedResults = (benchmark?.results || [])
		.filter(r => r.success)
		.sort((a, b) => a.averageProcessingTimeMs - b.averageProcessingTimeMs);
	
	$: failedResults = (benchmark?.results || []).filter(r => !r.success);
	
	$: fastestModel = sortedResults[0];
	$: slowestModel = sortedResults[sortedResults.length - 1];
</script>

<div class="benchmark-results">
	<!-- Results Header -->
	<div class="results-header">
		<div class="header-info">
			<h3>📊 Benchmark-Ergebnisse</h3>
			<div class="benchmark-meta">
				<span class="benchmark-id">ID: {benchmark?.benchmarkId || 'N/A'}</span>
				<span class="benchmark-time">{benchmark?.timestamp ? formatTimestamp(benchmark.timestamp) : 'N/A'}</span>
				<span class="total-time">
					Gesamt: {benchmark?.totalBenchmarkTimeMs ? formatTime(benchmark.totalBenchmarkTimeMs) : 'N/A'}
				</span>
			</div>
		</div>
	</div>
	
	<!-- Quick Stats -->
	<div class="quick-stats">
		<div class="stat-card">
			<div class="stat-icon">🏆</div>
			<div class="stat-content">
				<span class="stat-label">Schnellstes Modell</span>
				<span class="stat-value" style="color: {getModelColor(fastestModel?.modelName)}">
					{fastestModel?.modelName || 'N/A'}
				</span>
				<span class="stat-detail">
					{fastestModel ? formatTime(fastestModel.averageProcessingTimeMs) : 'N/A'}
				</span>
			</div>
		</div>
		
		<div class="stat-card">
			<div class="stat-icon">🐌</div>
			<div class="stat-content">
				<span class="stat-label">Langsamstes Modell</span>
				<span class="stat-value" style="color: {getModelColor(slowestModel?.modelName)}">
					{slowestModel?.modelName || 'N/A'}
				</span>
				<span class="stat-detail">
					{slowestModel ? formatTime(slowestModel.averageProcessingTimeMs) : 'N/A'}
				</span>
			</div>
		</div>
		
		<div class="stat-card">
			<div class="stat-icon">📈</div>
			<div class="stat-content">
				<span class="stat-label">Getestete Modelle</span>
				<span class="stat-value">{(benchmark?.results || []).length}</span>
				<span class="stat-detail">
					{(benchmark?.results || []).filter(r => r.success).length} erfolgreich
				</span>
			</div>
		</div>
		
		<div class="stat-card">
			<div class="stat-icon">💾</div>
			<div class="stat-content">
				<span class="stat-label">Durchschn. RAM</span>
				<span class="stat-value">
					{(benchmark?.results || []).length > 0 
						? Math.round((benchmark.results || []).reduce((sum, r) => sum + r.averageRamUsageMb, 0) / (benchmark.results || []).length)
						: 0}MB
				</span>
				<span class="stat-detail">Speicherverbrauch</span>
			</div>
		</div>
	</div>
	
	<!-- Detailed Results -->
	<div class="detailed-results">
		<h4>🔍 Detaillierte Ergebnisse</h4>
		
		{#if sortedResults.length > 0}
			<div class="results-grid">
				{#each sortedResults as result, index}
					{@const performance = getPerformanceRating(result.averageProcessingTimeMs)}
					<div class="result-card" style="border-left-color: {getModelColor(result.modelName)}">
						<div class="result-header">
							<div class="model-info">
								<span class="model-name" style="color: {getModelColor(result.modelName)}">
									{result.modelName}
								</span>
								<span class="model-rank">#{index + 1}</span>
							</div>
							<div class="performance-badge" style="background: {performance.color}20; color: {performance.color}">
								{performance.rating}
							</div>
						</div>
						
						<div class="result-metrics">
							<div class="metric">
								<span class="metric-label">⏱️ Verarbeitungszeit:</span>
								<span class="metric-value">{formatTime(result.averageProcessingTimeMs)}</span>
							</div>
							
							<div class="metric">
								<span class="metric-label">💾 RAM-Verbrauch:</span>
								<span class="metric-value">{result.averageRamUsageMb}MB</span>
							</div>
							
							<div class="metric">
								<span class="metric-label">🔄 CPU-Auslastung:</span>
								<span class="metric-value">{result.averageCpuUsage.toFixed(1)}%</span>
							</div>
							
							{#if result.averageConfidence > 0}
								<div class="metric">
									<span class="metric-label">🎯 Konfidenz:</span>
									<span class="metric-value">{(result.averageConfidence * 100).toFixed(1)}%</span>
								</div>
							{/if}
							
							<div class="metric">
								<span class="metric-label">📊 Performance-Score:</span>
								<span class="metric-value">{result.performanceScore.toFixed(1)}/100</span>
							</div>
						</div>
						
						<div class="time-range">
							<span class="time-range-label">Zeitbereich:</span>
							<span class="time-range-value">
								{formatTime(result.minProcessingTimeMs)} - {formatTime(result.maxProcessingTimeMs)}
							</span>
						</div>
					</div>
				{/each}
			</div>
		{/if}
		
		<!-- Failed Results -->
		{#if failedResults.length > 0}
			<div class="failed-results">
				<h5>❌ Fehlgeschlagene Tests</h5>
				{#each failedResults as result}
					<div class="failed-card">
						<span class="failed-model">{result.modelName}</span>
						<span class="failed-error">{result.errorMessage || 'Unbekannter Fehler'}</span>
					</div>
				{/each}
			</div>
		{/if}
	</div>
	
	<!-- Recommendations -->
	{#if benchmark?.recommendation}
		<div class="recommendations">
			<h4>💡 Empfehlungen</h4>
			
			<div class="recommendation-grid">
				{#if benchmark?.recommendation?.recommendedForPerformance}
					<div class="recommendation-card performance">
						<div class="rec-icon">🚀</div>
						<div class="rec-content">
							<span class="rec-title">Für Performance</span>
							<span class="rec-model">{benchmark?.recommendation?.recommendedForPerformance}</span>
						</div>
					</div>
				{/if}
				
				{#if benchmark?.recommendation?.recommendedForAccuracy}
					<div class="recommendation-card accuracy">
						<div class="rec-icon">🎯</div>
						<div class="rec-content">
							<span class="rec-title">Für Genauigkeit</span>
							<span class="rec-model">{benchmark?.recommendation?.recommendedForAccuracy}</span>
						</div>
					</div>
				{/if}
				
				{#if benchmark?.recommendation?.recommendedForBalance}
					<div class="recommendation-card balance">
						<div class="rec-icon">⚖️</div>
						<div class="rec-content">
							<span class="rec-title">Ausgewogen</span>
							<span class="rec-model">{benchmark?.recommendation?.recommendedForBalance}</span>
						</div>
					</div>
				{/if}
			</div>
			
			{#if benchmark.recommendation.reasoning}
				<div class="reasoning">
					<span class="reasoning-label">💭 Begründung:</span>
					<span class="reasoning-text">{benchmark.recommendation.reasoning}</span>
				</div>
			{/if}
			
			{#if benchmark.recommendation.hardwareLimitations.length > 0}
				<div class="limitations">
					<span class="limitations-label">⚠️ Hardware-Einschränkungen:</span>
					<ul class="limitations-list">
						{#each benchmark.recommendation.hardwareLimitations as limitation}
							<li>{limitation}</li>
						{/each}
					</ul>
				</div>
			{/if}
		</div>
	{/if}
	
	<!-- Hardware Info -->
	<div class="hardware-info">
		<h4>🖥️ Hardware-Informationen</h4>
		<div class="hardware-grid">
			<div class="hardware-item">
				<span class="hw-label">CPU:</span>
				<span class="hw-value">{benchmark?.hardwareInfo?.cpuInfo || 'N/A'}</span>
			</div>
			<div class="hardware-item">
				<span class="hw-label">CPU-Kerne:</span>
				<span class="hw-value">{benchmark?.hardwareInfo?.cpuCores || 'N/A'}</span>
			</div>
			<div class="hardware-item">
				<span class="hw-label">RAM:</span>
				<span class="hw-value">{benchmark?.hardwareInfo?.ramGb || 'N/A'} GB</span>
			</div>
			<div class="hardware-item">
				<span class="hw-label">GPU:</span>
				<span class="hw-value">
					{benchmark?.hardwareInfo?.cudaAvailable ? '✅ CUDA verfügbar' : '❌ CPU-only'}
				</span>
			</div>
		</div>
	</div>
</div>

<style>
	.benchmark-results {
		display: flex;
		flex-direction: column;
		gap: 1.5rem;
	}
	
	.results-header {
		border-bottom: 1px solid var(--border-color, #e5e7eb);
		padding-bottom: 1rem;
	}
	
	.header-info h3 {
		margin-bottom: 0.5rem;
		color: var(--text-primary, #111827);
	}
	
	.benchmark-meta {
		display: flex;
		gap: 1rem;
		flex-wrap: wrap;
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.benchmark-id,
	.benchmark-time,
	.total-time {
		padding: 0.25rem 0.5rem;
		background: var(--bg-secondary, #f8fafc);
		border-radius: 4px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.quick-stats {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 1rem;
	}
	
	.stat-card {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		padding: 1rem;
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
	}
	
	.stat-icon {
		font-size: 1.5rem;
	}
	
	.stat-content {
		display: flex;
		flex-direction: column;
		gap: 0.125rem;
	}
	
	.stat-label {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.stat-value {
		font-size: 1.125rem;
		font-weight: 600;
		color: var(--text-primary, #111827);
	}
	
	.stat-detail {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.detailed-results h4 {
		margin-bottom: 1rem;
		color: var(--text-primary, #111827);
	}
	
	.results-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
		gap: 1rem;
		margin-bottom: 1.5rem;
	}
	
	.result-card {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-left: 4px solid;
		border-radius: 8px;
		padding: 1.5rem;
	}
	
	.result-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 1rem;
	}
	
	.model-info {
		display: flex;
		align-items: center;
		gap: 0.5rem;
	}
	
	.model-name {
		font-size: 1.125rem;
		font-weight: 600;
	}
	
	.model-rank {
		background: var(--bg-secondary, #f8fafc);
		color: var(--text-secondary, #6b7280);
		padding: 0.25rem 0.5rem;
		border-radius: 4px;
		font-size: 0.75rem;
		font-weight: 500;
	}
	
	.performance-badge {
		padding: 0.25rem 0.75rem;
		border-radius: 4px;
		font-size: 0.75rem;
		font-weight: 500;
	}
	
	.result-metrics {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
		margin-bottom: 1rem;
	}
	
	.metric {
		display: flex;
		justify-content: space-between;
		align-items: center;
		font-size: 0.875rem;
	}
	
	.metric-label {
		color: var(--text-secondary, #6b7280);
	}
	
	.metric-value {
		font-weight: 500;
		color: var(--text-primary, #111827);
	}
	
	.time-range {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding-top: 0.75rem;
		border-top: 1px solid var(--border-color, #e5e7eb);
		font-size: 0.875rem;
	}
	
	.time-range-label {
		color: var(--text-secondary, #6b7280);
	}
	
	.time-range-value {
		font-weight: 500;
		color: var(--text-primary, #111827);
	}
	
	.failed-results {
		margin-top: 1.5rem;
		padding: 1rem;
		background: #fef2f2;
		border: 1px solid #fecaca;
		border-radius: 8px;
	}
	
	.failed-results h5 {
		margin-bottom: 0.75rem;
		color: #dc2626;
	}
	
	.failed-card {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.5rem;
		background: white;
		border-radius: 4px;
		margin-bottom: 0.5rem;
	}
	
	.failed-model {
		font-weight: 500;
		color: #dc2626;
	}
	
	.failed-error {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.recommendations {
		background: var(--bg-secondary, #f8fafc);
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		padding: 1.5rem;
	}
	
	.recommendations h4 {
		margin-bottom: 1rem;
		color: var(--text-primary, #111827);
	}
	
	.recommendation-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 1rem;
		margin-bottom: 1rem;
	}
	
	.recommendation-card {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		padding: 1rem;
		background: white;
		border-radius: 6px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.rec-icon {
		font-size: 1.25rem;
	}
	
	.rec-content {
		display: flex;
		flex-direction: column;
		gap: 0.125rem;
	}
	
	.rec-title {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.rec-model {
		font-weight: 600;
		color: var(--text-primary, #111827);
	}
	
	.reasoning,
	.limitations {
		margin-top: 1rem;
		padding: 1rem;
		background: white;
		border-radius: 6px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.reasoning-label,
	.limitations-label {
		font-weight: 500;
		color: var(--text-primary, #111827);
		margin-bottom: 0.5rem;
		display: block;
	}
	
	.reasoning-text {
		color: var(--text-secondary, #6b7280);
		line-height: 1.5;
	}
	
	.limitations-list {
		margin: 0;
		padding-left: 1.5rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.hardware-info {
		background: var(--bg-secondary, #f8fafc);
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		padding: 1.5rem;
	}
	
	.hardware-info h4 {
		margin-bottom: 1rem;
		color: var(--text-primary, #111827);
	}
	
	.hardware-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
		gap: 1rem;
	}
	
	.hardware-item {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.hw-label {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.hw-value {
		font-weight: 500;
		color: var(--text-primary, #111827);
	}
	
	@media (max-width: 768px) {
		.benchmark-meta {
			flex-direction: column;
			gap: 0.5rem;
		}
		
		.quick-stats {
			grid-template-columns: 1fr;
		}
		
		.results-grid {
			grid-template-columns: 1fr;
		}
		
		.recommendation-grid {
			grid-template-columns: 1fr;
		}
		
		.hardware-grid {
			grid-template-columns: repeat(2, 1fr);
		}
		
		.result-header {
			flex-direction: column;
			align-items: flex-start;
			gap: 0.5rem;
		}
	}
</style>
