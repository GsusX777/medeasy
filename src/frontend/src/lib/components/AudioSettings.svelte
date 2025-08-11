<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [WMM] Whisper Multi-Model - Audio-Einstellungen für Transkription
  [PSF] Patient Safety First - Medizinisch relevante Audio-Qualität
  [SF] Schweizer Formate - Deutsche Sprache
  [TSF] Tauri 1.5 + Svelte 4 Stack
  [ZTS] Zero Tolerance Security - Mikrofon-Zugriff nur mit Berechtigung
-->
<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { audioStore } from '$lib/stores/audio';
  import type { AudioDevice, AudioQuality } from '$lib/types/audio';
  
  // Audio Device Management [WMM][PSF]
  let availableDevices: AudioDevice[] = [];
  let selectedDeviceId: string = '';
  let devicePermissionGranted = false;
  let deviceError: string | null = null;
  
  // Audio Quality Settings [PSF][WMM]
  let audioQuality: AudioQuality = 'high';
  let sampleRate = 44100;
  let bitDepth = 16;
  let channels = 1; // Mono für medizinische Aufnahmen
  
  // Noise Reduction & Processing [PSF]
  let noiseReduction = true;
  let echoCancellation = true;
  let autoGainControl = true;
  let noiseSuppression = true;
  
  // Medical Audio Settings [PSF][SF]
  let medicalMode = true; // Optimiert für Sprache, nicht Musik
  let swissGermanOptimization = true;
  let lowLatencyMode = false; // Für Echtzeit-Feedback
  
  // Whisper Provider Settings [WMM] - Nur lokale Modelle
  let whisperProvider: 'local' = 'local'; // Nur lokal
  let whisperModel = 'small';
  let whisperLanguage = 'de'; // Deutsch als Standard
  
  // Audio Level Monitoring [PSF]
  let inputGain = 1.0;
  let outputGain = 1.0;
  let silenceThreshold = -50; // dB
  let maxRecordingLength = 3600; // Sekunden (1 Stunde)
  
  // Settings State [ZTS]
  let settingsChanged = false;
  let saving = false;
  let lastSaved: string | null = null;
  let autoSaveTimeoutId: number | null = null;
  
  onMount(async () => {
    // Browser-Check für SSR-Kompatibilität [ZTS]
    if (typeof window !== 'undefined') {
      await loadAudioSettings();
      await enumerateAudioDevices();
    }
  });
  
  onDestroy(() => {
    if (autoSaveTimeoutId) {
      clearTimeout(autoSaveTimeoutId);
    }
    // Stop audio monitoring on component destroy
    stopAudioMonitoring();
  });
  
  // Reactive statement to start/stop audio monitoring based on device selection
  $: if (selectedDeviceId && devicePermissionGranted && typeof window !== 'undefined') {
    startAudioMonitoring();
  } else if (!selectedDeviceId && audioMonitoring) {
    stopAudioMonitoring();
  }
  
  // Audio Device Enumeration [ZTS][PSF]
  async function enumerateAudioDevices() {
    try {
      // Browser-Check für SSR-Kompatibilität [ZTS]
      if (typeof window === 'undefined' || !navigator.mediaDevices) {
        console.warn('MediaDevices API nicht verfügbar (SSR oder unsupported browser)');
        return;
      }
      
      // Request microphone permission first [ZTS]
      const stream = await navigator.mediaDevices.getUserMedia({ 
        audio: {
          echoCancellation: echoCancellation,
          noiseSuppression: noiseSuppression,
          autoGainControl: autoGainControl,
          sampleRate: sampleRate,
          channelCount: channels
        } 
      });
      
      devicePermissionGranted = true;
      stream.getTracks().forEach(track => track.stop()); // Clean up
      
      // Enumerate available devices
      const devices = await navigator.mediaDevices.enumerateDevices();
      availableDevices = devices
        .filter(device => device.kind === 'audioinput')
        .map(device => ({
          id: device.deviceId,
          label: device.label || `Mikrofon ${device.deviceId.slice(0, 8)}`,
          kind: device.kind as 'audioinput'
        }));
      
      // Set default device if none selected
      if (!selectedDeviceId && availableDevices.length > 0) {
        selectedDeviceId = availableDevices[0].id;
        settingsChanged = true;
      }
      
      deviceError = null;
    } catch (error) {
      console.error('Fehler beim Zugriff auf Mikrofon:', error);
      deviceError = error instanceof Error ? error.message : 'Unbekannter Fehler';
      devicePermissionGranted = false;
    }
  }
  
  // Load Settings from Store [ZTS]
  async function loadAudioSettings() {
    try {
      const settings = audioStore.getSettings();
      selectedDeviceId = settings.deviceId || '';
      audioQuality = settings.quality || 'high';
      noiseReduction = settings.noiseReduction ?? true;
      echoCancellation = settings.echoCancellation ?? true;
      autoGainControl = settings.autoGainControl ?? true;
      noiseSuppression = settings.noiseSuppression ?? true;
      medicalMode = settings.medicalMode ?? true;
      swissGermanOptimization = settings.swissGermanOptimization ?? true;
      inputGain = settings.inputGain ?? 1.0;
      outputGain = settings.outputGain ?? 1.0;
      silenceThreshold = settings.silenceThreshold ?? -50;
      
      // Set quality-specific settings
      updateQualitySettings();
    } catch (error) {
      console.error('Fehler beim Laden der Audio-Einstellungen:', error);
    }
  }
  
  // Save Settings to Store [ZTS]
  async function saveAudioSettings() {
    if (!settingsChanged) {
      return;
    }
    
    saving = true;
    try {
      await audioStore.updateSettings({
        deviceId: selectedDeviceId,
        quality: audioQuality,
        sampleRate,
        bitDepth,
        channels,
        noiseReduction,
        echoCancellation,
        autoGainControl,
        noiseSuppression,
        medicalMode,
        swissGermanOptimization,
        lowLatencyMode,
        inputGain,
        outputGain,
        silenceThreshold,
        maxRecordingLength
      });
      
      settingsChanged = false;
      lastSaved = new Date().toLocaleString('de-CH');
    } catch (error) {
      console.error('Fehler beim Speichern der Audio-Einstellungen:', error);
    } finally {
      saving = false;
    }
  }
  
  // Update quality-specific settings [PSF][WMM]
  function updateQualitySettings() {
    switch (audioQuality) {
      case 'low':
        sampleRate = 16000;
        bitDepth = 16;
        break;
      case 'medium':
        sampleRate = 22050;
        bitDepth = 16;
        break;
      case 'high':
        sampleRate = 44100;
        bitDepth = 16;
        break;
      case 'ultra':
        sampleRate = 48000;
        bitDepth = 24;
        break;
    }
    settingsChanged = true;
  }
  
  // Continuous Audio Monitoring [PSF] - Dauerhaft aktiv bei geöffneten Einstellungen
  let audioMonitoring = false;
  let currentAudioLevel = 0;
  let currentPeakLevel = 0;
  let currentClipping = false;
  let audioStream: MediaStream | null = null;
  let audioContext: AudioContext | null = null;
  let analyser: AnalyserNode | null = null;
  let animationFrame: number | null = null;
  
  // Start continuous audio monitoring [PSF]
  async function startAudioMonitoring() {
    if (!selectedDeviceId || audioMonitoring) {
      return;
    }
    
    // Browser-Check für SSR-Kompatibilität [ZTS]
    if (typeof window === 'undefined' || !navigator.mediaDevices) {
      console.warn('MediaDevices API nicht verfügbar');
      return;
    }
    
    try {
      audioStream = await navigator.mediaDevices.getUserMedia({
        audio: {
          deviceId: { exact: selectedDeviceId },
          echoCancellation: echoCancellation,
          noiseSuppression: noiseSuppression,
          autoGainControl: autoGainControl,
          sampleRate: sampleRate,
          channelCount: channels
        }
      });
      
      // Audio Context für Level-Analyse [PSF]
      audioContext = new AudioContext();
      analyser = audioContext.createAnalyser();
      const source = audioContext.createMediaStreamSource(audioStream);
      source.connect(analyser);
      
      analyser.fftSize = 256;
      const bufferLength = analyser.frequencyBinCount;
      const dataArray = new Uint8Array(bufferLength);
      
      audioMonitoring = true;
      
      // Continuous Audio Level Monitoring
      function updateAudioLevels() {
        if (!analyser || !audioMonitoring) return;
        
        analyser.getByteFrequencyData(dataArray);
        
        // Calculate RMS level
        let sum = 0;
        let peak = 0;
        for (let i = 0; i < dataArray.length; i++) {
          const value = dataArray[i] / 255.0;
          sum += value * value;
          peak = Math.max(peak, value);
        }
        
        const rms = Math.sqrt(sum / dataArray.length);
        currentAudioLevel = Math.round(rms * 100);
        currentPeakLevel = Math.round(peak * 100);
        currentClipping = peak > 0.95; // Clipping bei >95%
        
        // Update globalen Audio-Store für Sidebar [PSF]
        const decibels = rms > 0 ? 20 * Math.log10(rms) : -60;
        audioStore.updateAudioLevel({
          volume: rms,
          peak: peak,
          rms: rms,
          decibels: decibels,
          isActive: true,
          isSilent: rms < 0.01,
          isClipping: currentClipping,
          timestamp: Date.now()
        });
        
        if (audioMonitoring) {
          animationFrame = requestAnimationFrame(updateAudioLevels);
        }
      }
      
      updateAudioLevels();
      
    } catch (error) {
      console.error('Audio-Monitoring fehlgeschlagen:', error);
      audioMonitoring = false;
    }
  }
  
  // Stop audio monitoring [PSF]
  function stopAudioMonitoring() {
    audioMonitoring = false;
    
    if (animationFrame) {
      cancelAnimationFrame(animationFrame);
      animationFrame = null;
    }
    
    if (audioStream) {
      audioStream.getTracks().forEach(track => track.stop());
      audioStream = null;
    }
    
    if (audioContext) {
      audioContext.close();
      audioContext = null;
    }
    
    analyser = null;
    currentAudioLevel = 0;
    currentPeakLevel = 0;
    currentClipping = false;
  }
  
  // Mark settings as changed
  function markChanged() {
    settingsChanged = true;
  }
  
  // Auto-save on change - Reactive statement
  $: if (settingsChanged && typeof window !== 'undefined') {
    // Clear existing timeout
    if (autoSaveTimeoutId) {
      clearTimeout(autoSaveTimeoutId);
    }
    
    // Set new timeout
    autoSaveTimeoutId = setTimeout(() => {
      saveAudioSettings();
    }, 1000); // Auto-save after 1 second
  }
</script>

<div class="audio-settings">
  <!-- Header [SF] -->
  <div class="settings-header">
    <h2>🎤 Audio-Einstellungen</h2>
    <div class="settings-status">
      {#if saving}
        <span class="status saving">Speichern...</span>
      {:else if settingsChanged}
        <span class="status changed">Nicht gespeichert</span>
      {:else if lastSaved}
        <span class="status saved">Gespeichert: {lastSaved}</span>
      {/if}
    </div>
  </div>
  
  <!-- Device Selection [ZTS][PSF] -->
  <div class="settings-section">
    <h3>🎧 Mikrofon-Auswahl</h3>
    
    {#if !devicePermissionGranted}
      <div class="permission-warning">
        <p>⚠️ Mikrofon-Berechtigung erforderlich</p>
        <button class="btn-primary" on:click={enumerateAudioDevices}>
          Berechtigung anfordern
        </button>
      </div>
    {:else if deviceError}
      <div class="error-message">
        <p>❌ Fehler: {deviceError}</p>
        <button class="btn-secondary" on:click={enumerateAudioDevices}>
          Erneut versuchen
        </button>
      </div>
    {:else}
      <div class="device-selection">
        <label for="device-select">Mikrofon:</label>
        <select 
          id="device-select" 
          bind:value={selectedDeviceId} 
          on:change={markChanged}
          class="device-dropdown"
        >
          <option value="">Bitte wählen...</option>
          {#each availableDevices as device}
            <option value={device.id}>{device.label}</option>
          {/each}
        </select>
      </div>
      
      {#if selectedDeviceId}
        <!-- Live Audio Monitor [PSF] - Unterhalb der Mikrofon-Auswahl -->
        <div class="audio-monitor">
          <div class="monitor-header">
            <h4>🎤 Live Audio-Monitor</h4>
            <div class="monitor-status">
              {#if audioMonitoring}
                <span class="status-active">🟢 Aktiv</span>
              {:else}
                <span class="status-inactive">🔴 Inaktiv</span>
              {/if}
            </div>
          </div>
          
          <div class="audio-level-display">
            <div class="level-bar-container">
              <div class="level-bar">
                <div 
                  class="level-fill {currentClipping ? 'clipping' : ''}" 
                  style="width: {currentAudioLevel}%"
                ></div>
                <div 
                  class="peak-indicator" 
                  style="left: {Math.min(100, currentPeakLevel)}%"
                ></div>
              </div>
              <div class="level-scale">
                <span>0%</span>
                <span>50%</span>
                <span>100%</span>
              </div>
            </div>
            
            <div class="level-values">
              <div class="value-item">
                <span class="value-label">RMS:</span>
                <span class="value-number">{currentAudioLevel}%</span>
              </div>
              <div class="value-item">
                <span class="value-label">Peak:</span>
                <span class="value-number">{currentPeakLevel}%</span>
              </div>
              <div class="value-item">
                <span class="value-label">dB:</span>
                <span class="value-number">{Math.round(20 * Math.log10(Math.max(0.001, currentAudioLevel / 100)))}dB</span>
              </div>
              {#if currentClipping}
                <div class="clipping-alert">
                  <span class="alert-icon">⚠️</span>
                  <span class="alert-text">Übersteuerung!</span>
                </div>
              {/if}
            </div>
          </div>
        </div>
      {/if}
    {/if}
  </div>
  
  <!-- Audio Quality [PSF][WMM] -->
  <div class="settings-section">
    <h3>🎵 Audio-Qualität</h3>
    
    <div class="quality-grid">
      <div class="quality-option">
        <label for="quality-select">Qualität:</label>
        <select 
          id="quality-select" 
          bind:value={audioQuality} 
          on:change={() => { updateQualitySettings(); markChanged(); }}
          class="quality-dropdown"
        >
          <option value="low">Niedrig (16 kHz) - Schnell</option>
          <option value="medium">Mittel (22 kHz) - Ausgewogen</option>
          <option value="high">Hoch (44 kHz) - Empfohlen</option>
          <option value="ultra">Ultra (48 kHz) - Beste Qualität</option>
        </select>
      </div>
      
      <div class="quality-details">
        <span>Sample Rate: {sampleRate} Hz</span>
        <span>Bit Depth: {bitDepth} Bit</span>
        <span>Kanäle: {channels} (Mono)</span>
      </div>
    </div>
  </div>
  
  <!-- Whisper Provider Selection [WMM] - Nur lokale Modelle -->
  <div class="settings-section">
    <h3>🤖 Whisper-Modell (Lokal)</h3>
    
    <div class="provider-selection">
      <div class="provider-details">
        <div class="detail-item">
          <span class="detail-label">Modell:</span>
          <select bind:value={whisperModel} on:change={markChanged} class="provider-dropdown">

            <option value="small">Small (Hohe Genauigkeit)</option>
            <option value="medium">Medium (Professionelle Nutzung)</option>
            <option value="large-v3">Large-v3 (Beste medizinische Terminologie)</option>
          </select>
        </div>
        <div class="detail-item">
          <span class="detail-label">Sprache:</span>
          <select bind:value={whisperLanguage} on:change={markChanged} class="provider-dropdown">
            <option value="de">Deutsch (Hochdeutsch)</option>
            <option value="auto">Automatische Erkennung</option>
          </select>
        </div>
        <div class="provider-info">
          <span class="info-icon">ℹ️</span>
          <span>Offline verfügbar, keine Cloud-Verbindung nötig. Schweizerdeutsch wird als Hochdeutsch interpretiert.</span>
        </div>
      </div>
    </div>
  </div>
  
  <!-- Noise Reduction & Processing [PSF] -->
  <div class="settings-section">
    <h3>🔇 Rauschunterdrückung</h3>
    
    <div class="processing-options">
      <label class="checkbox-label mock-feature" title="Erweiterte Rauschunterdrückung noch nicht implementiert">
        <input 
          type="checkbox" 
          bind:checked={noiseReduction} 
          on:change={markChanged}
          disabled
        />
        <span>Rauschunterdrückung aktivieren</span>
        <span class="mock-badge">MOCK</span>
      </label>
      
      <label class="checkbox-label real-feature" title="Browser-native Echo-Unterdrückung (funktional)">
        <input 
          type="checkbox" 
          bind:checked={echoCancellation} 
          on:change={markChanged}
        />
        <span>Echo-Unterdrückung</span>
        <span class="real-badge">ECHT</span>
      </label>
      
      <label class="checkbox-label real-feature" title="Browser-native automatische Verstärkung (funktional)">
        <input 
          type="checkbox" 
          bind:checked={autoGainControl} 
          on:change={markChanged}
        />
        <span>Automatische Verstärkungsregelung</span>
        <span class="real-badge">ECHT</span>
      </label>
      
      <label class="checkbox-label real-feature" title="Browser-native Geräuschunterdrückung (funktional)">
        <input 
          type="checkbox" 
          bind:checked={noiseSuppression} 
          on:change={markChanged}
        />
        <span>Geräuschunterdrückung</span>
        <span class="real-badge">ECHT</span>
      </label>
    </div>
  </div>
  
  <!-- Medical Audio Settings [PSF][SF] -->
  <div class="settings-section medical-section">
    <h3>🏥 Medizinische Einstellungen</h3>
    
    <div class="medical-options">
      <label class="checkbox-label mock-feature" title="Medizinischer Modus noch nicht implementiert - derzeit nur UI-Toggle">
        <input 
          type="checkbox" 
          bind:checked={medicalMode} 
          on:change={markChanged}
          disabled
        />
        <span>Medizinischer Modus (Sprache optimiert)</span>
        <span class="mock-badge">MOCK</span>
      </label>
      
      <label class="checkbox-label mock-feature" title="Schweizerdeutsch-Optimierung noch nicht implementiert - siehe Whisper-Limitationen">
        <input 
          type="checkbox" 
          bind:checked={swissGermanOptimization} 
          on:change={markChanged}
          disabled
        />
        <span>Schweizerdeutsch-Optimierung</span>
        <span class="mock-badge">MOCK</span>
      </label>
      
      <label class="checkbox-label mock-feature" title="Niedrige Latenz noch nicht implementiert - AudioContext-Optimierung erforderlich">
        <input 
          type="checkbox" 
          bind:checked={lowLatencyMode} 
          on:change={markChanged}
          disabled
        />
        <span>Niedrige Latenz (Echtzeit-Feedback)</span>
        <span class="mock-badge">MOCK</span>
      </label>
    </div>
  </div>
  
  <!-- Audio Levels [PSF] -->
  <div class="settings-section">
    <h3>🎚️ Audio-Pegel</h3>
    
    <div class="level-controls">
      <div class="level-control">
        <label for="input-gain">Eingangsverstärkung: {inputGain.toFixed(1)}x</label>
        <input 
          id="input-gain"
          type="range" 
          min="0.1" 
          max="3.0" 
          step="0.1" 
          bind:value={inputGain} 
          on:input={markChanged}
          class="gain-slider"
        />
      </div>
      
      <div class="level-control">
        <label for="silence-threshold">Stille-Schwellwert: {silenceThreshold} dB</label>
        <input 
          id="silence-threshold"
          type="range" 
          min="-80" 
          max="-10" 
          step="5" 
          bind:value={silenceThreshold} 
          on:input={markChanged}
          class="threshold-slider"
        />
      </div>
      
      <div class="level-control">
        <label for="max-length">Max. Aufnahmedauer: {Math.floor(maxRecordingLength / 60)} Min</label>
        <input 
          id="max-length"
          type="range" 
          min="300" 
          max="7200" 
          step="300" 
          bind:value={maxRecordingLength} 
          on:input={markChanged}
          class="duration-slider"
        />
      </div>
    </div>
  </div>
  
  <!-- Action Buttons [ZTS] -->
  <div class="settings-actions">
    <button 
      class="btn-primary" 
      on:click={saveAudioSettings}
      disabled={!settingsChanged || saving}
    >
      {saving ? 'Speichern...' : 'Einstellungen speichern'}
    </button>
    <button 
      class="btn-secondary" 
      on:click={loadAudioSettings}
      disabled={saving}
    >
      Zurücksetzen
    </button>
  </div>
</div>

<style>
  .audio-settings {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    background: var(--bg-secondary, #f8f9fa);
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
  
  .settings-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 30px;
    padding-bottom: 15px;
    border-bottom: 2px solid var(--border-color, #e9ecef);
  }
  
  .settings-header h2 {
    margin: 0;
    color: var(--text-primary, #212529);
    font-size: 1.8rem;
  }
  
  .settings-status .status {
    padding: 4px 12px;
    border-radius: 20px;
    font-size: 0.85rem;
    font-weight: 500;
  }
  
  .status.saving {
    background: var(--warning-light, #fff3cd);
    color: var(--warning-dark, #856404);
  }
  
  .status.changed {
    background: var(--info-light, #d1ecf1);
    color: var(--info-dark, #0c5460);
  }
  
  .status.saved {
    background: var(--success-light, #d4edda);
    color: var(--success-dark, #155724);
  }
  
  .settings-section {
    margin-bottom: 30px;
    padding: 20px;
    background: white;
    border-radius: 8px;
    border: 1px solid var(--border-color, #e9ecef);
  }
  
  .settings-section h3 {
    margin: 0 0 20px 0;
    color: var(--text-primary, #212529);
    font-size: 1.3rem;
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .permission-warning,
  .error-message {
    text-align: center;
    padding: 20px;
    border-radius: 8px;
    margin-bottom: 15px;
  }
  
  .permission-warning {
    background: var(--warning-light, #fff3cd);
    border: 1px solid var(--warning, #ffc107);
  }
  
  .error-message {
    background: var(--danger-light, #f8d7da);
    border: 1px solid var(--danger, #dc3545);
  }
  
  .device-selection {
    display: flex;
    align-items: center;
    gap: 15px;
    flex-wrap: wrap;
  }
  
  .device-dropdown,
  .quality-dropdown {
    flex: 1;
    min-width: 200px;
    padding: 8px 12px;
    border: 1px solid var(--border-color, #ced4da);
    border-radius: 6px;
    font-size: 1rem;
  }
  
  .quality-grid {
    display: grid;
    gap: 15px;
  }
  
  .quality-details {
    display: flex;
    gap: 20px;
    font-size: 0.9rem;
    color: var(--text-secondary, #6c757d);
  }
  
  .processing-options,
  .medical-options {
    display: grid;
    gap: 15px;
  }
  
  .checkbox-label {
    display: flex;
    align-items: center;
    gap: 10px;
    cursor: pointer;
    font-size: 1rem;
  }
  
  .checkbox-label input[type="checkbox"] {
    width: 18px;
    height: 18px;
  }
  
  .checkbox-label input[type="checkbox"]:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
  
  .medical-section {
    border-left: 4px solid var(--primary, #3B82F6);
  }
  
  .level-controls {
    display: grid;
    gap: 20px;
  }
  
  .level-control {
    display: grid;
    gap: 8px;
  }
  
  .level-control label {
    font-weight: 500;
    color: var(--text-primary, #212529);
  }
  
  .gain-slider,
  .threshold-slider,
  .duration-slider {
    width: 100%;
    height: 6px;
    border-radius: 3px;
    background: var(--bg-tertiary, #e9ecef);
    outline: none;
    cursor: pointer;
  }
  
  .settings-actions {
    display: flex;
    gap: 15px;
    justify-content: center;
    margin-top: 30px;
    padding-top: 20px;
    border-top: 1px solid var(--border-color, #e9ecef);
  }
  
  .btn-primary,
  .btn-secondary,
  .btn-test {
    padding: 10px 20px;
    border: none;
    border-radius: 6px;
    font-size: 1rem;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  
  .btn-primary {
    background: var(--primary, #3B82F6);
    color: white;
  }
  
  .btn-primary:hover:not(:disabled) {
    background: var(--primary-dark, #2563EB);
  }
  
  .btn-primary:disabled {
    background: var(--bg-tertiary, #e9ecef);
    color: var(--text-muted, #6c757d);
    cursor: not-allowed;
  }
  
  .btn-secondary {
    background: var(--bg-tertiary, #e9ecef);
    color: var(--text-primary, #212529);
  }
  
  .btn-secondary:hover {
    background: var(--color-secondary-hover);
    transform: translateY(-1px);
  }
  
  .btn-test {
    background: var(--success, #10B981);
    color: white;
  }
  
  .btn-test:hover {
    background: var(--success-dark, #059669);
  }
  
  @media (max-width: 768px) {
    .audio-settings {
      padding: 15px;
      margin: 10px;
    }
    
    .settings-header {
      flex-direction: column;
      gap: 10px;
      align-items: flex-start;
    }
    
    .device-selection {
      flex-direction: column;
      align-items: stretch;
    }
    
    .quality-details {
      flex-direction: column;
      gap: 5px;
    }
    
    .settings-actions {
      flex-direction: column;
    }
  }
  
  /* Audio Monitor UI [PSF] - Dauerhaft aktive Pegelanzeige */
  .audio-monitor {
    margin-top: 1rem;
    padding: 1.5rem;
    background: var(--color-surface-secondary, #f8f9fa);
    border-radius: 12px;
    border: 2px solid var(--color-border, #e9ecef);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .monitor-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  
  .monitor-header h4 {
    margin: 0;
    font-size: 1.1rem;
    color: var(--color-text-primary, #212529);
  }
  
  .monitor-status {
    font-size: 0.9rem;
    font-weight: 600;
  }
  
  .status-active {
    color: #22c55e;
  }
  
  .status-inactive {
    color: #ef4444;
  }
  
  .audio-level-display {
    background: var(--color-surface, #ffffff);
    border-radius: 8px;
    padding: 1rem;
    border: 1px solid var(--color-border, #dee2e6);
  }
  
  .level-bar-container {
    margin-bottom: 1rem;
  }
  
  .level-bar {
    position: relative;
    width: 100%;
    height: 24px;
    background: linear-gradient(90deg, #e9ecef 0%, #e9ecef 100%);
    border-radius: 12px;
    overflow: hidden;
    border: 1px solid var(--color-border, #dee2e6);
  }
  
  .level-fill {
    height: 100%;
    background: linear-gradient(90deg, #22c55e 0%, #eab308 60%, #f59e0b 80%, #ef4444 95%);
    transition: width 0.15s ease-out;
    border-radius: 12px;
    min-width: 2px;
  }
  
  .level-fill.clipping {
    background: #ef4444;
    animation: pulse-clipping 0.4s infinite;
  }
  
  .peak-indicator {
    position: absolute;
    top: 2px;
    width: 3px;
    height: calc(100% - 4px);
    background: #ffffff;
    border-radius: 2px;
    box-shadow: 0 0 6px rgba(255, 255, 255, 0.9), 0 0 3px rgba(0, 0, 0, 0.3);
    transition: left 0.1s ease-out;
    z-index: 10;
  }
  
  .level-scale {
    display: flex;
    justify-content: space-between;
    margin-top: 0.5rem;
    font-size: 0.8rem;
    color: var(--color-text-secondary, #6c757d);
  }
  
  .level-values {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
    gap: 1rem;
    align-items: center;
  }
  
  .value-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
  }
  
  .value-label {
    font-size: 0.8rem;
    color: var(--color-text-secondary, #6c757d);
    margin-bottom: 0.25rem;
    font-weight: 500;
  }
  
  .value-number {
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-text-primary, #212529);
    font-family: 'Courier New', monospace;
  }
  
  .clipping-alert {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    background: #fef2f2;
    border: 1px solid #fecaca;
    border-radius: 6px;
    padding: 0.5rem;
    animation: pulse-alert 0.5s infinite;
  }
  
  .alert-icon {
    font-size: 1.2rem;
  }
  
  .alert-text {
    font-size: 0.9rem;
    font-weight: 600;
    color: #dc2626;
  }
  
  @keyframes pulse-clipping {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.6; }
  }
  
  @keyframes pulse-alert {
    0%, 100% { transform: scale(1); }
    50% { transform: scale(1.02); }
  }
  
  /* Mock vs Real Feature Styling [PSF] */
  .mock-feature {
    opacity: 0.6;
    position: relative;
  }
  
  .mock-feature input[type="checkbox"] {
    cursor: not-allowed;
  }
  
  .mock-feature span:not(.mock-badge):not(.real-badge) {
    color: var(--color-text-secondary, #6c757d);
    text-decoration: line-through;
  }
  
  .real-feature {
    position: relative;
  }
  
  .mock-badge {
    position: absolute;
    right: 0;
    top: 50%;
    transform: translateY(-50%);
    background: #fbbf24;
    color: #92400e;
    font-size: 0.7rem;
    font-weight: 600;
    padding: 2px 6px;
    border-radius: 4px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .real-badge {
    position: absolute;
    right: 0;
    top: 50%;
    transform: translateY(-50%);
    background: #10b981;
    color: #ffffff;
    font-size: 0.7rem;
    font-weight: 600;
    padding: 2px 6px;
    border-radius: 4px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .checkbox-label {
    position: relative;
    padding-right: 60px; /* Platz für Badge */
  }
  
  /* Whisper Provider Selection [WMM] */
  .provider-selection {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }
  

  
  .provider-dropdown {
    flex: 1;
    padding: 0.5rem;
    border: 1px solid var(--color-border, #dee2e6);
    border-radius: 6px;
    background: var(--color-surface, #ffffff);
    font-size: 0.9rem;
  }
  
  .provider-details {
    background: var(--color-surface-secondary, #f8f9fa);
    border-radius: 8px;
    padding: 1rem;
    border: 1px solid var(--color-border, #e9ecef);
  }
  
  .detail-item {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 0.75rem;
  }
  
  .detail-item:last-of-type {
    margin-bottom: 0;
  }
  
  .detail-label {
    min-width: 80px;
    font-size: 0.9rem;
    color: var(--color-text-secondary, #6c757d);
  }
  
  .detail-item select {
    flex: 1;
    padding: 0.4rem;
    border: 1px solid var(--color-border, #dee2e6);
    border-radius: 4px;
    background: var(--color-surface, #ffffff);
    font-size: 0.85rem;
  }
  
  .provider-info {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin-top: 0.75rem;
    padding: 0.5rem;
    background: rgba(59, 130, 246, 0.1);
    border-radius: 6px;
    border: 1px solid rgba(59, 130, 246, 0.2);
  }
  
  .info-icon {
    font-size: 1rem;
  }
  
  .provider-info span:not(.info-icon) {
    font-size: 0.85rem;
    color: var(--color-text-secondary, #374151);
    line-height: 1.4;
  }
</style>
