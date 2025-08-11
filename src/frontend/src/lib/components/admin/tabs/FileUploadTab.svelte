<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	
	// [WMM] File upload tab for benchmark testing
	// [ATV] Audit trail for file uploads
	
	const dispatch = createEventDispatcher<{
		fileSelected: { file: File };
	}>();
	
	let audioFile: File | null = null;
	let dragOver = false;
	let fileInput: HTMLInputElement;
	
	function handleFileSelect(event: Event) {
		const target = event.target as HTMLInputElement;
		if (target.files && target.files[0]) {
			selectFile(target.files[0]);
		}
	}
	
	function handleDrop(event: DragEvent) {
		event.preventDefault();
		dragOver = false;
		
		if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
			selectFile(event.dataTransfer.files[0]);
		}
	}
	
	function handleDragOver(event: DragEvent) {
		event.preventDefault();
		dragOver = true;
	}
	
	function handleDragLeave() {
		dragOver = false;
	}
	
	function selectFile(file: File) {
		// Validate file type
		const validTypes = ['audio/wav', 'audio/mpeg', 'audio/mp4', 'audio/webm', 'audio/ogg'];
		if (!validTypes.includes(file.type)) {
			alert('Bitte wählen Sie eine gültige Audio-Datei aus (WAV, MP3, MP4, WebM, OGG).');
			return;
		}
		
		// Validate file size (max 100MB)
		const maxSize = 100 * 1024 * 1024; // 100MB
		if (file.size > maxSize) {
			alert('Die Datei ist zu groß. Maximale Größe: 100MB.');
			return;
		}
		
		audioFile = file;
		dispatch('fileSelected', { file });
	}
	
	function clearFile() {
		audioFile = null;
		if (fileInput) {
			fileInput.value = '';
		}
	}
	
	function formatFileSize(bytes: number): string {
		if (bytes === 0) return '0 Bytes';
		const k = 1024;
		const sizes = ['Bytes', 'KB', 'MB', 'GB'];
		const i = Math.floor(Math.log(bytes) / Math.log(k));
		return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
	}
</script>

<div class="file-upload-tab">
	<div class="upload-section">
		<h4>🎵 Audio-Datei hochladen</h4>
		
		<!-- File Drop Zone -->
		<div
			class="drop-zone"
			class:drag-over={dragOver}
			class:has-file={audioFile}
			on:drop={handleDrop}
			on:dragover={handleDragOver}
			on:dragleave={handleDragLeave}
			role="button"
			tabindex="0"
			on:click={() => fileInput?.click()}
			on:keydown={(e) => e.key === 'Enter' && fileInput?.click()}
		>
			{#if audioFile}
				<div class="file-info">
					<div class="file-icon">🎵</div>
					<div class="file-details">
						<div class="file-name">{audioFile.name}</div>
						<div class="file-meta">
							{formatFileSize(audioFile.size)} • {audioFile.type}
						</div>
					</div>
					<button class="clear-button" on:click|stopPropagation={clearFile}>
						❌
					</button>
				</div>
			{:else}
				<div class="drop-prompt">
					<div class="drop-icon">📁</div>
					<div class="drop-text">
						<strong>Audio-Datei hier ablegen</strong>
						<br />oder klicken zum Auswählen
					</div>
					<div class="drop-hint">
						Unterstützte Formate: WAV, MP3, MP4, WebM, OGG (max. 100MB)
					</div>
				</div>
			{/if}
		</div>
		
		<!-- Hidden File Input -->
		<input
			bind:this={fileInput}
			type="file"
			accept="audio/*"
			on:change={handleFileSelect}
			style="display: none;"
		/>
		
		<!-- File Requirements -->
		<div class="requirements">
			<h5>📋 Anforderungen für optimale Ergebnisse:</h5>
			<ul>
				<li><strong>Format:</strong> WAV (unkomprimiert) bevorzugt</li>
				<li><strong>Abtastrate:</strong> 16kHz oder höher</li>
				<li><strong>Kanäle:</strong> Mono bevorzugt</li>
				<li><strong>Qualität:</strong> Klare Sprache, minimale Hintergrundgeräusche</li>
				<li><strong>Sprache:</strong> Deutsch (Hochdeutsch oder Schweizerdeutsch)</li>
			</ul>
		</div>
	</div>
</div>

<style>
	.file-upload-tab {
		display: flex;
		flex-direction: column;
		gap: 1.5rem;
	}
	
	.upload-section h4 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1rem;
		font-weight: 600;
	}
	
	/* Drop Zone */
	.drop-zone {
		border: 3px dashed var(--border-color, #e5e7eb);
		border-radius: 1rem;
		padding: 2rem;
		text-align: center;
		cursor: pointer;
		transition: all 0.3s ease;
		background: var(--bg-secondary, #f8fafc);
		min-height: 150px;
		display: flex;
		align-items: center;
		justify-content: center;
	}
	
	.drop-zone:hover {
		border-color: var(--primary-color, #2563eb);
		background: rgba(37, 99, 235, 0.05);
	}
	
	.drop-zone.drag-over {
		border-color: var(--primary-color, #2563eb);
		background: rgba(37, 99, 235, 0.1);
		transform: scale(1.02);
	}
	
	.drop-zone.has-file {
		border-color: var(--success-color, #10b981);
		background: rgba(16, 185, 129, 0.05);
	}
	
	/* Drop Prompt */
	.drop-prompt {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 1rem;
	}
	
	.drop-icon {
		font-size: 3rem;
		opacity: 0.6;
	}
	
	.drop-text {
		font-size: 1.1rem;
		color: var(--text-primary, #1f2937);
		line-height: 1.5;
	}
	
	.drop-hint {
		font-size: 0.9rem;
		color: var(--text-secondary, #6b7280);
	}
	
	/* File Info */
	.file-info {
		display: flex;
		align-items: center;
		gap: 1rem;
		width: 100%;
		max-width: 400px;
	}
	
	.file-icon {
		font-size: 2rem;
		flex-shrink: 0;
	}
	
	.file-details {
		flex: 1;
		text-align: left;
	}
	
	.file-name {
		font-weight: 600;
		color: var(--text-primary, #1f2937);
		margin-bottom: 0.25rem;
		word-break: break-word;
	}
	
	.file-meta {
		font-size: 0.9rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.clear-button {
		background: none;
		border: none;
		font-size: 1.2rem;
		cursor: pointer;
		padding: 0.5rem;
		border-radius: 0.5rem;
		transition: background 0.2s ease;
		flex-shrink: 0;
	}
	
	.clear-button:hover {
		background: rgba(239, 68, 68, 0.1);
	}
	
	/* Requirements */
	.requirements {
		background: var(--info-bg, #eff6ff);
		border: 1px solid var(--info-border, #bfdbfe);
		border-radius: 0.75rem;
		padding: 1.5rem;
		display: none;
	}
	
	.requirements h5 {
		margin: 0 0 1rem 0;
		color: var(--info-text, #1e40af);
		font-size: 0.95rem;
		font-weight: 600;
	}
	
	.requirements ul {
		margin: 0;
		padding-left: 1.5rem;
		color: var(--text-secondary, #6b7280);
		font-size: 0.9rem;
		line-height: 1.6;
	}
	
	.requirements li {
		margin-bottom: 0.5rem;
	}
	
	.requirements li:last-child {
		margin-bottom: 0;
	}
</style>
