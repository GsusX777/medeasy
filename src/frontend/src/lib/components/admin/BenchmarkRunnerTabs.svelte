<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { createEventDispatcher, onMount } from 'svelte';
	import FileUploadTab from './tabs/FileUploadTab.svelte';
	import AudioRecordTab from './tabs/AudioRecordTab.svelte';
	import ChunkTestTab from './tabs/ChunkTestTab.svelte';
	import LiveTranscriptionTab from './tabs/LiveTranscriptionTab.svelte';
	
	// [WMM] Whisper Multi-Model benchmark runner with tab system
	// [ATV] Audit trail for benchmark operations
	// [PSF] Patient safety through performance validation
	
	export let availableModels: string[];
	export let isRunning: boolean = false;
	
	const dispatch = createEventDispatcher<{
		runBenchmark: {
			models: string[];
			audioFile?: File;
			audioRecordId?: string;
			iterations: number;
			testType: 'upload' | 'record' | 'chunk' | 'live';
		};
	}>();
	
	// Tab management
	type TabType = 'upload' | 'record' | 'chunk' | 'live';
	let activeTab: TabType = 'upload';
	
	// Shared state across all tabs
	let selectedModels: string[] = ['small'];
	let iterations = 1;
	let selectedAudioRecord: string | null = null;
	let audioRecords: Array<{id: string, fileName: string, created: string, duration: number}> = [];
	
	// Tab-specific state
	let uploadFile: File | null = null;
	let recordedAudio: Blob | null = null;
	let isRecording = false;
	
	// Chunk test settings - bound to ChunkTestTab [WMM]
	let chunkSizeSeconds = 2;
	let overlapMs = 100;
	let audioSource: 'record' | 'upload' = 'record';
	let chunkSelectedAudioRecord: string | null = null;
	let chunkUploadFile: File | null = null;
	
	const tabs = [
		{ id: 'upload', label: 'File Upload', icon: '📁', enabled: true },
		{ id: 'record', label: 'Audio Record', icon: '🎙️', enabled: true },
		{ id: 'chunk', label: 'Chunk Test', icon: '🔄', enabled: true },
		{ id: 'live', label: 'Live Transcription', icon: '🔴', enabled: true } // ✅ ACTIVATED
	] as const;
	
	const modelInfo: Record<string, { size: string; ram: string; description: string }> = {
		'small': { size: '461MB', ram: '2GB', description: 'Ausgewogen, empfohlen' },
		'medium': { size: '1542MB', ram: '4GB', description: 'Langsamer, genauer' },
		'large-v3': { size: '3094MB', ram: '8GB', description: 'Langsamst, sehr genau' }
	};
	
	onMount(() => {
		loadAudioRecords();
	});
	
	async function loadAudioRecords() {
		try {
			// [ATV] Load real audio records from backend database
			const response = await fetch('http://localhost:5155/api/ai/audio/recordings');
			if (!response.ok) {
				throw new Error(`HTTP ${response.status}: ${response.statusText}`);
			}
			
			const records = await response.json();
			
			// Transform backend data to frontend format
			audioRecords = records.map((record: any) => ({
				id: record.id,
				fileName: record.fileName,
				created: record.created,
				duration: record.durationSeconds
			}));
			
			console.log(`Loaded ${audioRecords.length} audio records from database [ATV]`);
		} catch (error) {
			console.error('Failed to load audio records:', error);
			// Fallback to empty array on error
			audioRecords = [];
		}
	}
	
	function toggleModel(model: string) {
		if (selectedModels.includes(model)) {
			selectedModels = selectedModels.filter(m => m !== model);
		} else {
			selectedModels = [...selectedModels, model];
		}
	}
	
	function handleTabChange(tabId: TabType) {
		if (tabs.find(t => t.id === tabId)?.enabled) {
			activeTab = tabId;
		}
	}
	
	function handleFileUpload(event: CustomEvent<{file: File}>) {
		uploadFile = event.detail.file;
		selectedAudioRecord = null; // Clear record selection when file is uploaded
	}
	
	function handleAudioRecorded(event: CustomEvent<{audioBlob: Blob, fileName: string}>) {
		recordedAudio = event.detail.audioBlob;
		// TODO: Save to backend and refresh audio records list
		loadAudioRecords();
	}
	
	// handleChunkTestStarted entfernt - Chunk-Test wird über gemeinsamen runBenchmark() Button gestartet [UI-Vereinfachung]
	
	function runBenchmark() {
		if (selectedModels.length === 0) {
			alert('Bitte wählen Sie mindestens ein Modell aus.');
			return;
		}
		
		let benchmarkData: any = {
			models: selectedModels,
			iterations,
			testType: activeTab
		};
		
		if (activeTab === 'upload' && uploadFile) {
			benchmarkData.audioFile = uploadFile;
		} else if (activeTab === 'record' && selectedAudioRecord) {
			benchmarkData.audioRecordId = selectedAudioRecord;
		} else if (activeTab === 'chunk') {
			// Für Chunk-Test: Sammle Daten aus dem ChunkTestTab [WMM]
			if (chunkSelectedAudioRecord) {
				benchmarkData.audioRecordId = chunkSelectedAudioRecord;
			} else if (chunkUploadFile) {
				benchmarkData.audioFile = chunkUploadFile;
			} else {
				alert('Bitte wählen Sie eine Audio-Quelle für den Chunk-Test aus.');
				return;
			}
			
			// Echte Chunk-Einstellungen aus dem ChunkTestTab verwenden [WMM]
			benchmarkData.chunkSettings = {
				chunkSizeSeconds: chunkSizeSeconds,
				overlapMs: overlapMs,
				testMode: 'sequential',
				audioSource: audioSource
			};
		} else {
			alert('Bitte wählen Sie eine Audio-Datei oder -Aufnahme aus.');
			return;
		}
		
		dispatch('runBenchmark', benchmarkData);
	}
	
	$: canRunBenchmark = selectedModels.length > 0 && (
		(activeTab === 'upload' && uploadFile) ||
		(activeTab === 'record' && selectedAudioRecord) ||
		(activeTab === 'chunk') || // ChunkTestTab handles its own validation
		(activeTab === 'live' && false) // TODO: Implement live transcription validation
	);
</script>

<div class="benchmark-runner-tabs">
	<!-- Tab Navigation -->
	<div class="tab-navigation">
		{#each tabs as tab}
			<button
				class="tab-button"
				class:active={activeTab === tab.id}
				class:disabled={!tab.enabled}
				on:click={() => handleTabChange(tab.id)}
				disabled={!tab.enabled}
			>
				<span class="tab-icon">{tab.icon}</span>
				<span class="tab-label">{tab.label}</span>
				{#if !tab.enabled}
					<span class="tab-badge">Später</span>
				{/if}
			</button>
		{/each}
	</div>
	
	<!-- Tab Content -->
	<div class="tab-content">
		{#if activeTab === 'upload'}
			<FileUploadTab on:fileSelected={handleFileUpload} />
		{:else if activeTab === 'record'}
			<AudioRecordTab 
				bind:isRecording 
				on:audioRecorded={handleAudioRecorded}
				{audioRecords}
			/>
		{:else if activeTab === 'chunk'}
			<ChunkTestTab 
				{audioRecords}
				{isRunning}
				bind:chunkSizeSeconds
				bind:overlapMs
				bind:audioSource
				bind:selectedAudioRecord={chunkSelectedAudioRecord}
				bind:uploadFile={chunkUploadFile}
			/>
		{:else if activeTab === 'live'}
			<LiveTranscriptionTab />
		{/if}
	</div>
	
	<!-- Record History Selection (shown for record tab and when records exist) -->
	{#if activeTab === 'record' && audioRecords.length > 0}
		<div class="section record-selection">
			<h3>🎵 Aufnahme auswählen</h3>
			<div class="record-selector-container">
				<div class="record-selector">
					<label for="audio-record-select">Gespeicherte Aufnahmen:</label>
					<select 
						id="audio-record-select" 
						bind:value={selectedAudioRecord}
						class="record-dropdown"
					>
						<option value={null}>-- Aufnahme auswählen --</option>
						{#each audioRecords as record}
							<option value={record.id}>
								{record.fileName} ({record.duration.toFixed(1)}s) - {new Date(record.created).toLocaleString('de-CH')}
							</option>
						{/each}
					</select>
				</div>
				
				<!-- Audio Player [SP][EIV] -->
				{#if selectedAudioRecord}
					{@const selectedRecord = audioRecords.find(r => r.id === selectedAudioRecord)}
					{#if selectedRecord}
						<div class="audio-player">
							<h4>🔊 Wiedergabe: {selectedRecord.fileName}</h4>
							<div class="player-info">
								<span class="duration">Dauer: {selectedRecord.duration.toFixed(1)}s</span>
								<span class="created">Erstellt: {new Date(selectedRecord.created).toLocaleString('de-CH')}</span>
							</div>
							<audio 
								controls 
								preload="none"
								src="http://localhost:5155/api/ai/audio/{selectedRecord.id}/data"
								class="audio-control"
							>
								Ihr Browser unterstützt das Audio-Element nicht.
							</audio>
						</div>
					{/if}
				{/if}
			</div>
		</div>
	{/if}
	
	<!-- Shared Model Selection -->
	<div class="section model-selection">
		<h3>🤖 Modell-Auswahl</h3>
		<div class="model-grid">
			{#each availableModels as model}
				<div class="model-card" class:selected={selectedModels.includes(model)}>
					<label class="model-label">
						<input
							type="checkbox"
							checked={selectedModels.includes(model)}
							on:change={() => toggleModel(model)}
							class="model-checkbox"
						/>
						<div class="model-info">
							<div class="model-name">{model}</div>
							<div class="model-details">
								<span class="model-size">{modelInfo[model]?.size}</span>
								<span class="model-ram">RAM: {modelInfo[model]?.ram}</span>
							</div>
							<div class="model-description">{modelInfo[model]?.description}</div>
						</div>
					</label>
				</div>
			{/each}
		</div>
	</div>
	
	<!-- Shared Benchmark Settings -->
	<div class="section benchmark-settings">
		<h3>⚙️ Benchmark-Einstellungen</h3>
		<div class="settings-grid">
			<div class="setting-item">
				<label for="iterations">Iterationen:</label>
				<input
					id="iterations"
					type="number"
					min="1"
					max="10"
					bind:value={iterations}
					class="setting-input"
				/>
			</div>
		</div>
	</div>
	
	<!-- Shared Run Benchmark Button -->
	<div class="section run-section">
		<button
			class="run-button"
			class:disabled={!canRunBenchmark || isRunning}
			disabled={!canRunBenchmark || isRunning}
			on:click={runBenchmark}
		>
			{#if isRunning}
				<span class="spinner"></span>
				Benchmark läuft...
			{:else}
				🚀 Benchmark starten
			{/if}
		</button>
	</div>
	
	<!-- Shared Benchmark History -->
	<slot name="benchmark-history" />
</div>

<style>
	.benchmark-runner-tabs {
		display: flex;
		flex-direction: column;
		gap: 2rem;
		margin: 0 auto;
	}
	
	/* Tab Navigation */
	.tab-navigation {
		display: flex;
		gap: 0.5rem;
		border-bottom: 2px solid var(--border-color, #e5e7eb);
		padding-bottom: 0.5rem;
	}
	
	.tab-button {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		padding: 0.75rem 1rem;
		border: none;
		background: transparent;
		border-radius: 0.5rem 0.5rem 0 0;
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.9rem;
		position: relative;
	}
	
	.tab-button:hover:not(.disabled) {
		background: var(--hover-bg, #f3f4f6);
	}
	
	.tab-button.active {
		background: var(--primary-color, #2563eb);
		color: white;
		font-weight: 600;
	}
	
	.tab-button.disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
	
	.tab-icon {
		font-size: 1.1rem;
	}
	
	.tab-badge {
		background: var(--warning-color, #f59e0b);
		color: white;
		font-size: 0.7rem;
		padding: 0.2rem 0.4rem;
		border-radius: 0.25rem;
		font-weight: 600;
	}
	
	/* Tab Content */
	.tab-content {
		min-height: 200px;
	}
	
	/* Record Selection */
	.record-selection {
		background: var(--bg-secondary, #f8fafc);
		padding: 1.5rem;
		border-radius: 0.75rem;
		border: 2px dashed var(--border-color, #e5e7eb);
	}
	
	.record-selector-container {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 2rem;
		align-items: start;
	}
	
	@media (max-width: 768px) {
		.record-selector-container {
			grid-template-columns: 1fr;
			gap: 1rem;
		}
	}
	
	.record-selector {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}
	
	.record-dropdown {
		padding: 0.75rem;
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 0.5rem;
		font-size: 0.9rem;
		background: white;
	}
	
	.record-dropdown:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	/* Audio Player */
	.audio-player {
		padding: 1.5rem;
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 0.75rem;
		background: var(--bg-secondary, #f9fafb);
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}
	
	.audio-player h4 {
		margin: 0;
		font-size: 1rem;
		color: var(--text-primary, #1f2937);
		display: flex;
		align-items: center;
		gap: 0.5rem;
	}
	
	.player-info {
		display: flex;
		justify-content: space-between;
		flex-wrap: wrap;
		gap: 0.5rem;
		font-size: 0.85rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.audio-control {
		width: 100%;
		height: 40px;
		border-radius: 0.5rem;
		outline: none;
	}
	
	.audio-control:focus {
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	/* Shared Sections */
	.section {
		background: white;
		padding: 1.5rem;
		border-radius: 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
	}
	
	.section h3 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1.1rem;
		font-weight: 600;
	}
	
	/* Model Selection */
	.model-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 1rem;
	}
	
	.model-card {
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 0.5rem;
		padding: 1rem;
		transition: all 0.2s ease;
		cursor: pointer;
	}
	
	.model-card:hover {
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 2px 8px rgba(37, 99, 235, 0.1);
	}
	
	.model-card.selected {
		border-color: var(--primary-color, #2563eb);
		background: rgba(37, 99, 235, 0.05);
	}
	
	.model-label {
		display: flex;
		align-items: flex-start;
		gap: 0.75rem;
		cursor: pointer;
	}
	
	.model-checkbox {
		margin-top: 0.25rem;
		width: 1.2rem;
		height: 1.2rem;
		cursor: pointer;
	}
	
	.model-info {
		flex: 1;
	}
	
	.model-name {
		font-weight: 600;
		color: var(--text-primary, #1f2937);
		margin-bottom: 0.25rem;
	}
	
	.model-details {
		display: flex;
		gap: 1rem;
		font-size: 0.8rem;
		color: var(--text-secondary, #6b7280);
		margin-bottom: 0.25rem;
	}
	
	.model-description {
		font-size: 0.85rem;
		color: var(--text-secondary, #6b7280);
		font-style: italic;
	}
	
	/* Benchmark Settings */
	.settings-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 1rem;
	}
	
	.setting-item {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}
	
	.setting-item label {
		font-weight: 500;
		color: var(--text-primary, #1f2937);
	}
	
	.setting-input {
		padding: 0.75rem;
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 0.5rem;
		font-size: 0.9rem;
	}
	
	.setting-input:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	/* Run Button */
	.run-section {
		text-align: center;
	}
	
	.run-button {
		background: var(--primary-color, #2563eb);
		color: white;
		border: none;
		padding: 1rem 2rem;
		border-radius: 0.75rem;
		font-size: 1.1rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.2s ease;
		display: inline-flex;
		align-items: center;
		gap: 0.5rem;
	}
	
	.run-button:hover:not(.disabled) {
		background: var(--primary-hover, #1d4ed8);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
	}
	
	.run-button.disabled {
		background: var(--gray-400, #9ca3af);
		cursor: not-allowed;
		transform: none;
		box-shadow: none;
	}
	
	.spinner {
		width: 1rem;
		height: 1rem;
		border: 2px solid transparent;
		border-top: 2px solid currentColor;
		border-radius: 50%;
		animation: spin 1s linear infinite;
	}
	
	@keyframes spin {
		to {
			transform: rotate(360deg);
		}
	}
</style>
