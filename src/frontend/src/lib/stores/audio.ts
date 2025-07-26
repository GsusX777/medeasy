// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

/**
 * Audio Store für MedEasy
 * [WMM] Whisper Multi-Model - Audio-Handling für Transkription
 * [PSF] Patient Safety First - Medizinisch relevante Audio-Qualität
 * [SF] Schweizer Formate - Deutsche Sprache
 * [ZTS] Zero Tolerance Security - Sichere Audio-Verarbeitung
 * [ATV] Audit-Trail Vollständig - Alle Audio-Events protokollieren
 */

import { writable, derived, get } from 'svelte/store';
import type { 
  AudioState, 
  AudioSettings, 
  AudioDevice, 
  AudioLevel, 
  AudioRecording, 
  AudioAnalysis,
  AudioPermissions,
  AudioEvent,
  AudioError,
  AudioErrorType,
  AudioConstraints
} from '$lib/types/audio';
import { AUDIO_QUALITY_PRESETS, MEDICAL_AUDIO_CONSTANTS } from '$lib/types/audio';

// Default Audio Settings [PSF][SF]
const DEFAULT_AUDIO_SETTINGS: AudioSettings = {
  deviceId: '',
  quality: 'high',
  sampleRate: 44100,
  bitDepth: 16,
  channels: 1,
  noiseReduction: true,
  echoCancellation: true,
  autoGainControl: true,
  noiseSuppression: true,
  medicalMode: true, // Always enabled [PSF]
  swissGermanOptimization: true, // [SF]
  lowLatencyMode: false,
  inputGain: 1.0,
  outputGain: 1.0,
  silenceThreshold: MEDICAL_AUDIO_CONSTANTS.SILENCE_THRESHOLD_DB,
  maxRecordingLength: MEDICAL_AUDIO_CONSTANTS.MAX_RECORDING_DURATION_SECONDS
};

// Initial Audio State [ZTS]
const INITIAL_AUDIO_STATE: AudioState = {
  isRecording: false,
  isPaused: false,
  isAnalyzing: false,
  currentDevice: null,
  availableDevices: [],
  permissions: {
    microphone: 'prompt',
    mediaDevices: false,
    secureContext: false,
    userActivation: false,
    errorCount: 0
  },
  currentLevel: {
    volume: 0,
    peak: 0,
    rms: 0,
    decibels: -Infinity,
    isActive: false,
    isSilent: true,
    isClipping: false,
    timestamp: Date.now()
  },
  currentRecording: null,
  currentAnalysis: null,
  settings: DEFAULT_AUDIO_SETTINGS,
  error: null,
  lastError: null
};

// Audio State Store [ZTS]
export const audioState = writable<AudioState>(INITIAL_AUDIO_STATE);

// Audio Events Store for Audit Trail [ATV]
export const audioEvents = writable<AudioEvent[]>([]);

// Internal State
let mediaStream: MediaStream | null = null;
let audioContext: AudioContext | null = null;
let analyserNode: AnalyserNode | null = null;
let microphoneNode: MediaStreamAudioSourceNode | null = null;
let levelUpdateInterval: number | null = null;
let recordingStartTime: Date | null = null;

// Audio Store Functions
class AudioStore {
  
  // Get current settings [ZTS]
  getSettings(): AudioSettings {
    return get(audioState).settings;
  }
  
  // Update audio settings [ZTS][ATV]
  async updateSettings(newSettings: Partial<AudioSettings>): Promise<void> {
    try {
      const currentState = get(audioState);
      const updatedSettings = { ...currentState.settings, ...newSettings };
      
      // Apply quality preset if quality changed [PSF]
      if (newSettings.quality && newSettings.quality !== currentState.settings.quality) {
        const preset = AUDIO_QUALITY_PRESETS[newSettings.quality];
        Object.assign(updatedSettings, preset);
      }
      
      // Ensure medical mode is always enabled [PSF]
      updatedSettings.medicalMode = true;
      
      // Update state
      audioState.update(state => ({
        ...state,
        settings: updatedSettings
      }));
      
      // Save to localStorage [ZTS]
      localStorage.setItem('medeasy_audio_settings', JSON.stringify(updatedSettings));
      
      // Log event [ATV]
      this.logEvent('settings_updated', { settings: updatedSettings });
      
      // Restart audio if currently active
      if (currentState.isRecording) {
        await this.restartAudio();
      }
      
    } catch (error) {
      console.error('Fehler beim Aktualisieren der Audio-Einstellungen:', error);
      this.handleError('unknown_error', error instanceof Error ? error.message : 'Unbekannter Fehler');
    }
  }
  
  // Load settings from localStorage [ZTS]
  loadSettings(): void {
    try {
      const saved = localStorage.getItem('medeasy_audio_settings');
      if (saved) {
        const settings = JSON.parse(saved) as AudioSettings;
        // Ensure medical mode is always enabled [PSF]
        settings.medicalMode = true;
        
        audioState.update(state => ({
          ...state,
          settings: { ...DEFAULT_AUDIO_SETTINGS, ...settings }
        }));
      }
    } catch (error) {
      console.error('Fehler beim Laden der Audio-Einstellungen:', error);
      // Use defaults on error
      audioState.update(state => ({
        ...state,
        settings: DEFAULT_AUDIO_SETTINGS
      }));
    }
  }
  
  // Enumerate audio devices [ZTS]
  async enumerateDevices(): Promise<AudioDevice[]> {
    try {
      // Check for secure context [ZTS]
      if (!window.isSecureContext) {
        throw new Error('Sichere Verbindung (HTTPS) erforderlich für Mikrofon-Zugriff');
      }
      
      // Check for MediaDevices API support [ZTS]
      if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
        throw new Error('MediaDevices API nicht unterstützt');
      }
      
      // Request permission first [ZTS]
      await this.requestPermissions();
      
      // Enumerate devices
      const devices = await navigator.mediaDevices.enumerateDevices();
      const audioDevices: AudioDevice[] = devices
        .filter(device => device.kind === 'audioinput')
        .map(device => ({
          id: device.deviceId,
          label: device.label || `Mikrofon ${device.deviceId.slice(0, 8)}`,
          kind: device.kind as 'audioinput'
        }));
      
      // Update state
      audioState.update(state => ({
        ...state,
        availableDevices: audioDevices,
        permissions: {
          ...state.permissions,
          mediaDevices: true
        }
      }));
      
      // Log event [ATV]
      this.logEvent('devices_enumerated', { deviceCount: audioDevices.length });
      
      return audioDevices;
      
    } catch (error) {
      console.error('Fehler beim Enumerieren der Audio-Geräte:', error);
      this.handleError('device_not_found', error instanceof Error ? error.message : 'Unbekannter Fehler');
      return [];
    }
  }
  
  // Request microphone permissions [ZTS]
  async requestPermissions(): Promise<boolean> {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ 
        audio: {
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true
        } 
      });
      
      // Clean up test stream
      stream.getTracks().forEach(track => track.stop());
      
      // Update permissions
      audioState.update(state => ({
        ...state,
        permissions: {
          ...state.permissions,
          microphone: 'granted',
          mediaDevices: true,
          secureContext: window.isSecureContext,
          userActivation: true
        }
      }));
      
      // Log event [ATV]
      this.logEvent('permissions_granted');
      
      return true;
      
    } catch (error) {
      console.error('Mikrofon-Berechtigung verweigert:', error);
      
      // Update permissions
      audioState.update(state => ({
        ...state,
        permissions: {
          ...state.permissions,
          microphone: 'denied',
          errorCount: state.permissions.errorCount + 1,
          lastError: error instanceof Error ? error.message : 'Unbekannter Fehler',
          lastErrorTime: new Date()
        }
      }));
      
      this.handleError('permission_denied', error instanceof Error ? error.message : 'Berechtigung verweigert');
      return false;
    }
  }
  
  // Start audio recording [PSF][ATV]
  async startRecording(sessionId?: string): Promise<boolean> {
    try {
      const currentState = get(audioState);
      
      if (currentState.isRecording) {
        console.warn('Aufnahme bereits aktiv');
        return true;
      }
      
      // Request permissions if needed
      if (currentState.permissions.microphone !== 'granted') {
        const granted = await this.requestPermissions();
        if (!granted) return false;
      }
      
      // Get audio constraints [PSF]
      const constraints = this.buildAudioConstraints(currentState.settings);
      
      // Get media stream
      mediaStream = await navigator.mediaDevices.getUserMedia({ audio: constraints });
      
      // Set up audio analysis [PSF]
      await this.setupAudioAnalysis(mediaStream);
      
      // Update state
      recordingStartTime = new Date();
      audioState.update(state => ({
        ...state,
        isRecording: true,
        isPaused: false,
        isAnalyzing: true,
        currentDevice: state.availableDevices.find(d => d.id === state.settings.deviceId) || null,
        currentRecording: {
          id: crypto.randomUUID(),
          sessionId: sessionId || crypto.randomUUID(),
          startTime: recordingStartTime!,
          duration: 0,
          sampleRate: state.settings.sampleRate,
          channels: state.settings.channels,
          bitDepth: state.settings.bitDepth,
          fileSize: 0,
          averageLevel: 0,
          peakLevel: 0,
          silencePercentage: 0,
          qualityScore: 0,
          swissGermanDetected: false,
          medicalTermsCount: 0,
          confidenceScore: 0,
          encrypted: true, // Always encrypted [SP]
          anonymized: true // Always anonymized [AIU]
        },
        error: null
      }));
      
      // Start level monitoring
      this.startLevelMonitoring();
      
      // Log event [ATV]
      this.logEvent('recording_started', { sessionId, deviceId: currentState.settings.deviceId });
      
      return true;
      
    } catch (error) {
      console.error('Fehler beim Starten der Aufnahme:', error);
      this.handleError('constraint_not_satisfied', error instanceof Error ? error.message : 'Aufnahme fehlgeschlagen');
      return false;
    }
  }
  
  // Stop audio recording [PSF][ATV]
  async stopRecording(): Promise<AudioRecording | null> {
    try {
      const currentState = get(audioState);
      
      if (!currentState.isRecording) {
        console.warn('Keine aktive Aufnahme');
        return null;
      }
      
      // Stop media stream
      if (mediaStream) {
        mediaStream.getTracks().forEach(track => track.stop());
        mediaStream = null;
      }
      
      // Clean up audio context
      this.cleanupAudioAnalysis();
      
      // Stop level monitoring
      this.stopLevelMonitoring();
      
      // Calculate final recording data
      const recording = currentState.currentRecording;
      if (recording && recordingStartTime) {
        recording.endTime = new Date();
        recording.duration = (recording.endTime.getTime() - recordingStartTime.getTime()) / 1000;
      }
      
      // Update state
      audioState.update(state => ({
        ...state,
        isRecording: false,
        isPaused: false,
        isAnalyzing: false,
        currentRecording: null,
        currentAnalysis: null
      }));
      
      // Log event [ATV]
      this.logEvent('recording_stopped', { 
        duration: recording?.duration,
        sessionId: recording?.sessionId 
      });
      
      recordingStartTime = null;
      return recording;
      
    } catch (error) {
      console.error('Fehler beim Stoppen der Aufnahme:', error);
      this.handleError('unknown_error', error instanceof Error ? error.message : 'Stoppen fehlgeschlagen');
      return null;
    }
  }
  
  // Pause/Resume recording [PSF]
  async pauseRecording(): Promise<void> {
    const currentState = get(audioState);
    if (!currentState.isRecording) return;
    
    audioState.update(state => ({
      ...state,
      isPaused: !state.isPaused
    }));
    
    this.logEvent(currentState.isPaused ? 'recording_resumed' : 'recording_paused');
  }
  
  // Build audio constraints from settings [PSF]
  private buildAudioConstraints(settings: AudioSettings): AudioConstraints {
    const constraints: AudioConstraints = {
      echoCancellation: settings.echoCancellation,
      noiseSuppression: settings.noiseSuppression,
      autoGainControl: settings.autoGainControl,
      sampleRate: settings.sampleRate,
      channelCount: settings.channels
    };
    
    if (settings.deviceId) {
      constraints.deviceId = { exact: settings.deviceId };
    }
    
    if (settings.lowLatencyMode) {
      constraints.latency = 0.01; // 10ms
    }
    
    return constraints;
  }
  
  // Setup audio analysis [PSF][WMM]
  private async setupAudioAnalysis(stream: MediaStream): Promise<void> {
    try {
      // Create audio context
      audioContext = new AudioContext({
        sampleRate: get(audioState).settings.sampleRate
      });
      
      // Create analyser node
      analyserNode = audioContext.createAnalyser();
      analyserNode.fftSize = MEDICAL_AUDIO_CONSTANTS.BUFFER_SIZE_SAMPLES;
      analyserNode.smoothingTimeConstant = 0.8;
      
      // Connect microphone
      microphoneNode = audioContext.createMediaStreamSource(stream);
      microphoneNode.connect(analyserNode);
      
    } catch (error) {
      console.error('Fehler beim Setup der Audio-Analyse:', error);
      throw error;
    }
  }
  
  // Cleanup audio analysis
  private cleanupAudioAnalysis(): void {
    if (microphoneNode) {
      microphoneNode.disconnect();
      microphoneNode = null;
    }
    
    if (audioContext) {
      audioContext.close();
      audioContext = null;
    }
    
    analyserNode = null;
  }
  
  // Start level monitoring [PSF]
  private startLevelMonitoring(): void {
    if (levelUpdateInterval) return;
    
    levelUpdateInterval = window.setInterval(() => {
      this.updateAudioLevel();
    }, MEDICAL_AUDIO_CONSTANTS.UPDATE_INTERVAL_MS);
  }
  
  // Stop level monitoring
  private stopLevelMonitoring(): void {
    if (levelUpdateInterval) {
      clearInterval(levelUpdateInterval);
      levelUpdateInterval = null;
    }
  }
  
  // Update audio level [PSF] - Überladung für externe Daten
  updateAudioLevel(level?: AudioLevel): void {
    // Wenn externes Level-Objekt übergeben wird, direkt verwenden
    if (level) {
      audioState.update(state => ({
        ...state,
        currentLevel: {
          ...level,
          timestamp: Date.now()
        }
      }));
      
      // Log clipping events [ATV]
      if (level.isClipping) {
        this.logEvent('audio_clipping', { level: level.peak });
      }
      return;
    }
    
    // Interne Analyse-Logik
    if (!analyserNode) return;
    
    const bufferLength = analyserNode.frequencyBinCount;
    const dataArray = new Uint8Array(bufferLength);
    analyserNode.getByteFrequencyData(dataArray);
    
    // Calculate RMS and peak
    let sum = 0;
    let peak = 0;
    
    for (let i = 0; i < bufferLength; i++) {
      const value = dataArray[i] / 255.0;
      sum += value * value;
      peak = Math.max(peak, value);
    }
    
    const rms = Math.sqrt(sum / bufferLength);
    const volume = Math.round(peak * 100);
    const decibels = 20 * Math.log10(rms || 0.001);
    
    const currentState = get(audioState);
    const silenceThreshold = currentState.settings.silenceThreshold;
    
    const audioLevel: AudioLevel = {
      volume,
      peak: Math.round(peak * 100),
      rms,
      decibels,
      isActive: volume > 1,
      isSilent: decibels < silenceThreshold,
      isClipping: peak > 0.95,
      timestamp: Date.now()
    };
    
    // Update state
    audioState.update(state => ({
      ...state,
      currentLevel: audioLevel
    }));
    
    // Log clipping events [ATV]
    if (audioLevel.isClipping) {
      this.logEvent('audio_clipping', { level: audioLevel.peak });
    }
  }
  

  
  // Restart audio with new settings [PSF]
  private async restartAudio(): Promise<void> {
    const wasRecording = get(audioState).isRecording;
    if (wasRecording) {
      await this.stopRecording();
      await this.startRecording();
    }
  }
  
  // Handle errors [ZTS]
  private handleError(type: AudioErrorType, message: string): void {
    const error: AudioError = {
      type,
      message,
      timestamp: new Date()
    };
    
    audioState.update(state => ({
      ...state,
      error: message,
      lastError: new Date(),
      permissions: {
        ...state.permissions,
        errorCount: state.permissions.errorCount + 1,
        lastError: message,
        lastErrorTime: new Date()
      }
    }));
    
    // Log error event [ATV]
    this.logEvent('error', { error });
  }
  
  // Log audio events [ATV]
  private logEvent(type: AudioEvent['type'], data?: any): void {
    const event: AudioEvent = {
      type,
      timestamp: new Date(),
      data,
      sessionId: get(audioState).currentRecording?.sessionId
    };
    
    audioEvents.update(events => [...events, event]);
    
    // Keep only last 1000 events to prevent memory issues
    audioEvents.update(events => events.slice(-1000));
  }
}

// Export audio store instance
export const audioStore = new AudioStore();

// Derived stores for convenience [PSF]
export const isRecording = derived(audioState, $state => $state.isRecording);
export const currentLevel = derived(audioState, $state => $state.currentLevel);
export const audioError = derived(audioState, $state => $state.error);
export const availableDevices = derived(audioState, $state => $state.availableDevices);
export const audioSettings = derived(audioState, $state => $state.settings);

// Initialize audio store [ZTS]
if (typeof window !== 'undefined') {
  audioStore.loadSettings();
  
  // Auto-enumerate devices on load
  audioStore.enumerateDevices().catch(console.error);
  
  // Listen for device changes
  if (navigator.mediaDevices) {
    navigator.mediaDevices.addEventListener('devicechange', () => {
      audioStore.enumerateDevices().catch(console.error);
    });
  }
}
