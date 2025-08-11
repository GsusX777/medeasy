<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	
	// [WMM] Whisper Multi-Model benchmark runner
	// [ATV] Audit trail for benchmark operations
	// [PSF] Patient safety through performance validation
	
	export let availableModels: string[];
	export let isRunning: boolean = false;
	
	const dispatch = createEventDispatcher<{
		runBenchmark: {
			models: string[];
			audioFile: File;
			iterations: number;
		};
	}>();
	
	let selectedModels: string[] = ['small'];
	let audioFile: File | null = null;
	let iterations = 1;
	let dragOver = false;
	let fileInput: HTMLInputElement;
	
	const modelInfo: Record<string, { size: string; ram: string; description: string }> = {
		'small': { size: '461MB', ram: '2GB', description: 'Ausgewogen, empfohlen' },
		'medium': { size: '1542MB', ram: '4GB', description: 'Langsamer, genauer' },
		'large-v3': { size: '3094MB', ram: '8GB', description: 'Langsamst, sehr genau' }
	};
	
	function toggleModel(model: string) {
		if (selectedModels.includes(model)) {
			selectedModels = selectedModels.filter(m => m !== model);
		} else {
			selectedModels = [...selectedModels, model];
		}
	}
	
	function handleFileSelect(event: Event) {
		const target = event.target as HTMLInputElement;
		if (target.files && target.files[0]) {
			audioFile = target.files[0];
		}
	}
	
	function handleDrop(event: DragEvent) {
		event.preventDefault();
		dragOver = false;
		
		if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
			const file = event.dataTransfer.files[0];
			if (isAudioFile(file)) {
				audioFile = file;
			} else {
				alert('Bitte wählen Sie eine Audio-Datei (M4A, WAV, MP3, FLAC)');
			}
		}
	}
	
	function handleDragOver(event: DragEvent) {
		event.preventDefault();
		dragOver = true;
	}
	
	function handleDragLeave() {
		dragOver = false;
	}
	
	function isAudioFile(file: File): boolean {
		const audioTypes = ['audio/mp4', 'audio/wav', 'audio/mpeg', 'audio/flac', 'audio/x-m4a'];
		const audioExtensions = ['.m4a', '.wav', '.mp3', '.flac'];
		
		return audioTypes.includes(file.type) || 
			   audioExtensions.some(ext => file.name.toLowerCase().endsWith(ext));
	}
	
	function formatFileSize(bytes: number): string {
		if (bytes === 0) return '0 Bytes';
		const k = 1024;
		const sizes = ['Bytes', 'KB', 'MB', 'GB'];
		const i = Math.floor(Math.log(bytes) / Math.log(k));
		return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
	}
	
	function runBenchmark() {
		if (!audioFile || selectedModels.length === 0 || isRunning) return;
		
		dispatch('runBenchmark', {
			models: selectedModels,
			audioFile,
			iterations
		});
	}
	
	function selectAllModels() {
		selectedModels = [...availableModels];
	}
	
	function clearSelection() {
		selectedModels = [];
	}
	
	$: canRun = audioFile && selectedModels.length > 0 && !isRunning;
	$: estimatedTime = selectedModels.length * iterations * 15; // Rough estimate: 15s per model per iteration
</script>

<div class="benchmark-runner">
	<!-- Audio File Upload -->
	<div class="section">
		<h3>🎵 Audio-Datei hochladen</h3>
		
		<div 
			class="file-drop-zone"
			class:drag-over={dragOver}
			class:has-file={audioFile}
			on:drop={handleDrop}
			on:dragover={handleDragOver}
			on:dragleave={handleDragLeave}
			role="button"
			tabindex="0"
		>
			{#if audioFile}
				<div class="file-info">
					<div class="file-icon">🎵</div>
					<div class="file-details">
						<span class="file-name">{audioFile.name}</span>
						<span class="file-size">{formatFileSize(audioFile.size)}</span>
						<span class="file-type">{audioFile.type || 'Audio-Datei'}</span>
					</div>
					<button 
						class="remove-file-btn"
						on:click={() => audioFile = null}
						disabled={isRunning}
					>
						🗑️
					</button>
				</div>
			{:else}
				<div class="drop-prompt">
					<div class="drop-icon">📁</div>
					<p class="drop-text">
						Audio-Datei hier ablegen oder klicken zum Auswählen
					</p>
					<p class="drop-formats">
						Unterstützte Formate: M4A, WAV, MP3, FLAC
					</p>
				</div>
			{/if}
			
			<input
				type="file"
				accept="audio/*,.m4a,.wav,.mp3,.flac"
				on:change={handleFileSelect}
				disabled={isRunning}
				style="display: none;"
				bind:this={fileInput}
			/>
		</div>
		
		{#if !audioFile}
			<button 
				class="file-select-btn"
				on:click={() => fileInput?.click()}
				disabled={isRunning}
			>
				📁 Datei auswählen
			</button>
		{/if}
	</div>
	
	<!-- Benchmark Settings -->
	<div class="section">
		<h3>⚙️ Benchmark-Einstellungen</h3>
		
		<div class="settings-grid">
			<div class="setting-item">
				<label for="iterations" class="setting-label">
					🔄 Wiederholungen:
				</label>
				<input
					id="iterations"
					type="number"
					min="1"
					max="5"
					bind:value={iterations}
					disabled={isRunning}
					class="setting-input"
				/>
				<span class="setting-help">
					Anzahl der Test-Durchläufe pro Modell
				</span>
			</div>
		</div>
		
		{#if estimatedTime > 0}
			<div class="time-estimate">
				<span class="estimate-label">⏱️ Geschätzte Dauer:</span>
				<span class="estimate-value">
					{Math.floor(estimatedTime / 60)}:{(estimatedTime % 60).toString().padStart(2, '0')} Min
				</span>
			</div>
		{/if}
	</div>
	
	<!-- Model Selection -->
	<div class="section">
		<div class="section-header">
			<h3>🤖 Whisper-Modelle auswählen</h3>
			<div class="model-actions">
				<button class="select-btn" on:click={selectAllModels}>
					Alle auswählen
				</button>
				<button class="select-btn" on:click={clearSelection}>
					Auswahl löschen
				</button>
			</div>
		</div>
		
		<div class="models-grid">
			{#each availableModels as model}
				<label class="model-card" class:selected={selectedModels.includes(model)}>
					<input
						type="checkbox"
						bind:group={selectedModels}
						value={model}
						disabled={isRunning}
					/>
					<div class="model-info">
						<div class="model-header">
							<span class="model-name">{model}</span>
							<span class="model-size">{modelInfo[model]?.size || 'N/A'}</span>
						</div>
						<div class="model-details">
							<span class="model-ram">RAM: {modelInfo[model]?.ram || 'N/A'}</span>
							<span class="model-description">{modelInfo[model]?.description || ''}</span>
						</div>
					</div>
				</label>
			{/each}
		</div>
		
		{#if selectedModels.length > 0}
			<div class="selection-summary">
				<span class="summary-text">
					{selectedModels.length} Modell{selectedModels.length !== 1 ? 'e' : ''} ausgewählt: 
					{selectedModels.join(', ')}
				</span>
			</div>
		{/if}
	</div>
	
	<!-- Run Benchmark -->
	<div class="section">
		<div class="run-section">
			<button 
				class="run-btn"
				class:disabled={!canRun}
				on:click={runBenchmark}
				disabled={!canRun}
			>
				{#if isRunning}
					🔄 Benchmark läuft...
				{:else}
					🚀 Benchmark starten
				{/if}
			</button>
			
			{#if !canRun && !isRunning}
				<div class="requirements">
					<p class="requirements-title">Voraussetzungen:</p>
					<ul class="requirements-list">
						{#if selectedModels.length === 0}
							<li>❌ Mindestens ein Modell auswählen</li>
						{:else}
							<li>✅ {selectedModels.length} Modell(e) ausgewählt</li>
						{/if}
						
						{#if !audioFile}
							<li>❌ Audio-Datei hochladen</li>
						{:else}
							<li>✅ Audio-Datei bereit</li>
						{/if}
					</ul>
				</div>
			{/if}
		</div>
	</div>
</div>

<style>
	.benchmark-runner {
		display: flex;
		flex-direction: column;
		gap: 2rem;
	}
	
	.section {
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		padding: 1.5rem;
		background: var(--bg-secondary, #f8fafc);
	}
	
	.section h3 {
		margin-bottom: 1rem;
		color: var(--text-primary, #111827);
	}
	
	.section-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 1rem;
	}
	
	.section-header h3 {
		margin-bottom: 0;
	}
	
	.model-actions {
		display: flex;
		gap: 0.5rem;
	}
	
	.select-btn {
		padding: 0.25rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 4px;
		background: white;
		cursor: pointer;
		font-size: 0.875rem;
		transition: all 0.2s ease;
	}
	
	.select-btn:hover {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.models-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: 1rem;
		margin-bottom: 1rem;
	}
	
	.model-card {
		display: flex;
		align-items: center;
		padding: 1rem;
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 8px;
		background: white;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.model-card:hover {
		border-color: var(--primary-color, #2563eb);
	}
	
	.model-card.selected {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.model-card input[type="checkbox"] {
		margin-right: 0.75rem;
		cursor: pointer;
	}
	
	.model-info {
		flex: 1;
	}
	
	.model-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 0.5rem;
	}
	
	.model-name {
		font-weight: 600;
		color: var(--text-primary, #111827);
	}
	
	.model-size {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		background: var(--bg-secondary, #f8fafc);
		padding: 0.25rem 0.5rem;
		border-radius: 4px;
	}
	
	.model-details {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.model-ram {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	.model-description {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		font-style: italic;
	}
	
	.selection-summary {
		padding: 0.75rem;
		background: var(--primary-bg, #eff6ff);
		border-radius: 6px;
		border: 1px solid var(--primary-color, #2563eb);
	}
	
	.summary-text {
		font-size: 0.875rem;
		color: var(--primary-color, #2563eb);
		font-weight: 500;
	}
	
	.file-drop-zone {
		border: 2px dashed var(--border-color, #e5e7eb);
		border-radius: 8px;
		padding: 2rem;
		text-align: center;
		cursor: pointer;
		transition: all 0.2s ease;
		background: white;
		margin-bottom: 1rem;
	}
	
	.file-drop-zone.drag-over {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.file-drop-zone.has-file {
		border-color: #059669;
		background: #ecfdf5;
	}
	
	.file-info {
		display: flex;
		align-items: center;
		gap: 1rem;
	}
	
	.file-icon {
		font-size: 2rem;
	}
	
	.file-details {
		flex: 1;
		text-align: left;
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.file-name {
		font-weight: 500;
		color: var(--text-primary, #111827);
	}
	
	.file-size,
	.file-type {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.remove-file-btn {
		padding: 0.5rem;
		border: 1px solid #dc2626;
		border-radius: 4px;
		background: white;
		color: #dc2626;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.remove-file-btn:hover {
		background: #dc2626;
		color: white;
	}
	
	.drop-prompt {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.5rem;
	}
	
	.drop-icon {
		font-size: 3rem;
		opacity: 0.5;
	}
	
	.drop-text {
		font-weight: 500;
		color: var(--text-primary, #111827);
		margin: 0;
	}
	
	.drop-formats {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		margin: 0;
	}
	
	.file-select-btn {
		padding: 0.75rem 1.5rem;
		border: 1px solid var(--primary-color, #2563eb);
		border-radius: 6px;
		background: var(--primary-color, #2563eb);
		color: white;
		cursor: pointer;
		transition: all 0.2s ease;
		font-weight: 500;
	}
	
	.file-select-btn:hover:not(:disabled) {
		background: #1d4ed8;
		border-color: #1d4ed8;
	}
	
	.file-select-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
	
	.settings-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 1rem;
		margin-bottom: 1rem;
	}
	
	.setting-item {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}
	
	.setting-label {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.setting-input {
		padding: 0.5rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		font-size: 0.875rem;
	}
	
	.setting-input:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	.setting-help {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.time-estimate {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		padding: 0.75rem;
		background: #fffbeb;
		border: 1px solid #d97706;
		border-radius: 6px;
	}
	
	.estimate-label {
		font-size: 0.875rem;
		color: #92400e;
		font-weight: 500;
	}
	
	.estimate-value {
		font-size: 0.875rem;
		color: #92400e;
		font-weight: 600;
	}
	
	.run-section {
		display: flex;
		flex-direction: column;
		gap: 1rem;
		align-items: center;
	}
	
	.run-btn {
		padding: 1rem 2rem;
		border: none;
		border-radius: 8px;
		background: var(--primary-color, #2563eb);
		color: white;
		cursor: pointer;
		transition: all 0.2s ease;
		font-weight: 600;
		font-size: 1rem;
		min-width: 200px;
	}
	
	.run-btn:hover:not(:disabled) {
		background: #1d4ed8;
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
	}
	
	.run-btn:disabled,
	.run-btn.disabled {
		background: var(--text-secondary, #6b7280);
		cursor: not-allowed;
		transform: none;
		box-shadow: none;
	}
	
	.requirements {
		text-align: center;
	}
	
	.requirements-title {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		margin-bottom: 0.5rem;
	}
	
	.requirements-list {
		list-style: none;
		padding: 0;
		margin: 0;
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.requirements-list li {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	@media (max-width: 768px) {
		.models-grid {
			grid-template-columns: 1fr;
		}
		
		.model-header {
			flex-direction: column;
			align-items: flex-start;
			gap: 0.5rem;
		}
		
		.file-info {
			flex-direction: column;
			text-align: center;
		}
		
		.settings-grid {
			grid-template-columns: 1fr;
		}
		
		.time-estimate {
			flex-direction: column;
			text-align: center;
		}
	}
</style>
