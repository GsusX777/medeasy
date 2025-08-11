<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { createEventDispatcher, onMount, onDestroy } from 'svelte';
	
	// [WMM] Live transcription tab for real-time audio processing
	// [ATV] Audit trail for live transcription
	// [CT] Cloud transparency for live transcription features
	// [PSF] Patient safety through real-time monitoring
	
	const dispatch = createEventDispatcher<{
		liveTranscriptionComplete: { 
			audioBlob: Blob; 
			transcript: string;
			modelUsed: string;
			chunkSettings: {
				chunkSizeSeconds: number;
				overlapMs: number;
			};
		};
	}>;
	
	export let availableModels: string[] = [];
	export let isRunning: boolean = false;
	
	// Live transcription state
	let isLiveRecording = false;
	let isPaused = false; // Pause state
	let selectedModel = 'small'; // Single model selection
	let selectedDeviceId = '';
	let availableDevices: MediaDeviceInfo[] = [];
	
	// Pause tracking
	let pausedDuration = 0; // Total paused time in ms
	let pauseStartTime = 0; // When pause started
	
	// Chunk settings
	let chunkSizeSeconds = 5; // Longer chunks for better language detection
	let overlapMs = 500; // 500ms overlap
	
	// Audio recording
	let mediaRecorder: MediaRecorder | null = null;
	let audioStream: MediaStream | null = null;
	let audioChunks: Blob[] = [];
	let recordingDuration = 0;
	let recordingTimer: number | null = null;
	
	// Live transcription display
	let currentTranscript = '';
	let transcriptionChunks: string[] = [];
	let isProcessingChunk = false;
	
	// Volume visualization
	let audioContext: AudioContext | null = null;
	let analyser: AnalyserNode | null = null;
	let dataArray: Uint8Array | null = null;
	let animationFrame: number | null = null;
	let volumeLevel = 0;
	
	// Audio settings optimized for Whisper [WMM]
	const audioSettings = {
		sampleRate: 16000,    // 16kHz for Whisper
		channelCount: 1,      // Mono
		bitRate: 256000,      // 256kbps
		format: 'wav'         // WAV for better Whisper compatibility
	};
	
	const modelInfo: Record<string, { size: string; ram: string; description: string }> = {
		'small': { size: '461MB', ram: '2GB', description: 'Empfohlen für Live-Transkription' },
		'medium': { size: '1542MB', ram: '4GB', description: 'Höhere Genauigkeit, mehr Latenz' },
		'large-v3': { size: '3094MB', ram: '8GB', description: 'Beste Genauigkeit, hohe Latenz' }
	};
	
	onMount(() => {
		loadAudioDevices();
	});
	
	onDestroy(() => {
		stopLiveTranscription();
		if (audioStream) {
			audioStream.getTracks().forEach(track => track.stop());
		}
		if (animationFrame) {
			cancelAnimationFrame(animationFrame);
		}
	});
	
	async function loadAudioDevices() {
		try {
			const devices = await navigator.mediaDevices.enumerateDevices();
			availableDevices = devices.filter(device => device.kind === 'audioinput');
			
			if (availableDevices.length > 0 && !selectedDeviceId) {
				selectedDeviceId = availableDevices[0].deviceId;
			}
			console.log(`🎤 Found ${availableDevices.length} audio input devices [WMM]`);
		} catch (error) {
			console.error('Failed to load audio devices:', error);
		}
	}
	
	async function startLiveTranscription() {
		try {
			// Request microphone access
			const constraints: MediaStreamConstraints = {
				audio: {
					deviceId: selectedDeviceId ? { exact: selectedDeviceId } : undefined,
					sampleRate: audioSettings.sampleRate,
					channelCount: audioSettings.channelCount,
					echoCancellation: true,
					noiseSuppression: true,
					autoGainControl: true
				}
			};
			
			audioStream = await navigator.mediaDevices.getUserMedia(constraints);
			
			// Setup audio analysis for volume visualization
			setupAudioAnalysis();
			
			// Setup MediaRecorder for WAV recording
			const options: MediaRecorderOptions = {
				mimeType: 'audio/wav', // Try WAV first
				audioBitsPerSecond: audioSettings.bitRate
			};
			
			// Fallback to WebM if WAV not supported
			if (!MediaRecorder.isTypeSupported(options.mimeType!)) {
				options.mimeType = 'audio/webm;codecs=opus';
				console.log('🔄 Fallback to WebM format [WMM]');
			}
			
			mediaRecorder = new MediaRecorder(audioStream, options);
			audioChunks = [];
			transcriptionChunks = [];
			currentTranscript = '';
			
			mediaRecorder.ondataavailable = (event) => {
				if (event.data.size > 0) {
					audioChunks.push(event.data);
					// Process chunk for live transcription
					processLiveChunk(event.data);
				}
			};
			
			mediaRecorder.onstop = () => {
				finalizeLiveTranscription();
			};
			
			// Start recording with chunk intervals
			const chunkIntervalMs = chunkSizeSeconds * 1000;
			mediaRecorder.start(chunkIntervalMs);
			
			isLiveRecording = true;
			recordingDuration = 0;
			
			// Start timer
			recordingTimer = setInterval(() => {
				recordingDuration++;
			}, 1000);
			
			console.log(`🔴 Live transcription started with ${selectedModel} model [WMM][Live]`);
			
		} catch (error) {
			console.error('Failed to start live transcription:', error);
			alert('Fehler beim Starten der Live-Transkription. Bitte überprüfen Sie die Mikrofon-Berechtigung.');
		}
	}
	
	function pauseLiveTranscription() {
		if (mediaRecorder && isLiveRecording && !isPaused) {
			mediaRecorder.pause();
			isPaused = true;
			pauseStartTime = Date.now();
			
			// Pause timer
			if (recordingTimer) {
				clearInterval(recordingTimer);
				recordingTimer = null;
			}
			
			// Pause volume visualization
			if (animationFrame) {
				cancelAnimationFrame(animationFrame);
				animationFrame = null;
			}
			
			console.log(`⏸️ Live transcription paused at ${recordingDuration}s [WMM][Live]`);
		}
	}
	
	function resumeLiveTranscription() {
		if (mediaRecorder && isLiveRecording && isPaused) {
			mediaRecorder.resume();
			isPaused = false;
			
			// Track total paused time
			pausedDuration += Date.now() - pauseStartTime;
			
			// Resume timer
			recordingTimer = setInterval(() => {
				recordingDuration++;
			}, 1000);
			
			// Resume volume visualization
			updateVolumeLevel();
			
			console.log(`▶️ Live transcription resumed at ${recordingDuration}s [WMM][Live]`);
		}
	}
	
	function stopLiveTranscription() {
		if (mediaRecorder && isLiveRecording) {
			mediaRecorder.stop();
			isLiveRecording = false;
			isPaused = false;
			
			if (recordingTimer) {
				clearInterval(recordingTimer);
				recordingTimer = null;
			}
			
			if (animationFrame) {
				cancelAnimationFrame(animationFrame);
				animationFrame = null;
			}
			
			// Reset pause tracking
			pausedDuration = 0;
			pauseStartTime = 0;
			
			console.log(`⏹️ Live transcription stopped after ${recordingDuration}s [WMM][Live]`);
		}
	}
	
	async function processLiveChunk(chunkBlob: Blob) {
		if (isProcessingChunk) {
			console.log('⏳ Skipping chunk - previous chunk still processing [WMM]');
			return;
		}
		
		isProcessingChunk = true;
		
		try {
			// TODO: Send chunk to backend for transcription
			// This will be implemented in Phase 2
			console.log(`🔄 Processing chunk: ${chunkBlob.size} bytes with ${selectedModel} [WMM][Live]`);
			
			// Simulate transcription for now
			await new Promise(resolve => setTimeout(resolve, 1000));
			
			// Mock transcription result
			const mockTranscript = `[Chunk ${transcriptionChunks.length + 1}] Live-Transkription läuft...`;
			transcriptionChunks.push(mockTranscript);
			currentTranscript = transcriptionChunks.join(' ');
			
		} catch (error) {
			console.error('Failed to process live chunk:', error);
		} finally {
			isProcessingChunk = false;
		}
	}
	
	async function finalizeLiveTranscription() {
		try {
			// Combine all audio chunks into final blob
			const finalAudioBlob = new Blob(audioChunks, { 
				type: audioChunks[0]?.type || 'audio/wav' 
			});
			
			// Dispatch completion event for parent component
			dispatch('liveTranscriptionComplete', {
				audioBlob: finalAudioBlob,
				transcript: currentTranscript,
				modelUsed: selectedModel,
				chunkSettings: {
					chunkSizeSeconds,
					overlapMs
				}
			});
			
			console.log(`✅ Live transcription finalized: ${finalAudioBlob.size} bytes, ${transcriptionChunks.length} chunks [WMM][Live]`);
			
		} catch (error) {
			console.error('Failed to finalize live transcription:', error);
		}
	}
	
	function setupAudioAnalysis() {
		if (!audioStream) return;
		
		try {
			audioContext = new AudioContext();
			analyser = audioContext.createAnalyser();
			const source = audioContext.createMediaStreamSource(audioStream);
			
			analyser.fftSize = 256;
			dataArray = new Uint8Array(analyser.frequencyBinCount);
			
			source.connect(analyser);
			updateVolumeLevel();
		} catch (error) {
			console.error('Failed to setup audio analysis:', error);
		}
	}
	
	function updateVolumeLevel() {
		if (!analyser || !dataArray) return;
		
		analyser.getByteFrequencyData(dataArray);
		
		// Calculate average volume
		let sum = 0;
		for (let i = 0; i < dataArray.length; i++) {
			sum += dataArray[i];
		}
		volumeLevel = sum / dataArray.length;
		
		if (isLiveRecording) {
			animationFrame = requestAnimationFrame(updateVolumeLevel);
		}
	}
	
	function formatDuration(seconds: number): string {
		const mins = Math.floor(seconds / 60);
		const secs = seconds % 60;
		return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
	}
</script>

<div class="live-transcription-tab">
	<!-- Microphone Selection -->
	<div class="device-selection">
		<h3>🎤 Mikrofon-Auswahl</h3>
		<select bind:value={selectedDeviceId} class="device-select">
			{#each availableDevices as device}
				<option value={device.deviceId}>
					{device.label || `Mikrofon ${device.deviceId.slice(0, 8)}...`}
				</option>
			{/each}
		</select>
		{#if availableDevices.length === 0}
			<p class="no-devices">⚠️ Keine Mikrofone gefunden</p>
		{/if}
	</div>

	<!-- Single Model Selection -->
	<div class="model-selection">
		<h3>🤖 Modell-Auswahl (Einzelmodell)</h3>
		<div class="model-options">
			{#each Object.entries(modelInfo) as [modelName, info]}
				<label class="model-option" class:selected={selectedModel === modelName}>
					<input 
						type="radio" 
						bind:group={selectedModel} 
						value={modelName}
						disabled={isLiveRecording}
					/>
					<div class="model-info">
						<div class="model-name">{modelName}</div>
						<div class="model-details">
							<span class="model-size">{info.size}</span>
							<span class="model-ram">{info.ram} RAM</span>
						</div>
						<div class="model-description">{info.description}</div>
					</div>
				</label>
			{/each}
		</div>
	</div>

	<!-- Chunk Settings -->
	<div class="chunk-settings">
		<h3>⚙️ Chunk-Einstellungen</h3>
		<div class="settings-grid">
			<div class="setting-item">
				<label for="chunkSize">Chunk-Größe: {chunkSizeSeconds}s</label>
				<input 
					id="chunkSize"
					type="range" 
					bind:value={chunkSizeSeconds} 
					min="2" 
					max="10" 
					step="1"
					disabled={isLiveRecording}
					class="chunk-slider"
				/>
				<div class="setting-hint">Längere Chunks = bessere Sprachenerkennung</div>
			</div>
			<div class="setting-item">
				<label for="overlap">Überlappung: {overlapMs}ms</label>
				<input 
					id="overlap"
					type="range" 
					bind:value={overlapMs} 
					min="100" 
					max="1000" 
					step="100"
					disabled={isLiveRecording}
					class="chunk-slider"
				/>
				<div class="setting-hint">Überlappung für kontinuierliche Transkription</div>
			</div>
		</div>
	</div>

	<!-- Live Recording Controls -->
	<div class="recording-controls">
		<div class="control-buttons">
			{#if !isLiveRecording}
				<button 
					class="start-button"
					on:click={startLiveTranscription}
					disabled={isRunning || availableDevices.length === 0}
				>
					🔴 Live-Transkription starten
				</button>
			{:else}
				<div class="recording-buttons">
					{#if !isPaused}
						<button 
							class="pause-button"
							on:click={pauseLiveTranscription}
						>
							⏸️ Pausieren
						</button>
					{:else}
						<button 
							class="resume-button"
							on:click={resumeLiveTranscription}
						>
							▶️ Fortsetzen
						</button>
					{/if}
					<button 
						class="stop-button"
						on:click={stopLiveTranscription}
					>
						⏹️ Stoppen & Speichern
					</button>
				</div>
			{/if}
		</div>

		<!-- Recording Status -->
		{#if isLiveRecording}
			<div class="recording-status">
				<div class="status-info">
					<div class="recording-indicator">
						<div class="pulse-dot"></div>
						<span>LIVE</span>
					</div>
					<div class="duration">{formatDuration(recordingDuration)}</div>
					<div class="model-used">Modell: {selectedModel}</div>
				</div>
				
				<!-- Volume Visualization -->
				<div class="volume-meter">
					<div class="volume-label">Lautstärke:</div>
					<div class="volume-bar">
						<div 
							class="volume-level" 
							style="width: {Math.min(volumeLevel * 2, 100)}%"
						></div>
					</div>
					<div class="volume-value">{Math.round(volumeLevel)}</div>
				</div>
			</div>
		{/if}
	</div>

	<!-- Live Transcription Display -->
	<div class="transcription-display">
		<h3>📝 Live-Transkription</h3>
		<div class="transcript-container">
			{#if currentTranscript}
				<div class="transcript-text">{currentTranscript}</div>
				{#if isProcessingChunk}
					<div class="processing-indicator">
						<div class="spinner"></div>
						<span>Verarbeite Chunk...</span>
					</div>
				{/if}
			{:else}
				<div class="transcript-placeholder">
					{#if isLiveRecording}
						<div class="waiting-indicator">
							<div class="spinner"></div>
							<span>Warte auf ersten Chunk...</span>
						</div>
					{:else}
						<p>Starten Sie die Live-Transkription, um Echtzeit-Text zu sehen.</p>
					{/if}
				</div>
			{/if}
		</div>
		
		<!-- Chunk Statistics -->
		{#if transcriptionChunks.length > 0}
			<div class="chunk-stats">
				<div class="stat-item">
					<span class="stat-label">Chunks verarbeitet:</span>
					<span class="stat-value">{transcriptionChunks.length}</span>
				</div>
				<div class="stat-item">
					<span class="stat-label">Audio-Größe:</span>
					<span class="stat-value">{Math.round(audioChunks.reduce((sum, chunk) => sum + chunk.size, 0) / 1024)} KB</span>
				</div>
			</div>
		{/if}
	</div>

	<!-- Privacy Notice -->
	<div class="privacy-notice">
		<h4>🔒 Datenschutz & Sicherheit</h4>
		<ul>
			<li>✅ Lokale Verarbeitung (kein Cloud-Upload)</li>
			<li>✅ Verschlüsselte Speicherung nach Aufnahme-Ende</li>
			<li>✅ Automatische Anonymisierung von Patientendaten</li>
			<li>✅ Vollständiger Audit-Trail für medizinische Compliance</li>
		</ul>
	</div>
</div>

<style>
	.live-transcription-tab {
		display: flex;
		flex-direction: column;
		gap: 2rem;
		padding: 1.5rem;
		max-width: 1200px;
		margin: 0 auto;
	}
	
	/* Device Selection */
	.device-selection {
		background: var(--bg-secondary, #f8fafc);
		padding: 1.5rem;
		border-radius: 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.device-selection h3 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1.25rem;
		font-weight: 600;
	}
	
	.device-select {
		width: 100%;
		padding: 0.75rem 1rem;
		border: 1px solid var(--border-color, #d1d5db);
		border-radius: 0.5rem;
		background: white;
		font-size: 1rem;
		color: var(--text-primary, #1f2937);
	}
	
	.device-select:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	.no-devices {
		color: var(--error-color, #dc2626);
		margin: 0.5rem 0 0 0;
		font-size: 0.875rem;
	}
	
	/* Model Selection */
	.model-selection {
		background: var(--bg-secondary, #f8fafc);
		padding: 1.5rem;
		border-radius: 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.model-selection h3 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1.25rem;
		font-weight: 600;
	}
	
	.model-options {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: 1rem;
	}
	
	.model-option {
		display: flex;
		align-items: flex-start;
		gap: 0.75rem;
		padding: 1rem;
		background: white;
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 0.75rem;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.model-option:hover {
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
	}
	
	.model-option.selected {
		border-color: var(--primary-color, #2563eb);
		background: var(--primary-bg, #eff6ff);
	}
	
	.model-option input[type="radio"] {
		margin: 0.25rem 0 0 0;
		accent-color: var(--primary-color, #2563eb);
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
		margin-bottom: 0.5rem;
	}
	
	.model-size,
	.model-ram {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		background: var(--bg-tertiary, #f3f4f6);
		padding: 0.25rem 0.5rem;
		border-radius: 0.25rem;
	}
	
	.model-description {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		line-height: 1.4;
	}
	
	/* Chunk Settings */
	.chunk-settings {
		background: var(--bg-secondary, #f8fafc);
		padding: 1.5rem;
		border-radius: 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.chunk-settings h3 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1.25rem;
		font-weight: 600;
	}
	
	.settings-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
		gap: 1.5rem;
	}
	
	.setting-item {
		background: white;
		padding: 1rem;
		border-radius: 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.setting-item label {
		display: block;
		margin-bottom: 0.5rem;
		font-weight: 600;
		color: var(--text-primary, #1f2937);
	}
	
	.chunk-slider {
		width: 100%;
		margin-bottom: 0.5rem;
		accent-color: var(--primary-color, #2563eb);
	}
	
	.setting-hint {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		line-height: 1.4;
	}
	
	/* Recording Controls */
	.recording-controls {
		background: var(--bg-secondary, #f8fafc);
		padding: 1.5rem;
		border-radius: 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.control-buttons {
		text-align: center;
		margin-bottom: 1rem;
	}
	
	.recording-buttons {
		display: flex;
		gap: 1rem;
		justify-content: center;
		flex-wrap: wrap;
	}
	
	.start-button,
	.pause-button,
	.resume-button,
	.stop-button {
		padding: 1rem 2rem;
		font-size: 1.1rem;
		font-weight: 600;
		border: none;
		border-radius: 0.75rem;
		cursor: pointer;
		transition: all 0.2s ease;
		min-width: 180px;
	}
	
	.start-button {
		background: var(--success-color, #10b981);
		color: white;
	}
	
	.start-button:hover:not(:disabled) {
		background: var(--success-hover, #059669);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
	}
	
	.start-button:disabled {
		background: var(--disabled-color, #9ca3af);
		cursor: not-allowed;
		opacity: 0.6;
	}
	
	.pause-button {
		background: var(--warning-color, #f59e0b);
		color: white;
	}
	
	.pause-button:hover {
		background: var(--warning-hover, #d97706);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(245, 158, 11, 0.3);
	}
	
	.resume-button {
		background: var(--primary-color, #2563eb);
		color: white;
	}
	
	.resume-button:hover {
		background: var(--primary-hover, #1d4ed8);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
	}
	
	.stop-button {
		background: var(--error-color, #dc2626);
		color: white;
	}
	
	.stop-button:hover {
		background: var(--error-hover, #b91c1c);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(220, 38, 38, 0.3);
	}
	
	/* Recording Status */
	.recording-status {
		background: white;
		padding: 1rem;
		border-radius: 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.status-info {
		display: flex;
		align-items: center;
		gap: 1.5rem;
		margin-bottom: 1rem;
		flex-wrap: wrap;
	}
	
	.recording-indicator {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		font-weight: 600;
		color: var(--error-color, #dc2626);
	}
	
	.pulse-dot {
		width: 12px;
		height: 12px;
		background: var(--error-color, #dc2626);
		border-radius: 50%;
		animation: pulse 1.5s infinite;
	}
	
	@keyframes pulse {
		0%, 100% { opacity: 1; }
		50% { opacity: 0.3; }
	}
	
	.duration {
		font-family: 'Courier New', monospace;
		font-size: 1.1rem;
		font-weight: 600;
		color: var(--text-primary, #1f2937);
	}
	
	.model-used {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		background: var(--bg-tertiary, #f3f4f6);
		padding: 0.25rem 0.5rem;
		border-radius: 0.25rem;
	}
	
	/* Volume Meter */
	.volume-meter {
		display: flex;
		align-items: center;
		gap: 0.75rem;
	}
	
	.volume-label {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		min-width: 80px;
	}
	
	.volume-bar {
		flex: 1;
		height: 8px;
		background: var(--bg-tertiary, #f3f4f6);
		border-radius: 4px;
		overflow: hidden;
		max-width: 200px;
	}
	
	.volume-level {
		height: 100%;
		background: linear-gradient(90deg, #10b981, #f59e0b, #dc2626);
		transition: width 0.1s ease;
		border-radius: 4px;
	}
	
	.volume-value {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		min-width: 30px;
		text-align: right;
	}
	
	/* Transcription Display */
	.transcription-display {
		background: var(--bg-secondary, #f8fafc);
		padding: 1.5rem;
		border-radius: 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.transcription-display h3 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1.25rem;
		font-weight: 600;
	}
	
	.transcript-container {
		background: white;
		padding: 1.5rem;
		border-radius: 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		min-height: 150px;
		margin-bottom: 1rem;
		position: relative;
	}
	
	.transcript-text {
		line-height: 1.6;
		color: var(--text-primary, #1f2937);
		font-size: 1rem;
		white-space: pre-wrap;
		word-wrap: break-word;
	}
	
	.transcript-placeholder {
		display: flex;
		align-items: center;
		justify-content: center;
		height: 100%;
		min-height: 120px;
		color: var(--text-secondary, #6b7280);
		text-align: center;
	}
	
	.processing-indicator,
	.waiting-indicator {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		color: var(--primary-color, #2563eb);
		font-size: 0.875rem;
		margin-top: 1rem;
	}
	
	.spinner {
		width: 16px;
		height: 16px;
		border: 2px solid var(--border-color, #e5e7eb);
		border-top: 2px solid var(--primary-color, #2563eb);
		border-radius: 50%;
		animation: spin 1s linear infinite;
	}
	
	@keyframes spin {
		0% { transform: rotate(0deg); }
		100% { transform: rotate(360deg); }
	}
	
	/* Chunk Statistics */
	.chunk-stats {
		display: flex;
		gap: 2rem;
		padding: 1rem;
		background: var(--bg-tertiary, #f3f4f6);
		border-radius: 0.5rem;
		flex-wrap: wrap;
	}
	
	.stat-item {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}
	
	.stat-label {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.stat-value {
		font-weight: 600;
		color: var(--text-primary, #1f2937);
		font-size: 1.1rem;
	}
	
	/* Privacy Notice */
	.privacy-notice {
		background: var(--success-bg, #f0fdf4);
		padding: 1.5rem;
		border-radius: 1rem;
		border: 1px solid var(--success-border, #bbf7d0);
	}
	
	.privacy-notice h4 {
		margin: 0 0 1rem 0;
		color: var(--success-text, #166534);
		font-size: 1.1rem;
		font-weight: 600;
	}
	
	.privacy-notice ul {
		margin: 0;
		padding-left: 1.5rem;
		list-style: none;
	}
	
	.privacy-notice li {
		margin-bottom: 0.5rem;
		color: var(--success-text, #166534);
		line-height: 1.5;
		position: relative;
	}
	
	.privacy-notice li::before {
		content: '';
		position: absolute;
		left: -1.5rem;
		top: 0.1rem;
		width: 4px;
		height: 4px;
		background: var(--success-color, #10b981);
		border-radius: 50%;
	}
	
	/* Responsive Design */
	@media (max-width: 768px) {
		.live-transcription-tab {
			padding: 1rem;
			gap: 1.5rem;
		}
		
		.model-options {
			grid-template-columns: 1fr;
		}
		
		.settings-grid {
			grid-template-columns: 1fr;
			gap: 1rem;
		}
		
		.status-info {
			flex-direction: column;
			align-items: flex-start;
			gap: 0.75rem;
		}
		
		.volume-meter {
			flex-direction: column;
			align-items: flex-start;
			gap: 0.5rem;
		}
		
		.volume-bar {
			max-width: none;
			width: 100%;
		}
		
		.chunk-stats {
			flex-direction: column;
			gap: 1rem;
		}
	}
</style>
