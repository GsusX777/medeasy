<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { onMount } from 'svelte';
	import AdminLayout from "$lib/components/admin/AdminLayout.svelte";
	import BenchmarkRunnerTabs from '$lib/components/admin/BenchmarkRunnerTabs.svelte';
	import BenchmarkResults from "$lib/components/admin/BenchmarkResults.svelte";
	import BenchmarkHistory from "$lib/components/admin/BenchmarkHistory.svelte";
	import TranscriptionComparison from "$lib/components/admin/TranscriptionComparison.svelte";
	// import LivePerformanceMonitor from "$lib/components/admin/LivePerformanceMonitor.svelte"; // DEAKTIVIERT für Performance-Tests
	import { AI_ENDPOINTS, ADMIN_ENDPOINTS, apiRequest, uploadFile, validateApiResponse } from "$lib/config/api";
	
	// [WMM] Whisper Multi-Model benchmarking
	// [ATV] Audit trail for benchmark operations
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
	
	let currentBenchmark: BenchmarkResult | null = null;
	let benchmarkHistory: BenchmarkResult[] = [];
	let isRunning = false;
	let availableModels: string[] = ['small', 'medium', 'large-v3'];
	let hardwareInfo: HardwareInfo | null = null;
	
	// [NEW] State for TranscriptionComparison component
	let showTranscriptionComparison = false;
	let selectedBenchmarksForComparison: any[] = [];
	
	onMount(async () => {
		await loadHardwareInfo();
		await loadBenchmarkHistory();
	});
	
	async function loadHardwareInfo() {
		try {
			const response = await apiRequest(AI_ENDPOINTS.WHISPER_HARDWARE_INFO);
			if (response.ok) {
				const data = await response.json();
				hardwareInfo = data.hardwareInfo;
			}
		} catch (error) {
			console.error('Failed to load hardware info:', error);
		}
	}
	
	async function loadBenchmarkHistory() {
		try {
			// Load real benchmark history from backend [ATV][PSF]
			const response = await apiRequest(ADMIN_ENDPOINTS.BENCHMARK_HISTORY);
			
			if (response.ok) {
				const data = await response.json();
				
				// ✅ Pass backend data directly to BenchmarkHistory component
				// Backend API already provides correct field names and structure
				benchmarkHistory = data.history.map((item: any) => ({
					// ✅ Core fields - pass through directly from backend
					benchmarkId: item.benchmarkId,
					requestId: item.requestId || item.benchmarkId,
					timestamp: item.timestamp,
					status: item.status, // ✅ Critical: was missing!
					
					// ✅ Model information
					modelsCount: item.modelsCount,
					fastestModel: item.fastestModel, // ✅ Critical: was missing!
					fastestTimeMs: item.fastestTimeMs,
					slowestModel: item.slowestModel,
					slowestTimeMs: item.slowestTimeMs,
					
					// ✅ Performance data
					totalTimeMs: item.totalTimeMs, // ✅ Correct field name!
					performanceScore: item.performanceScore, // ✅ Critical: was missing!
					confidenceScore: item.confidenceScore,
					averageRamUsageMb: item.averageRamUsageMb,
					cpuUsage: item.cpuUsage, // ✅ Critical: was missing!
					ramUsage: item.ramUsage, // ✅ Critical: was missing!
					cudaUsed: item.cudaUsed,
					
					// [NEW] Transcribed text for comparison and display
					transcribedText: item.transcribedText,
					
					// ✅ Audio file information
					audioFileName: item.audioFileName, // ✅ Critical: was missing!
					audioFileSizeMb: item.audioFileSizeMb,
					
					// ✅ Additional data for compatibility
					results: item.results || []
				}));
				
				console.log('✅ Benchmark history loaded:', benchmarkHistory.length, 'entries');
			} else {
				console.error('Failed to load benchmark history:', response.status, response.statusText);
				// Fallback to empty array instead of mock data
				benchmarkHistory = [];
			}
		} catch (error) {
			console.error('Failed to load benchmark history:', error);
			// Fallback to empty array instead of mock data
			benchmarkHistory = [];
		}
	}
	
	async function handleRunBenchmark(event: CustomEvent<{
		models: string[];
		audioFile?: File;
		audioRecordId?: string;
		iterations: number;
		testType: 'upload' | 'record' | 'chunk' | 'live';
	}>) {
		const { models, audioFile, audioRecordId, iterations, testType } = event.detail;
		
		// [PSF] Validate input before processing
		if (!audioFile && !audioRecordId) {
			console.error('❌ Benchmark failed: No audio source selected');
			alert('Bitte wählen Sie eine Audio-Datei oder -Aufnahme aus, bevor Sie den Benchmark starten.');
			return;
		}
		
		if (models.length === 0) {
			console.error('❌ Benchmark failed: No models selected');
			alert('Bitte wählen Sie mindestens ein Whisper-Modell für den Benchmark aus.');
			return;
		}
		
		console.log('🚀 Starting benchmark with:', {
			testType: testType,
			fileName: audioFile?.name || `AudioRecord-${audioRecordId}`,
			fileSize: audioFile ? `${(audioFile.size / 1024 / 1024).toFixed(2)} MB` : 'N/A',
			audioRecordId: audioRecordId,
			models: models,
			iterations: iterations
		});
		
		isRunning = true;
		currentBenchmark = null;
		
		try {
			let response: Response;
			
			if (testType === 'upload' && audioFile) {
				// File upload benchmark
				response = await uploadFile(
					AI_ENDPOINTS.WHISPER_BENCHMARK,
					audioFile,
					{ 
						modelsToTest: models.join(','),
						iterations: iterations.toString()
					}
				);
			} else if (testType === 'record' && audioRecordId) {
				// Audio record benchmark
				response = await fetch(`${AI_ENDPOINTS.WHISPER_BENCHMARK}/record`, {
					method: 'POST',
					headers: {
						'Content-Type': 'application/json'
					},
					body: JSON.stringify({
						audioRecordId: audioRecordId,
						modelsToTest: models.join(','),
						iterations: iterations.toString()
					})
				});
			} else if (testType === 'chunk') {
				// Chunk-based benchmark [WMM][PSF][UX]
				// Support both AudioRecord and File Upload modes
				
				if (audioRecordId) {
					// AudioRecord Mode: Use JSON [Existing]
					response = await fetch(AI_ENDPOINTS.WHISPER_BENCHMARK_CHUNK, {
						method: 'POST',
						headers: {
							'Content-Type': 'application/json'
						},
						body: JSON.stringify({
							audioRecordId: audioRecordId,
							modelsToTest: models.join(','),
							iterations: iterations,
							chunkSettings: {
								chunkSizeSeconds: 2,
								overlapMs: 500,
								testMode: 'sequential',
								audioSource: 'record'
							}
						})
					});
				} else if (audioFile) {
					// File Upload Mode: Use multipart/form-data [UX][Bugfix]
					const formData = new FormData();
					formData.append('audioFile', audioFile);
					formData.append('modelsToTest', models.join(','));
					formData.append('iterations', iterations.toString());
					formData.append('chunkSizeSeconds', '2');
					formData.append('overlapMs', '500');
					
					response = await fetch(AI_ENDPOINTS.WHISPER_BENCHMARK_CHUNK, {
						method: 'POST',
						// No Content-Type header - let browser set it for multipart/form-data
						body: formData
					});
				} else {
					throw new Error('Chunk test requires either AudioRecord ID or uploaded file');
				}
			} else {
				throw new Error(`Unsupported test type: ${testType}`);
			}
			
			if (!response.ok) {
				throw new Error(`HTTP ${response.status}: ${response.statusText}`);
			}
			
			const result = await validateApiResponse<any>(response);
			currentBenchmark = {
				...(result as any),
				timestamp: new Date().toISOString()
			};
			
			// [ATV] Log benchmark completion
			console.log('Benchmark completed successfully', {
				benchmarkId: (result as any).benchmarkId,
				models: models,
				totalTime: result.totalBenchmarkTimeMs
			});
			
			// ✅ Fix Reactivity: Reload benchmark history from backend
			// This ensures the new benchmark data is properly loaded and reactive
			await loadBenchmarkHistory();
			
			console.log('✅ Benchmark history reloaded after completion:', benchmarkHistory.length, 'entries');
			
		} catch (error) {
			console.error('❌ Benchmark failed:', error);
			
			// [PSF] Show user-friendly error message
			let errorMessage = 'Benchmark-Test fehlgeschlagen.';
			if (error instanceof Error) {
				if (error.message.includes('400')) {
					errorMessage = 'Ungültige Audio-Datei oder Parameter. Bitte prüfen Sie Ihre Eingaben.';
				} else if (error.message.includes('500')) {
					errorMessage = 'Server-Fehler beim Benchmark-Test. Bitte versuchen Sie es später erneut.';
				} else if (error.message.includes('timeout')) {
					errorMessage = 'Benchmark-Test dauerte zu lange. Versuchen Sie eine kleinere Audio-Datei.';
				}
			}
			
			alert(`❌ ${errorMessage}\n\nDetails: ${error}`);
			
			// [ATV] Log detailed error for debugging
			console.error('Detailed benchmark error:', {
				error: error,
				models: models,
				audioFile: audioFile?.name,
				fileSize: audioFile?.size
			});
		} finally {
			isRunning = false;
		}
	}
	
	function clearCurrentBenchmark() {
		currentBenchmark = null;
	}
	
	/**
	 * Opens transcription comparison with selected benchmarks [NEW]
	 */
	function openTranscriptionComparison(selectedBenchmarks: any[]) {
		selectedBenchmarksForComparison = selectedBenchmarks;
		showTranscriptionComparison = true;
	}
	
	/**
	 * Closes transcription comparison [NEW]
	 */
	function closeTranscriptionComparison() {
		showTranscriptionComparison = false;
		selectedBenchmarksForComparison = [];
	}
</script>

<AdminLayout>
	<div class="benchmarks-page">
		<header class="page-header">
			<div class="header-content">
				<h1>🧪 Whisper Benchmark Tests</h1>
				<p class="subtitle">
					Performance-Tests für alle Whisper-Modelle mit echten Audio-Dateien
				</p>
			</div>
			
			<div class="header-stats">
				{#if hardwareInfo}
					<div class="hardware-summary">
						<div class="hardware-item">
							<span class="hardware-label">CPU:</span>
							<span class="hardware-value">{hardwareInfo.cpuCores} Kerne</span>
						</div>
						<div class="hardware-item">
							<span class="hardware-label">RAM:</span>
							<span class="hardware-value">{hardwareInfo.ramGb || 'N/A'} GB</span>
						</div>
						<div class="hardware-item">
							<span class="hardware-label">GPU:</span>
							<span class="hardware-value">
								{hardwareInfo.cudaAvailable ? '✅ CUDA' : '❌ CPU-only'}
							</span>
						</div>
					</div>
				{/if}
			</div>
		</header>
		
		<div class="benchmarks-grid">
			<!-- Live Performance Monitor - Runs parallel to benchmarks [WMM][PSF] -->
			<div class="performance-section">
				<!-- <LivePerformanceMonitor /> DEAKTIVIERT für Performance-Tests -->
			</div>
			
			<!-- Benchmark Runner -->
			<div class="benchmark-section">
				<h2>🚀 Neuen Benchmark starten</h2>
				<BenchmarkRunnerTabs
					{availableModels}
					bind:isRunning
					on:runBenchmark={handleRunBenchmark}
				>
					<div slot="benchmark-history">
						<!-- Benchmark History - Always show section, even if empty -->
						<div class="section history-section">
							{#if benchmarkHistory.length > 0}
								<BenchmarkHistory 
									history={benchmarkHistory}
									on:compareTranscriptions={(e) => openTranscriptionComparison(e.detail.selectedBenchmarks)}
								/>
							{:else}
								<div class="empty-history">
									<div class="empty-icon">📈</div>
									<p>Noch keine Benchmark-Tests durchgeführt.</p>
									<p class="empty-hint">Starten Sie Ihren ersten Benchmark-Test, um die Ergebnisse hier zu sehen.</p>
								</div>
							{/if}
						</div>
					</div>
				</BenchmarkRunnerTabs>
			</div>
		</div>
		
		<!-- Transcription Comparison Modal -->
		{#if showTranscriptionComparison}
			<div class="comparison-overlay">
				<TranscriptionComparison 
					selectedBenchmarks={selectedBenchmarksForComparison}
					onClose={closeTranscriptionComparison}
				/>
			</div>
		{/if}
		
		<!-- Running Indicator -->
		{#if isRunning}
			<div class="running-overlay">
				<div class="running-modal">
					<div class="running-spinner"></div>
					<h3>🧪 Benchmark läuft...</h3>
					<p>Die Whisper-Modelle werden getestet. Dies kann einige Minuten dauern.</p>
					<div class="running-progress">
						<div class="progress-bar">
							<div class="progress-fill"></div>
						</div>
					</div>
				</div>
			</div>
		{/if}
	</div>
</AdminLayout>

<style>
	.benchmarks-page {
		padding: 2rem;
		max-width: 1400px;
		margin: 0 auto;
		position: relative;
	}
	
	.page-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		margin-bottom: 2rem;
		flex-wrap: wrap;
		gap: 1.5rem;
	}
	
	.header-content h1 {
		font-size: 2rem;
		color: var(--text-primary, #111827);
		margin-bottom: 0.5rem;
	}
	
	.subtitle {
		color: var(--text-secondary, #6b7280);
		font-size: 1rem;
		line-height: 1.5;
	}
	
	.hardware-summary {
		display: flex;
		gap: 1.5rem;
		background: white;
		padding: 1rem 1.5rem;
		border-radius: 8px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.hardware-item {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.25rem;
	}
	
	.hardware-label {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.hardware-value {
		font-size: 0.875rem;
		color: var(--text-primary, #111827);
		font-weight: 600;
	}
	
	.benchmarks-grid {
		display: flex;
		flex-direction: column;
		gap: 2rem;
	}
	
	.performance-section {
		/* Live Performance Monitor styling [WMM][PSF] */
		margin-bottom: 1rem;
	}
	
	.benchmark-section,
	.results-section,
	.history-section {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 12px;
		padding: 2rem;
	}
	
	.benchmark-section h2,
	.results-section h2,
	.history-section h2 {
		margin-bottom: 1.5rem;
		color: var(--text-primary, #111827);
	}
	
	.section-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 1.5rem;
	}
	
	.section-header h2 {
		margin-bottom: 0;
	}
	
	.clear-btn {
		padding: 0.5rem 1rem;
		border: 1px solid #dc2626;
		border-radius: 6px;
		background: white;
		color: #dc2626;
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.875rem;
	}
	
	.clear-btn:hover {
		background: #dc2626;
		color: white;
	}
	
	.running-overlay {
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
	}
	
	.running-modal {
		background: white;
		border-radius: 12px;
		padding: 3rem;
		text-align: center;
		max-width: 400px;
		width: 90%;
		box-shadow: 0 25px 50px rgba(0, 0, 0, 0.25);
	}
	
	.running-spinner {
		width: 48px;
		height: 48px;
		border: 4px solid var(--border-color, #e5e7eb);
		border-top: 4px solid var(--primary-color, #2563eb);
		border-radius: 50%;
		animation: spin 1s linear infinite;
		margin: 0 auto 1.5rem;
	}
	
	@keyframes spin {
		0% { transform: rotate(0deg); }
		100% { transform: rotate(360deg); }
	}
	
	.running-modal h3 {
		color: var(--text-primary, #111827);
		margin-bottom: 1rem;
	}
	
	.running-modal p {
		color: var(--text-secondary, #6b7280);
		margin-bottom: 2rem;
		line-height: 1.5;
	}
	
	.running-progress {
		width: 100%;
	}
	
	.progress-bar {
		width: 100%;
		height: 8px;
		background: var(--border-color, #e5e7eb);
		border-radius: 4px;
		overflow: hidden;
	}
	
	.progress-fill {
		height: 100%;
		background: var(--primary-color, #2563eb);
		width: 0%;
		animation: progress 3s ease-in-out infinite;
	}
	
	@keyframes progress {
		0% { width: 0%; }
		50% { width: 70%; }
		100% { width: 100%; }
	}
	
	/* Comparison Overlay Styles */
	.comparison-overlay {
		position: fixed;
		top: 0;
		left: 0;
		width: 100%;
		height: 100%;
		background: rgba(0, 0, 0, 0.8);
		z-index: 1000;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 20px;
		box-sizing: border-box;
		animation: fadeIn 0.3s ease-out;
	}
	
	@keyframes fadeIn {
		from { opacity: 0; }
		to { opacity: 1; }
	}
	
	/* Empty History Styles */
	.empty-history {
		text-align: center;
		padding: 3rem 2rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.empty-icon {
		font-size: 3rem;
		margin-bottom: 1rem;
		opacity: 0.6;
	}
	
	.empty-history p {
		margin: 0.5rem 0;
		font-size: 1rem;
	}
	
	.empty-hint {
		font-size: 0.9rem !important;
		opacity: 0.8;
		font-style: italic;
	}
	
	@media (max-width: 768px) {
		.benchmarks-page {
			padding: 1rem;
		}
		
		.page-header {
			flex-direction: column;
			align-items: stretch;
		}
		
		.hardware-summary {
			flex-direction: column;
			gap: 1rem;
		}
		
		.hardware-item {
			flex-direction: row;
			justify-content: space-between;
		}
		
		.benchmark-section,
		.results-section,
		.history-section {
			padding: 1.5rem;
		}
		
		.running-modal {
			padding: 2rem;
		}
	}
</style>
