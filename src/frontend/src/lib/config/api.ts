// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// [ZTS] Zero tolerance security - centralized API configuration
// [PSF] Patient safety first - consistent API endpoints
// [ATV] Audit trail - all API calls must be traceable

/**
 * Centralized API configuration for MedEasy Frontend
 * Ensures consistent backend communication across all components
 */

// Backend API Configuration
export const API_CONFIG = {
	// Base URLs
	BASE_URL: 'http://localhost:5155',
	AI_BASE_URL: 'http://localhost:5155/api/ai',
	ADMIN_BASE_URL: 'http://localhost:5155/api/admin',
	SYSTEM_BASE_URL: 'http://localhost:5155/api/system',
	
	// Timeout settings
	DEFAULT_TIMEOUT: 30000, // 30 seconds
	UPLOAD_TIMEOUT: 120000, // 2 minutes for file uploads
	BENCHMARK_TIMEOUT: 300000, // 5 minutes for benchmarks
	
	// Retry settings
	MAX_RETRIES: 3,
	RETRY_DELAY: 1000, // 1 second
} as const;

// AI Service Endpoints
export const AI_ENDPOINTS = {
	// Whisper endpoints
	WHISPER_TRANSCRIBE: `${API_CONFIG.AI_BASE_URL}/whisper/transcribe-file`,
	WHISPER_BENCHMARK: `${API_CONFIG.AI_BASE_URL}/whisper/benchmark-models-file`,
	WHISPER_BENCHMARK_CHUNK: `${API_CONFIG.AI_BASE_URL}/benchmark-chunk-test`,
	WHISPER_AVAILABLE_MODELS: `${API_CONFIG.AI_BASE_URL}/whisper/available-models`,
	WHISPER_HARDWARE_INFO: `${API_CONFIG.AI_BASE_URL}/whisper/hardware-info`,
	
	// Health check
	HEALTH: `${API_CONFIG.BASE_URL}/health`,
} as const;

// System Monitoring Endpoints [PSF][ZTS]
export const SYSTEM_ENDPOINTS = {
	// Performance monitoring (SystemController)
	PERFORMANCE: `${API_CONFIG.SYSTEM_BASE_URL}/performance`,
	HEALTH: `${API_CONFIG.SYSTEM_BASE_URL}/health`,
	STATS: `${API_CONFIG.SYSTEM_BASE_URL}/stats`,
} as const;

// Admin Panel Endpoints [ATV]
export const ADMIN_ENDPOINTS = {
	// System logs
	LOGS: `${API_CONFIG.ADMIN_BASE_URL}/logs`,
	LOGS_EXPORT: `${API_CONFIG.ADMIN_BASE_URL}/logs/export`,
	
	// Benchmark history
	BENCHMARK_HISTORY: `${API_CONFIG.ADMIN_BASE_URL}/benchmarks/history`,
	BENCHMARK_EXPORT: `${API_CONFIG.ADMIN_BASE_URL}/benchmarks/export`,
	
	// Audit trail
	AUDIT_TRAIL: `${API_CONFIG.ADMIN_BASE_URL}/audit`,
} as const;

// HTTP Client Configuration
export const HTTP_CONFIG = {
	headers: {
		'Content-Type': 'application/json',
		'Accept': 'application/json',
		'X-Client': 'MedEasy-Frontend',
		'X-Version': '1.0.0',
	},
	
	// CORS settings for development
	credentials: 'same-origin' as RequestCredentials,
} as const;

/**
 * Creates a configured fetch request with default settings
 * [ATV] All API calls are logged for audit purposes
 */
export async function apiRequest(
	url: string, 
	options: RequestInit = {},
	timeout: number = API_CONFIG.DEFAULT_TIMEOUT
): Promise<Response> {
	const controller = new AbortController();
	const timeoutId = setTimeout(() => controller.abort(), timeout);
	
	try {
		// [ATV] Log API request for audit trail
		console.log('API Request:', {
			url,
			method: options.method || 'GET',
			timestamp: new Date().toISOString(),
			headers: options.headers
		});
		
		// Detect FormData to avoid overriding Content-Type
		const isFormData = options.body instanceof FormData;
		
		const response = await fetch(url, {
			...HTTP_CONFIG,
			...options,
			signal: controller.signal,
			headers: {
				// Don't include Content-Type for FormData - let browser set multipart boundary
				...(isFormData ? 
					{ 'Accept': HTTP_CONFIG.headers.Accept, 'X-Client': HTTP_CONFIG.headers['X-Client'], 'X-Version': HTTP_CONFIG.headers['X-Version'] } : 
					HTTP_CONFIG.headers
				),
				...options.headers,
			},
		});
		
		clearTimeout(timeoutId);
		
		// [ATV] Log API response for audit trail
		console.log('API Response:', {
			url,
			status: response.status,
			statusText: response.statusText,
			timestamp: new Date().toISOString()
		});
		
		return response;
	} catch (error) {
		clearTimeout(timeoutId);
		
		// [ATV] Log API error for audit trail
		console.error('API Error:', {
			url,
			error: error instanceof Error ? error.message : 'Unknown error',
			timestamp: new Date().toISOString()
		});
		
		throw error;
	}
}

/**
 * Upload file with progress tracking
 * [PSF] Patient safety - secure file upload with validation
 */
export async function uploadFile(
	url: string,
	file: File,
	additionalData: Record<string, string> = {},
	onProgress?: (progress: number) => void
): Promise<Response> {
	const formData = new FormData();
	// Explicitly set filename and content-type for ASP.NET Core IFormFile binding
	formData.append('audioFile', file, file.name);
	
	// Add additional form data
	Object.entries(additionalData).forEach(([key, value]) => {
		formData.append(key, value);
	});
	
	// [DEBUG] Log FormData contents for debugging
	console.log('🔧 DEBUG: FormData contents:');
	for (let [key, value] of formData.entries()) {
		if (value instanceof File) {
			console.log(`  ${key}: File(${value.name}, ${value.size} bytes, ${value.type})`);
		} else {
			console.log(`  ${key}: ${value}`);
		}
	}
	
	// [ATV] Log file upload for audit trail
	console.log('File Upload:', {
		url,
		fileName: file.name,
		fileSize: file.size,
		fileType: file.type,
		additionalData,
		timestamp: new Date().toISOString()
	});
	
	// Use longer timeout for benchmark operations
	const timeout = url.includes('benchmark') ? API_CONFIG.BENCHMARK_TIMEOUT : API_CONFIG.UPLOAD_TIMEOUT;
	
	return apiRequest(url, {
		method: 'POST',
		body: formData,
		headers: {
			// Remove Content-Type to let browser set boundary for FormData
			'X-Client': 'MedEasy-Frontend',
			'X-Version': '1.0.0',
		}
	}, timeout);
}

/**
 * Environment-specific configuration
 * [ZTS] Zero tolerance security - environment validation
 */
export function getEnvironmentConfig() {
	const isDevelopment = import.meta.env.DEV;
	const isProduction = import.meta.env.PROD;
	
	return {
		isDevelopment,
		isProduction,
		apiUrl: isDevelopment ? API_CONFIG.BASE_URL : API_CONFIG.BASE_URL, // TODO: Add production URL
		enableLogging: isDevelopment,
		enableDebug: isDevelopment,
	};
}

/**
 * Validate API response
 * [PSF] Patient safety - validate all API responses
 */
export async function validateApiResponse<T>(response: Response): Promise<T> {
	if (!response.ok) {
		const errorText = await response.text();
		throw new Error(`API Error ${response.status}: ${errorText}`);
	}
	
	const contentType = response.headers.get('content-type');
	if (contentType && contentType.includes('application/json')) {
		return response.json();
	} else {
		throw new Error('Invalid response content type');
	}
}

// Export types for TypeScript
export type ApiEndpoint = keyof typeof AI_ENDPOINTS | keyof typeof ADMIN_ENDPOINTS;
export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
