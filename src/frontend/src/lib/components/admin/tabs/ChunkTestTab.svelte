<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	
	// [WMM] Chunk test tab for sequential benchmark testing per chunk
	// [PSF] Performance analysis for continuous transcription scenarios
	// [ATV] Audit trail for chunk testing operations
	
	export let audioRecords: Array<{id: string, fileName: string, created: string, duration: number}> = [];
	export let isRunning: boolean = false;
	
	const dispatch = createEventDispatcher<{
		chunkTestStarted: {
			models: string[];
			audioRecordId?: string;
			audioFile?: File;
			chunkSettings: ChunkTestSettings;
		};
	}>(); 
	
	interface ChunkTestSettings {
		chunkSizeSeconds: number;
		overlapMs: number;
		testMode: 'sequential';
		audioSource: 'record' | 'upload';
	}
	
	// Chunk test settings - exported for parent component access [WMM]
	export let chunkSizeSeconds = 2;
	export let overlapMs = 100;
	export let audioSource: 'record' | 'upload' = 'record';
	export let selectedAudioRecord: string | null = null;
	export let uploadFile: File | null = null;
	
	// File upload handling
	function handleFileUpload(event: Event) {
		const target = event.target as HTMLInputElement;
		if (target.files && target.files[0]) {
			uploadFile = target.files[0];
			selectedAudioRecord = null; // Clear record selection
		}
	}
	
	function handleFileDrop(event: DragEvent) {
		event.preventDefault();
		const files = event.dataTransfer?.files;
		if (files && files[0]) {
			uploadFile = files[0];
			selectedAudioRecord = null;
		}
	}
	
	function handleDragOver(event: DragEvent) {
		event.preventDefault();
	}
	
	// Chunk-Test wird über den gemeinsamen "🚀 Benchmark starten" Button gestartet
	// Die startChunkTest Funktion und canStartTest sind nicht mehr nötig [UI-Vereinfachung]
	
	$: estimatedChunks = selectedAudioRecord 
		? Math.ceil((audioRecords.find(r => r.id === selectedAudioRecord)?.duration || 0) / (chunkSizeSeconds - overlapMs / 1000))
		: 0;
</script>

<div class="chunk-test-tab">
	<!-- Chunk Settings -->
	<div class="section chunk-settings">
		<h3>🔄 Chunk-Test Einstellungen</h3>
		<div class="settings-grid">
			<div class="setting-item">
				<label for="chunk-size">Chunk-Größe (Sekunden):</label>
				<input 
					id="chunk-size"
					type="number" 
					bind:value={chunkSizeSeconds}
					min="1" 
					max="10" 
					step="0.5"
					class="setting-input"
					disabled={isRunning}
				/>
				<small>Empfohlen: 1-5 Sekunden für Live-Simulation</small>
			</div>
			
			<div class="setting-item">
				<label for="overlap">Überlappung (Millisekunden):</label>
				<input 
					id="overlap"
					type="number" 
					bind:value={overlapMs}
					min="0" 
					max="500" 
					step="50"
					class="setting-input"
					disabled={isRunning}
				/>
				<small>Verhindert Wort-Abschnitte zwischen Chunks</small>
			</div>
		</div>
		
		{#if estimatedChunks > 0}
			<div class="chunk-preview">
				<strong>Geschätzte Chunks:</strong> {estimatedChunks}
				<small>(bei {chunkSizeSeconds}s Chunks mit {overlapMs}ms Überlappung)</small>
			</div>
		{/if}
	</div>
	
	<!-- Audio Source Selection -->
	<div class="section audio-source">
		<h3>🎵 Audio-Quelle auswählen</h3>
		<div class="source-tabs">
			<button 
				class="source-tab" 
				class:active={audioSource === 'record'}
				on:click={() => audioSource = 'record'}
				disabled={isRunning}
			>
				🎙️ Gespeicherte Aufnahme
			</button>
			<button 
				class="source-tab" 
				class:active={audioSource === 'upload'}
				on:click={() => audioSource = 'upload'}
				disabled={isRunning}
			>
				📁 Datei hochladen
			</button>
		</div>
		
		{#if audioSource === 'record'}
			<div class="record-selection">
				{#if audioRecords.length > 0}
					<label for="audio-record-select">Gespeicherte Aufnahmen:</label>
					<select 
						id="audio-record-select" 
						bind:value={selectedAudioRecord}
						class="record-dropdown"
						disabled={isRunning}
					>
						<option value={null}>-- Aufnahme auswählen --</option>
						{#each audioRecords as record}
							<option value={record.id}>
								{record.fileName} ({record.duration.toFixed(1)}s) - {new Date(record.created).toLocaleString('de-CH')}
							</option>
						{/each}
					</select>
				{:else}
					<p class="no-records">Keine gespeicherten Aufnahmen verfügbar. Nehmen Sie zuerst eine Aufnahme im "Audio Record" Tab auf.</p>
				{/if}
			</div>
		{:else if audioSource === 'upload'}
			<div class="file-upload">
				<div 
					class="upload-area" 
					class:has-file={uploadFile}
					on:drop={handleFileDrop}
					on:dragover={handleDragOver}
				>
					{#if uploadFile}
						<div class="file-info">
							<span class="file-icon">🎵</span>
							<div class="file-details">
								<strong>{uploadFile.name}</strong>
								<small>{(uploadFile.size / 1024 / 1024).toFixed(2)} MB</small>
							</div>
						</div>
					{:else}
						<div class="upload-prompt">
							<span class="upload-icon">📁</span>
							<p>Audio-Datei hier ablegen oder klicken zum Auswählen</p>
							<small>Unterstützte Formate: M4A, WAV, MP3, WebM</small>
						</div>
					{/if}
					<input 
						type="file" 
						accept="audio/*" 
						on:change={handleFileUpload}
						class="file-input"
						disabled={isRunning}
					/>
				</div>
			</div>
		{/if}
	</div>
	
	<!-- Chunk-Test wird über den gemeinsamen "🚀 Benchmark starten" Button gestartet -->
	<!-- Keine separaten Start-Buttons mehr nötig [UI-Vereinfachung] -->
</div>

<style>
	.chunk-test-tab {
		display: flex;
		flex-direction: column;
		gap: 1.5rem;
		padding: 1.5rem;
		margin: 0 auto;
	}
	
	.section {
		background: var(--bg-secondary, #f8fafc);
		border-radius: 0.75rem;
		padding: 1.5rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.section h3 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1.25rem;
		font-weight: 600;
	}
	
	/* Chunk Settings */
	.settings-grid {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 1.5rem;
		margin-bottom: 1rem;
	}
	
	.setting-item {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}
	
	.setting-item label {
		font-weight: 500;
		color: var(--text-primary, #1f2937);
		font-size: 0.9rem;
	}
	
	.setting-input {
		padding: 0.75rem;
		border: 1px solid var(--border-color, #d1d5db);
		border-radius: 0.5rem;
		font-size: 1rem;
		background: white;
		transition: border-color 0.2s;
	}
	
	.setting-input:focus {
		outline: none;
		border-color: var(--primary-color, #3b82f6);
		box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
	}
	
	.setting-input:disabled {
		background: var(--bg-disabled, #f3f4f6);
		color: var(--text-disabled, #9ca3af);
		cursor: not-allowed;
	}
	
	.setting-item small {
		color: var(--text-secondary, #6b7280);
		font-size: 0.8rem;
	}
	
	.chunk-preview {
		padding: 1rem;
		background: var(--bg-info, #eff6ff);
		border: 1px solid var(--border-info, #bfdbfe);
		border-radius: 0.5rem;
		color: var(--text-info, #1e40af);
	}
	
	.chunk-preview strong {
		margin-right: 0.5rem;
	}
	
	.chunk-preview small {
		display: block;
		margin-top: 0.25rem;
		color: var(--text-secondary, #6b7280);
	}
	
	/* Audio Source */
	.source-tabs {
		display: flex;
		gap: 0.5rem;
		margin-bottom: 1.5rem;
	}
	
	.source-tab {
		padding: 0.75rem 1.5rem;
		border: 1px solid var(--border-color, #d1d5db);
		border-radius: 0.5rem;
		background: white;
		color: var(--text-secondary, #6b7280);
		cursor: pointer;
		transition: all 0.2s;
		font-size: 0.9rem;
		font-weight: 500;
	}
	
	.source-tab:hover:not(:disabled) {
		border-color: var(--primary-color, #3b82f6);
		color: var(--primary-color, #3b82f6);
	}
	
	.source-tab.active {
		background: var(--primary-color, #3b82f6);
		color: white;
		border-color: var(--primary-color, #3b82f6);
	}
	
	.source-tab:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
	
	/* Record Selection */
	.record-selection label {
		display: block;
		margin-bottom: 0.5rem;
		font-weight: 500;
		color: var(--text-primary, #1f2937);
	}
	
	.record-dropdown {
		width: 100%;
		padding: 0.75rem;
		border: 1px solid var(--border-color, #d1d5db);
		border-radius: 0.5rem;
		background: white;
		font-size: 0.9rem;
		transition: border-color 0.2s;
	}
	
	.record-dropdown:focus {
		outline: none;
		border-color: var(--primary-color, #3b82f6);
		box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
	}
	
	.record-dropdown:disabled {
		background: var(--bg-disabled, #f3f4f6);
		color: var(--text-disabled, #9ca3af);
		cursor: not-allowed;
	}
	
	.no-records {
		padding: 1rem;
		background: var(--bg-warning, #fef3c7);
		border: 1px solid var(--border-warning, #fbbf24);
		border-radius: 0.5rem;
		color: var(--text-warning, #92400e);
		margin: 0;
		font-size: 0.9rem;
	}
	
	/* File Upload */
	.upload-area {
		position: relative;
		border: 2px dashed var(--border-color, #d1d5db);
		border-radius: 0.75rem;
		padding: 2rem;
		text-align: center;
		transition: all 0.2s;
		cursor: pointer;
		background: white;
	}
	
	.upload-area:hover {
		border-color: var(--primary-color, #3b82f6);
		background: var(--bg-hover, #f8fafc);
	}
	
	.upload-area.has-file {
		border-color: var(--success-color, #10b981);
		background: var(--bg-success, #ecfdf5);
	}
	
	.file-input {
		position: absolute;
		top: 0;
		left: 0;
		width: 100%;
		height: 100%;
		opacity: 0;
		cursor: pointer;
	}
	
	.file-input:disabled {
		cursor: not-allowed;
	}
	
	.upload-prompt {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.5rem;
	}
	
	.upload-icon {
		font-size: 3rem;
		opacity: 0.6;
	}
	
	.upload-prompt p {
		margin: 0;
		color: var(--text-primary, #1f2937);
		font-weight: 500;
	}
	
	.upload-prompt small {
		color: var(--text-secondary, #6b7280);
	}
	
	.file-info {
		display: flex;
		align-items: center;
		gap: 1rem;
		justify-content: center;
	}
	
	.file-icon {
		font-size: 2rem;
	}
	
	.file-details {
		text-align: left;
	}
	
	.file-details strong {
		display: block;
		color: var(--text-primary, #1f2937);
		margin-bottom: 0.25rem;
	}
	
	.file-details small {
		color: var(--text-secondary, #6b7280);
	}
	
	/* Start Section */
	.start-section {
		text-align: center;
	}
	
	.start-button {
		padding: 1rem 2rem;
		background: var(--primary-color, #3b82f6);
		color: white;
		border: none;
		border-radius: 0.75rem;
		font-size: 1.1rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.2s;
		min-width: 200px;
	}
	
	.start-button:hover:not(:disabled) {
		background: var(--primary-hover, #2563eb);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
	}
	
	.start-button:disabled,
	.start-button.disabled {
		background: var(--bg-disabled, #9ca3af);
		color: var(--text-disabled, #6b7280);
		cursor: not-allowed;
		transform: none;
		box-shadow: none;
	}
	
	.start-hint {
		margin: 1rem 0 0 0;
		color: var(--text-secondary, #6b7280);
		font-size: 0.9rem;
	}
	
	/* Responsive */
	@media (max-width: 768px) {
		.settings-grid {
			grid-template-columns: 1fr;
		}
		
		.source-tabs {
			flex-direction: column;
		}
		
		.chunk-test-tab {
			padding: 1rem;
		}
		
		.section {
			padding: 1rem;
		}
	}
</style>
