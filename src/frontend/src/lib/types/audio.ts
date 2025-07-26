// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

/**
 * Audio Types für MedEasy
 * [WMM] Whisper Multi-Model - Audio-Handling für Transkription
 * [PSF] Patient Safety First - Medizinisch relevante Audio-Qualität
 * [SF] Schweizer Formate - Deutsche Sprache
 * [ZTS] Zero Tolerance Security - Sichere Audio-Verarbeitung
 */

export interface AudioDevice {
  id: string;
  label: string;
  kind: 'audioinput' | 'audiooutput';
}

export type AudioQuality = 'low' | 'medium' | 'high' | 'ultra';

export interface AudioSettings {
  // Device Settings [ZTS]
  deviceId: string;
  quality: AudioQuality;
  sampleRate: number;
  bitDepth: number;
  channels: number;
  
  // Processing Settings [PSF]
  noiseReduction: boolean;
  echoCancellation: boolean;
  autoGainControl: boolean;
  noiseSuppression: boolean;
  
  // Medical Settings [PSF][SF]
  medicalMode: boolean;
  swissGermanOptimization: boolean;
  lowLatencyMode: boolean;
  
  // Level Settings [PSF]
  inputGain: number;
  outputGain: number;
  silenceThreshold: number; // dB
  maxRecordingLength: number; // seconds
}

export interface AudioLevel {
  // Current Audio Levels [PSF]
  volume: number; // 0-100
  peak: number; // 0-100
  rms: number; // Root Mean Square
  decibels: number; // dB level
  
  // Status [PSF]
  isActive: boolean;
  isSilent: boolean;
  isClipping: boolean;
  timestamp: number;
}

export interface AudioRecording {
  // Recording Metadata [ATV]
  id: string;
  sessionId: string;
  startTime: Date;
  endTime?: Date;
  duration: number; // seconds
  
  // Audio Properties [PSF]
  sampleRate: number;
  channels: number;
  bitDepth: number;
  fileSize: number; // bytes
  
  // Quality Metrics [PSF][WMM]
  averageLevel: number; // dB
  peakLevel: number; // dB
  silencePercentage: number; // 0-100
  qualityScore: number; // 0-100
  
  // Medical Context [SF][PSF]
  swissGermanDetected: boolean;
  medicalTermsCount: number;
  confidenceScore: number; // 0-100
  
  // File Information [ATV]
  filePath?: string;
  encrypted: boolean;
  anonymized: boolean;
}

export interface AudioAnalysis {
  // Real-time Analysis [WMM][PSF]
  transcriptionActive: boolean;
  currentText: string;
  confidence: number; // 0-100
  
  // Language Detection [SF]
  detectedLanguage: 'de' | 'de-CH' | 'en' | 'unknown';
  swissGermanConfidence: number; // 0-100
  
  // Audio Quality [PSF]
  signalToNoise: number; // dB
  speechPresent: boolean;
  backgroundNoise: number; // dB
  
  // Medical Analysis [PSF][MDL]
  medicalTermsDetected: string[];
  urgencyLevel: 'low' | 'medium' | 'high' | 'critical';
  keywordsCount: number;
  
  // Performance [WMM]
  processingLatency: number; // ms
  bufferHealth: number; // 0-100
  lastUpdate: Date;
}

export interface AudioPermissions {
  // Browser Permissions [ZTS]
  microphone: 'granted' | 'denied' | 'prompt';
  mediaDevices: boolean;
  
  // Security Context [ZTS][PSF]
  secureContext: boolean; // HTTPS required
  userActivation: boolean; // User gesture required
  
  // Error Handling [ZTS]
  lastError?: string;
  errorCount: number;
  lastErrorTime?: Date;
}

export interface AudioConstraints {
  // MediaStream Constraints [PSF][WMM]
  deviceId?: { exact: string } | { ideal: string };
  sampleRate?: number | { min?: number; max?: number; ideal?: number };
  channelCount?: number | { min?: number; max?: number; ideal?: number };
  
  // Audio Processing [PSF]
  echoCancellation?: boolean;
  noiseSuppression?: boolean;
  autoGainControl?: boolean;
  
  // Advanced Constraints [PSF]
  latency?: number; // seconds
  sampleSize?: number; // bits
  volume?: number; // 0.0 - 1.0
}

export interface AudioState {
  // Current State [PSF]
  isRecording: boolean;
  isPaused: boolean;
  isAnalyzing: boolean;
  
  // Device State [ZTS]
  currentDevice: AudioDevice | null;
  availableDevices: AudioDevice[];
  permissions: AudioPermissions;
  
  // Audio Data [PSF]
  currentLevel: AudioLevel;
  currentRecording: AudioRecording | null;
  currentAnalysis: AudioAnalysis | null;
  
  // Settings [ZTS]
  settings: AudioSettings;
  
  // Error State [ZTS]
  error: string | null;
  lastError: Date | null;
}

export interface AudioEvent {
  // Event Types [ATV]
  type: 'start' | 'stop' | 'pause' | 'resume' | 'error' | 'level' | 'analysis' |
        'settings_updated' | 'devices_enumerated' | 'permissions_granted' |
        'recording_started' | 'recording_stopped' | 'recording_paused' | 'recording_resumed' |
        'audio_clipping' | 'device_changed' | 'quality_changed';
  timestamp: Date;
  
  // Event Data [ATV]
  data?: any;
  error?: string;
  
  // Context [PSF]
  sessionId?: string;
  userId?: string;
  deviceId?: string;
}

// Audio Quality Presets [PSF][WMM]
export const AUDIO_QUALITY_PRESETS: Record<AudioQuality, Partial<AudioSettings>> = {
  low: {
    sampleRate: 16000,
    bitDepth: 16,
    channels: 1,
    noiseReduction: true,
    echoCancellation: true,
    autoGainControl: true,
    noiseSuppression: true
  },
  medium: {
    sampleRate: 22050,
    bitDepth: 16,
    channels: 1,
    noiseReduction: true,
    echoCancellation: true,
    autoGainControl: true,
    noiseSuppression: true
  },
  high: {
    sampleRate: 44100,
    bitDepth: 16,
    channels: 1,
    noiseReduction: true,
    echoCancellation: true,
    autoGainControl: true,
    noiseSuppression: true
  },
  ultra: {
    sampleRate: 48000,
    bitDepth: 24,
    channels: 1,
    noiseReduction: true,
    echoCancellation: true,
    autoGainControl: true,
    noiseSuppression: true
  }
};

// Medical Audio Constants [PSF][SF]
export const MEDICAL_AUDIO_CONSTANTS = {
  // Silence Detection [PSF]
  SILENCE_THRESHOLD_DB: -50,
  SILENCE_DURATION_MS: 2000,
  
  // Quality Thresholds [PSF]
  MIN_SIGNAL_TO_NOISE_DB: 20,
  MIN_CONFIDENCE_SCORE: 70,
  MAX_BACKGROUND_NOISE_DB: -30,
  
  // Recording Limits [PSF]
  MAX_RECORDING_DURATION_SECONDS: 3600, // 1 hour
  MIN_RECORDING_DURATION_SECONDS: 1,
  MAX_FILE_SIZE_MB: 500,
  
  // Swiss German Detection [SF]
  SWISS_GERMAN_CONFIDENCE_THRESHOLD: 60,
  MEDICAL_TERMS_MIN_COUNT: 3,
  
  // Performance [WMM]
  MAX_PROCESSING_LATENCY_MS: 500,
  BUFFER_SIZE_SAMPLES: 4096,
  UPDATE_INTERVAL_MS: 100
} as const;

// Error Types [ZTS]
export type AudioErrorType = 
  | 'permission_denied'
  | 'device_not_found'
  | 'device_busy'
  | 'constraint_not_satisfied'
  | 'not_supported'
  | 'network_error'
  | 'security_error'
  | 'unknown_error';

export interface AudioError {
  type: AudioErrorType;
  message: string;
  code?: string;
  timestamp: Date;
  deviceId?: string;
  constraints?: AudioConstraints;
}
