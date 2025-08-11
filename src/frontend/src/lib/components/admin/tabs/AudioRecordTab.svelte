<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { createEventDispatcher, onMount, onDestroy } from 'svelte';
	
	// [WMM] Audio recording tab for benchmark testing
	// [ATV] Audit trail for audio recordings
	// [SP] Secure audio recording with encryption
	
	const dispatch = createEventDispatcher<{
		audioRecorded: { audioBlob: Blob; fileName: string };
	}>();
	
	export let isRecording: boolean = false;
	export let audioRecords: Array<{id: string, fileName: string, created: string, duration: number}> = [];
	
	let mediaRecorder: MediaRecorder | null = null;
	let audioStream: MediaStream | null = null;
	let audioChunks: Blob[] = [];
	let recordingDuration = 0;
	let recordingTimer: number | null = null;
	let audioContext: AudioContext | null = null;
	let analyser: AnalyserNode | null = null;
	let dataArray: Uint8Array | null = null;
	let animationFrame: number | null = null;
	let volumeLevel = 0;
	
	// Audio settings for MedEasy [SF][WMM]
	const audioSettings = {
		sampleRate: 16000,    // 16kHz for Whisper
		channelCount: 1,      // Mono
		bitRate: 256000,      // 256kbps
		format: 'webm'        // WebM for browser compatibility
	};
	
	let availableDevices: MediaDeviceInfo[] = [];
	let selectedDeviceId: string = '';
	
	onMount(() => {
		loadAudioDevices();
		loadAudioRecordings();
	});
	
	onDestroy(() => {
		stopRecording();
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
		} catch (error) {
			console.error('Failed to load audio devices:', error);
		}
	}

	// [ATV][EIV] Load saved audio recordings from backend
	async function loadAudioRecordings() {
		try {
			const response = await fetch('http://localhost:5155/api/ai/audio/recordings');
			
			if (!response.ok) {
				throw new Error(`HTTP error! status: ${response.status}`);
			}
			
			const recordings = await response.json();
			
			// Transform to expected format
			audioRecords = recordings.map(record => ({
				id: record.id,
				fileName: record.fileName,
				created: record.created,
				duration: record.durationSeconds
			}));
			
			console.log('Loaded audio recordings [ATV]:', audioRecords.length);
		} catch (error) {
			console.error('Failed to load audio recordings [ECP]:', error);
			// Don't fail component loading, just log error
		}
	}
	
	async function startRecording() {
		try {
			// Request microphone access with specific device
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
			
			// Setup MediaRecorder
			const options: MediaRecorderOptions = {
				mimeType: 'audio/webm;codecs=opus',
				audioBitsPerSecond: audioSettings.bitRate
			};
			
			mediaRecorder = new MediaRecorder(audioStream, options);
			audioChunks = [];
			
			mediaRecorder.ondataavailable = (event) => {
				if (event.data.size > 0) {
					audioChunks.push(event.data);
				}
			};
			
			mediaRecorder.onstop = () => {
				processRecording();
			};
			
			mediaRecorder.start(100); // Collect data every 100ms
			isRecording = true;
			recordingDuration = 0;
			
			// Start timer
			recordingTimer = setInterval(() => {
				recordingDuration++;
			}, 1000);
			
		} catch (error) {
			console.error('Failed to start recording:', error);
			alert('Fehler beim Starten der Aufnahme. Bitte überprüfen Sie die Mikrofon-Berechtigung.');
		}
	}
	
	function stopRecording() {
		if (mediaRecorder && isRecording) {
			mediaRecorder.stop();
			isRecording = false;
			
			if (recordingTimer) {
				clearInterval(recordingTimer);
				recordingTimer = null;
			}
			
			if (audioStream) {
				audioStream.getTracks().forEach(track => track.stop());
				audioStream = null;
			}
			
			if (animationFrame) {
				cancelAnimationFrame(animationFrame);
				animationFrame = null;
			}
		}
	}
	
	function setupAudioAnalysis() {
		if (!audioStream) return;
		
		audioContext = new AudioContext();
		analyser = audioContext.createAnalyser();
		const source = audioContext.createMediaStreamSource(audioStream);
		
		analyser.fftSize = 256;
		dataArray = new Uint8Array(analyser.frequencyBinCount);
		
		source.connect(analyser);
		updateVolumeLevel();
	}
	
	function updateVolumeLevel() {
		if (!analyser || !dataArray) return;
		
		analyser.getByteFrequencyData(dataArray);
		
		// Calculate average volume
		let sum = 0;
		for (let i = 0; i < dataArray.length; i++) {
			sum += dataArray[i];
		}
		volumeLevel = sum / dataArray.length / 255; // Normalize to 0-1
		
		if (isRecording) {
			animationFrame = requestAnimationFrame(updateVolumeLevel);
		}
	}
	
	async function processRecording() {
		if (audioChunks.length === 0) return;
		
		const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
		const fileName = `recording_${new Date().toISOString().replace(/[:.]/g, '-')}.webm`;
		
		// [SP][EIV][ATV] Send to backend for encryption and storage
		try {
			const formData = new FormData();
			formData.append('audioFile', audioBlob, fileName);
			formData.append('fileName', fileName);
			formData.append('duration', recordingDuration.toString());
			
			const response = await fetch('http://localhost:5155/api/ai/audio/save-recording', {
				method: 'POST',
				body: formData
			});
			
			if (!response.ok) {
				throw new Error(`HTTP error! status: ${response.status}`);
			}
			
			const savedRecord = await response.json();
			console.log('Audio recording saved successfully [ATV]:', savedRecord);
			
			// Add to local records list for UI
			const newRecord = {
				id: savedRecord.id,
				fileName: savedRecord.fileName,
				created: savedRecord.created,
				duration: savedRecord.durationSeconds
			};
			
			// Update audioRecords array
			audioRecords = [newRecord, ...audioRecords];
			
			// Dispatch event for parent component
			dispatch('audioRecorded', { 
				audioBlob, 
				fileName,
				audioRecordId: savedRecord.id,
				savedRecord: newRecord
			});
			
		} catch (error) {
			console.error('Failed to save audio recording [ECP]:', error);
			// Still dispatch event for fallback handling
			dispatch('audioRecorded', { audioBlob, fileName, error: error.message });
		}
		
		// Reset
		audioChunks = [];
		recordingDuration = 0;
		volumeLevel = 0;
	}
	
	function formatDuration(seconds: number): string {
		const mins = Math.floor(seconds / 60);
		const secs = seconds % 60;
		return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
	}
	
	function formatFileSize(bytes: number): string {
		if (bytes === 0) return '0 Bytes';
		const k = 1024;
		const sizes = ['Bytes', 'KB', 'MB', 'GB'];
		const i = Math.floor(Math.log(bytes) / Math.log(k));
		return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
	}
</script>

<div class="audio-record-tab">
	<!-- Microphone Selection -->
	<div class="device-selection">
		<h4>🎤 Mikrofon auswählen</h4>
		<select bind:value={selectedDeviceId} class="device-dropdown" disabled={isRecording}>
			{#each availableDevices as device}
				<option value={device.deviceId}>
					{device.label || `Mikrofon ${device.deviceId.slice(0, 8)}...`}
				</option>
			{/each}
		</select>
		{#if availableDevices.length === 0}
			<p class="no-devices">Keine Mikrofone gefunden. Bitte überprüfen Sie die Berechtigung.</p>
		{/if}
	</div>
	
	<!-- Recording Controls -->
	<div class="recording-controls">
		<h4>🎙️ Audio-Aufnahme</h4>
		
		<div class="recorder-interface">
			<!-- Volume Visualization -->
			<div class="volume-meter">
				<div class="volume-bar">
					<div 
						class="volume-fill" 
						style="width: {volumeLevel * 100}%"
						class:recording={isRecording}
					></div>
				</div>
				<span class="volume-label">
					{isRecording ? 'Aufnahme läuft' : 'Bereit'}
				</span>
			</div>
			
			<!-- Recording Timer -->
			{#if isRecording || recordingDuration > 0}
				<div class="recording-timer">
					<span class="timer-icon">⏱️</span>
					<span class="timer-text">{formatDuration(recordingDuration)}</span>
				</div>
			{/if}
			
			<!-- Control Buttons -->
			<div class="control-buttons">
				{#if !isRecording}
					<button 
						class="record-button start" 
						on:click={startRecording}
						disabled={availableDevices.length === 0}
					>
						<span class="button-icon">🔴</span>
						Aufnahme starten
					</button>
				{:else}
					<button class="record-button stop" on:click={stopRecording}>
						<span class="button-icon">⏹️</span>
						Aufnahme stoppen
					</button>
				{/if}
			</div>
		</div>
	</div>
	
	<!-- Audio Settings Info -->
	<div class="audio-settings-info">
		<h5>⚙️ Aufnahme-Einstellungen (MedEasy optimiert):</h5>
		<div class="settings-grid">
			<div class="setting-item">
				<span class="setting-label">Format:</span>
				<span class="setting-value">WebM (Opus)</span>
			</div>
			<div class="setting-item">
				<span class="setting-label">Abtastrate:</span>
				<span class="setting-value">{audioSettings.sampleRate / 1000}kHz</span>
			</div>
			<div class="setting-item">
				<span class="setting-label">Kanäle:</span>
				<span class="setting-value">Mono</span>
			</div>
			<div class="setting-item">
				<span class="setting-label">Bitrate:</span>
				<span class="setting-value">{audioSettings.bitRate / 1000}kbps</span>
			</div>
		</div>
	</div>
	
	<!-- Recording Tips -->
	<div class="recording-tips">
		<h5>💡 Tipps für optimale Aufnahmen:</h5>
		<ul>
			<li><strong>Umgebung:</strong> Ruhiger Raum ohne Hintergrundgeräusche</li>
			<li><strong>Mikrofon:</strong> 15-20cm Abstand zum Mund</li>
			<li><strong>Sprache:</strong> Deutlich und in normalem Tempo sprechen</li>
			<li><strong>Dauer:</strong> 5-30 Sekunden für Benchmark-Tests optimal</li>
			<li><strong>Inhalt:</strong> Medizinische Fachbegriffe für realistische Tests</li>
		</ul>
	</div>
</div>

<style>
	.audio-record-tab {
		display: flex;
		flex-direction: column;
		gap: 1.5rem;
	}
	
	/* Device Selection */
	.device-selection h4,
	.recording-controls h4 {
		margin: 0 0 1rem 0;
		color: var(--text-primary, #1f2937);
		font-size: 1rem;
		font-weight: 600;
	}
	
	.device-dropdown {
		width: 100%;
		padding: 0.75rem;
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 0.5rem;
		font-size: 0.9rem;
		background: white;
	}
	
	.device-dropdown:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	.device-dropdown:disabled {
		background: var(--gray-100, #f3f4f6);
		cursor: not-allowed;
	}
	
	.no-devices {
		color: var(--warning-color, #f59e0b);
		font-size: 0.9rem;
		margin: 0.5rem 0 0 0;
	}
	
	/* Recording Interface */
	.recorder-interface {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 1.5rem;
		padding: 2rem;
		background: var(--bg-secondary, #f8fafc);
		border-radius: 1rem;
		border: 2px solid var(--border-color, #e5e7eb);
	}
	
	/* Volume Meter */
	.volume-meter {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.5rem;
		width: 100%;
		max-width: 300px;
	}
	
	.volume-bar {
		width: 100%;
		height: 8px;
		background: var(--gray-200, #e5e7eb);
		border-radius: 4px;
		overflow: hidden;
	}
	
	.volume-fill {
		height: 100%;
		background: var(--success-color, #10b981);
		transition: width 0.1s ease;
		border-radius: 4px;
	}
	
	.volume-fill.recording {
		background: var(--primary-color, #2563eb);
		animation: pulse 1s infinite;
	}
	
	@keyframes pulse {
		0%, 100% { opacity: 1; }
		50% { opacity: 0.7; }
	}
	
	.volume-label {
		font-size: 0.9rem;
		color: var(--text-secondary, #6b7280);
		font-weight: 500;
	}
	
	/* Recording Timer */
	.recording-timer {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		font-size: 1.2rem;
		font-weight: 600;
		color: var(--primary-color, #2563eb);
	}
	
	.timer-icon {
		font-size: 1.4rem;
	}
	
	/* Control Buttons */
	.control-buttons {
		display: flex;
		gap: 1rem;
	}
	
	.record-button {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		padding: 1rem 1.5rem;
		border: none;
		border-radius: 0.75rem;
		font-size: 1rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.2s ease;
	}
	
	.record-button.start {
		background: var(--success-color, #10b981);
		color: white;
	}
	
	.record-button.start:hover:not(:disabled) {
		background: var(--success-hover, #059669);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
	}
	
	.record-button.stop {
		background: var(--error-color, #ef4444);
		color: white;
	}
	
	.record-button.stop:hover {
		background: var(--error-hover, #dc2626);
		transform: translateY(-1px);
		box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
	}
	
	.record-button:disabled {
		background: var(--gray-400, #9ca3af);
		cursor: not-allowed;
		transform: none;
		box-shadow: none;
	}
	
	.button-icon {
		font-size: 1.2rem;
	}
	
	/* Audio Settings Info */
	.audio-settings-info {
		background: var(--info-bg, #eff6ff);
		border: 1px solid var(--info-border, #bfdbfe);
		border-radius: 0.75rem;
		padding: 1.5rem;
		display: none;
	}
	
	.audio-settings-info h5 {
		margin: 0 0 1rem 0;
		color: var(--info-text, #1e40af);
		font-size: 0.95rem;
		font-weight: 600;
	}
	
	.settings-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
		gap: 0.75rem;
	}
	
	.setting-item {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.5rem;
		background: white;
		border-radius: 0.5rem;
		border: 1px solid var(--info-border, #bfdbfe);
	}
	
	.setting-label {
		font-weight: 500;
		color: var(--text-secondary, #6b7280);
		font-size: 0.85rem;
	}
	
	.setting-value {
		font-weight: 600;
		color: var(--info-text, #1e40af);
		font-size: 0.85rem;
	}
	
	/* Recording Tips */
	.recording-tips {
		background: var(--warning-bg, #fffbeb);
		border: 1px solid var(--warning-border, #fed7aa);
		border-radius: 0.75rem;
		padding: 1.5rem;
	}
	
	.recording-tips h5 {
		margin: 0 0 1rem 0;
		color: var(--warning-text, #92400e);
		font-size: 0.95rem;
		font-weight: 600;
	}
	
	.recording-tips ul {
		margin: 0;
		padding-left: 1.5rem;
		color: var(--text-secondary, #6b7280);
		font-size: 0.9rem;
		line-height: 1.6;
	}
	
	.recording-tips li {
		margin-bottom: 0.5rem;
	}
	
	.recording-tips li:last-child {
		margin-bottom: 0;
	}
</style>
