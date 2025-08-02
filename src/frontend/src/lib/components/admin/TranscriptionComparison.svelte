<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { onMount } from 'svelte';

	// Props für die Komponente
	export let selectedBenchmarks: any[] = [];
	export let onClose: () => void = () => {};

	// Lokale State-Variablen
	let comparisonData: any[] = [];
	let isLoading = false;
	let showStatistics = true;

	// Reactive Statements
	$: if (selectedBenchmarks.length > 0) {
		prepareComparisonData();
	}

	/**
	 * Bereitet die Vergleichsdaten für die Anzeige vor [AIU]
	 */
	function prepareComparisonData() {
		comparisonData = selectedBenchmarks.map((benchmark, index) => ({
			id: benchmark.benchmarkId,
			modelName: benchmark.fastestModel || `Model ${index + 1}`,
			transcribedText: benchmark.transcribedText || '[NO_TRANSCRIPTION]',
			confidenceScore: benchmark.confidenceScore || 0,
			processingTime: benchmark.totalTimeMs || 0,
			timestamp: benchmark.timestamp,
			isLegacy: benchmark.transcribedText?.includes('[LEGACY_ENTRY_NO_TRANSCRIPTION]') || false,
			wordCount: countWords(benchmark.transcribedText || ''),
			characterCount: (benchmark.transcribedText || '').length
		}));
	}

	/**
	 * Zählt Wörter in einem Text [WMM]
	 */
	function countWords(text: string): number {
		if (!text || text.includes('[') || text === '') return 0;
		return text.trim().split(/\s+/).filter(word => word.length > 0).length;
	}

	/**
	 * Berechnet Ähnlichkeit zwischen zwei Texten (einfache Implementierung) [WMM]
	 */
	function calculateSimilarity(text1: string, text2: string): number {
		if (!text1 || !text2 || text1.includes('[') || text2.includes('[')) return 0;
		
		const words1 = text1.toLowerCase().split(/\s+/);
		const words2 = text2.toLowerCase().split(/\s+/);
		
		const commonWords = words1.filter(word => words2.includes(word));
		const totalWords = Math.max(words1.length, words2.length);
		
		return totalWords > 0 ? (commonWords.length / totalWords) * 100 : 0;
	}

	/**
	 * Exportiert Vergleich als Text-Datei [ATV]
	 */
	function exportComparison() {
		const exportData = comparisonData.map(item => 
			`=== ${item.modelName} ===\n` +
			`Konfidenz: ${(item.confidenceScore * 100).toFixed(1)}%\n` +
			`Verarbeitungszeit: ${item.processingTime}ms\n` +
			`Wörter: ${item.wordCount}\n` +
			`Zeichen: ${item.characterCount}\n` +
			`Text: ${item.transcribedText}\n\n`
		).join('');

		const blob = new Blob([exportData], { type: 'text/plain;charset=utf-8' });
		const url = URL.createObjectURL(blob);
		const a = document.createElement('a');
		a.href = url;
		a.download = `transcription-comparison-${new Date().toISOString().split('T')[0]}.txt`;
		document.body.appendChild(a);
		a.click();
		document.body.removeChild(a);
		URL.revokeObjectURL(url);
	}

	/**
	 * Kopiert Text in die Zwischenablage [UX]
	 */
	async function copyToClipboard(text: string) {
		try {
			await navigator.clipboard.writeText(text);
			// Hier könnte eine Toast-Benachrichtigung hinzugefügt werden
		} catch (err) {
			console.error('Failed to copy text:', err);
		}
	}

	onMount(() => {
		prepareComparisonData();
	});
</script>

<!-- Hauptcontainer -->
<div class="transcription-comparison">
	<!-- Header mit Aktionen -->
	<div class="comparison-header">
		<div class="header-left">
			<h2>Transkriptions-Vergleich</h2>
			<p class="subtitle">{comparisonData.length} Modelle werden verglichen</p>
		</div>
		<div class="header-actions">
			<label class="toggle-label">
				<input type="checkbox" bind:checked={showStatistics} />
				<span>Statistiken anzeigen</span>
			</label>
			<button class="export-btn" on:click={exportComparison}>
				📄 Exportieren
			</button>
			<button class="close-btn" on:click={onClose}>
				✕ Schließen
			</button>
		</div>
	</div>

	<!-- Side-by-Side Textvergleich -->
	<div class="side-by-side-comparison">
		{#each comparisonData as item, index}
			<div class="model-column" class:legacy={item.isLegacy}>
				<!-- Column Header -->
				<div class="column-header">
					<div class="header-top">
						<h3 class="model-title">{item.modelName}</h3>
						<button class="copy-btn-header" on:click={() => copyToClipboard(item.transcribedText)}>
							📋
						</button>
					</div>
					<div class="model-meta">
						<span class="confidence-badge" class:high={item.confidenceScore > 0.8} class:medium={item.confidenceScore > 0.5 && item.confidenceScore <= 0.8} class:low={item.confidenceScore <= 0.5}>
							{(item.confidenceScore * 100).toFixed(1)}% Konfidenz
						</span>
						<span class="processing-badge">
							{item.processingTime}ms
						</span>
					</div>
				</div>

				<!-- Statistics (optional) -->
				{#if showStatistics}
					<div class="column-stats">
						<div class="stat-item">
							<span class="stat-number">{item.wordCount}</span>
							<span class="stat-label">Wörter</span>
						</div>
						<div class="stat-item">
							<span class="stat-number">{item.characterCount}</span>
							<span class="stat-label">Zeichen</span>
						</div>
						{#if index > 0 && !item.isLegacy && !comparisonData[0].isLegacy}
							<div class="stat-item similarity-stat">
								<span class="stat-number">{calculateSimilarity(comparisonData[0].transcribedText, item.transcribedText).toFixed(1)}%</span>
								<span class="stat-label">Ähnlichkeit</span>
							</div>
						{/if}
					</div>
				{/if}

				<!-- Transcribed Text -->
				<div class="text-area">
					<div class="text-display" class:legacy-text={item.isLegacy}>
						{#if item.isLegacy}
							<div class="placeholder-message legacy">
								<div class="placeholder-icon">⚠️</div>
								<div class="placeholder-text">Legacy-Eintrag ohne Transkription</div>
							</div>
						{:else if item.transcribedText === '[NO_TRANSCRIPTION]'}
							<div class="placeholder-message no-data">
								<div class="placeholder-icon">ℹ️</div>
								<div class="placeholder-text">Keine Transkription verfügbar</div>
							</div>
						{:else}
							<div class="actual-text">{item.transcribedText}</div>
						{/if}
					</div>
				</div>

				<!-- Footer -->
				<div class="column-footer">
					<span class="timestamp-small">
						{new Date(item.timestamp).toLocaleString('de-CH')}
					</span>
				</div>
			</div>
		{/each}
	</div>


</div>

<style>
	.transcription-comparison {
		background: white;
		border-radius: 12px;
		box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
		padding: 24px;
		max-width: 1400px;
		margin: 0 auto;
		font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	}

	/* Header Styles */
	.comparison-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 24px;
		padding-bottom: 16px;
		border-bottom: 2px solid #e5e7eb;
	}

	.header-left h2 {
		margin: 0;
		color: #1f2937;
		font-size: 24px;
		font-weight: 600;
	}

	.subtitle {
		margin: 4px 0 0 0;
		color: #6b7280;
		font-size: 14px;
	}

	.header-actions {
		display: flex;
		align-items: center;
		gap: 16px;
	}

	.toggle-label {
		display: flex;
		align-items: center;
		gap: 8px;
		font-size: 14px;
		color: #374151;
		cursor: pointer;
	}

	.export-btn, .close-btn {
		padding: 8px 16px;
		border: none;
		border-radius: 6px;
		font-size: 14px;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s;
	}

	.export-btn {
		background: #3b82f6;
		color: white;
	}

	.export-btn:hover {
		background: #2563eb;
	}

	.close-btn {
		background: #ef4444;
		color: white;
	}

	.close-btn:hover {
		background: #dc2626;
	}

	/* Side-by-Side Comparison Layout - HORIZONTAL */
	.side-by-side-comparison {
		display: flex;
		flex-direction: row;
		gap: 16px;
		min-height: 500px;
		overflow-x: auto;
		padding: 8px;
		align-items: stretch;
	}

	/* Model Column Layout - VERTICAL WITHIN EACH COLUMN */
	.model-column {
		flex: 1;
		min-width: 300px;
		max-width: 500px;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		display: flex;
		flex-direction: column;
		box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
		transition: all 0.2s;
	}

	.model-column:hover {
		box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
	}

	.model-column.legacy {
		border-color: #fbbf24;
		background: #fffbeb;
	}

	/* Column Header */
	.column-header {
		padding: 16px 20px;
		border-bottom: 1px solid #e5e7eb;
		background: #f9fafb;
		border-radius: 12px 12px 0 0;
	}

	.header-top {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 8px;
	}

	.model-title {
		margin: 0;
		font-size: 18px;
		font-weight: 600;
		color: #1f2937;
	}

	.copy-btn-header {
		padding: 6px 8px;
		border: 1px solid #d1d5db;
		border-radius: 6px;
		background: white;
		font-size: 12px;
		cursor: pointer;
		transition: all 0.2s;
		box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
	}

	.copy-btn-header:hover {
		background: #f3f4f6;
		border-color: #9ca3af;
	}

	.model-meta {
		display: flex;
		gap: 8px;
		flex-wrap: wrap;
	}

	.confidence-badge {
		padding: 4px 8px;
		border-radius: 6px;
		font-size: 11px;
		font-weight: 500;
		text-transform: uppercase;
	}

	.confidence-badge.high {
		background: #d1fae5;
		color: #065f46;
	}

	.confidence-badge.medium {
		background: #fef3c7;
		color: #92400e;
	}

	.confidence-badge.low {
		background: #fee2e2;
		color: #991b1b;
	}

	.processing-badge {
		padding: 4px 8px;
		border-radius: 6px;
		background: #e5e7eb;
		color: #6b7280;
		font-size: 11px;
		font-weight: 500;
	}

	/* Column Statistics */
	.column-stats {
		display: flex;
		justify-content: space-around;
		padding: 12px 16px;
		background: #f9fafb;
		border-bottom: 1px solid #e5e7eb;
	}

	.stat-item {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 2px;
	}

	.stat-number {
		font-size: 16px;
		font-weight: 600;
		color: #1f2937;
	}

	.stat-label {
		font-size: 11px;
		color: #6b7280;
		font-weight: 500;
		text-transform: uppercase;
	}

	.similarity-stat .stat-number {
		color: #059669;
	}

	/* Text Area */
	.text-area {
		flex: 1;
		display: flex;
		flex-direction: column;
	}

	.text-display {
		flex: 1;
		padding: 16px;
		overflow-y: auto;
		min-height: 200px;
	}

	.actual-text {
		line-height: 1.6;
		color: #1f2937;
		font-size: 14px;
		white-space: pre-wrap;
		word-wrap: break-word;
	}

	.placeholder-message {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: 8px;
		height: 100%;
		min-height: 150px;
		text-align: center;
	}

	.placeholder-icon {
		font-size: 24px;
	}

	.placeholder-text {
		font-size: 14px;
		font-style: italic;
	}

	.placeholder-message.legacy .placeholder-text {
		color: #d97706;
	}

	.placeholder-message.no-data .placeholder-text {
		color: #6b7280;
	}

	/* Column Footer */
	.column-footer {
		padding: 12px 16px;
		border-top: 1px solid #e5e7eb;
		background: #f9fafb;
		border-radius: 0 0 12px 12px;
	}

	.timestamp-small {
		font-size: 11px;
		color: #9ca3af;
	}



	/* Responsive Design */
	@media (max-width: 768px) {
		.comparison-header {
			flex-direction: column;
			align-items: flex-start;
			gap: 16px;
		}

		.header-actions {
			flex-wrap: wrap;
		}

		.comparison-grid {
			grid-template-columns: 1fr;
		}

		.side-by-side-grid {
			grid-template-columns: 1fr;
		}
	}
</style>
