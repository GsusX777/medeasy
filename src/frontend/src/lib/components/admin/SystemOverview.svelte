<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	export let systemStats: {
		totalLogs: number;
		activeSessions: number;
		lastBenchmark: Date | null;
		systemHealth: string;
	};
	
	// [ATV] System overview with audit trail integration
	// [PSF] Patient safety monitoring
	// [DSC] Swiss compliance status display
	
	$: healthColor = getHealthColor(systemStats.systemHealth);
	$: healthIcon = getHealthIcon(systemStats.systemHealth);
	
	function getHealthColor(health: string): string {
		switch (health) {
			case 'healthy': return '#10b981';
			case 'warning': return '#f59e0b';
			case 'error': return '#ef4444';
			default: return '#6b7280';
		}
	}
	
	function getHealthIcon(health: string): string {
		switch (health) {
			case 'healthy': return '✅';
			case 'warning': return '⚠️';
			case 'error': return '❌';
			default: return '❓';
		}
	}
	
	function formatLastBenchmark(date: Date | null): string {
		if (!date) return 'Nie';
		const now = new Date();
		const diff = now.getTime() - date.getTime();
		const hours = Math.floor(diff / (1000 * 60 * 60));
		const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
		
		if (hours > 0) {
			return `vor ${hours}h ${minutes}m`;
		} else {
			return `vor ${minutes}m`;
		}
	}
</script>

<div class="system-overview">
	<h2>📊 System-Übersicht</h2>
	
	<div class="stats-grid">
		<div class="stat-card health">
			<div class="stat-header">
				<span class="stat-icon">{healthIcon}</span>
				<h3>System-Status</h3>
			</div>
			<div class="stat-value" style="color: {healthColor}">
				{systemStats.systemHealth.toUpperCase()}
			</div>
			<p class="stat-description">
				Alle Services operational
			</p>
		</div>
		
		<div class="stat-card logs">
			<div class="stat-header">
				<span class="stat-icon">📋</span>
				<h3>Gesamt-Logs</h3>
			</div>
			<div class="stat-value">
				{systemStats.totalLogs.toLocaleString('de-CH')}
			</div>
			<p class="stat-description">
				Einträge heute
			</p>
		</div>
		
		<div class="stat-card sessions">
			<div class="stat-header">
				<span class="stat-icon">👥</span>
				<h3>Aktive Sessions</h3>
			</div>
			<div class="stat-value">
				{systemStats.activeSessions}
			</div>
			<p class="stat-description">
				Laufende Konsultationen
			</p>
		</div>
		
		<div class="stat-card benchmark">
			<div class="stat-header">
				<span class="stat-icon">🧪</span>
				<h3>Letzter Benchmark</h3>
			</div>
			<div class="stat-value">
				{formatLastBenchmark(systemStats.lastBenchmark)}
			</div>
			<p class="stat-description">
				Whisper-Modelle getestet
			</p>
		</div>
	</div>
	
	<div class="services-status">
		<h3>🔧 Service-Status</h3>
		<div class="services-grid">
			<div class="service-item">
				<div class="service-indicator healthy"></div>
				<div class="service-info">
					<span class="service-name">.NET Backend</span>
					<span class="service-details">Port 5155 • API v1.0</span>
				</div>
			</div>
			
			<div class="service-item">
				<div class="service-indicator healthy"></div>
				<div class="service-info">
					<span class="service-name">Python AI Service</span>
					<span class="service-details">Port 50051 • gRPC</span>
				</div>
			</div>
			
			<div class="service-item">
				<div class="service-indicator healthy"></div>
				<div class="service-info">
					<span class="service-name">Svelte Frontend</span>
					<span class="service-details">Tauri App • Local</span>
				</div>
			</div>
			
			<div class="service-item">
				<div class="service-indicator healthy"></div>
				<div class="service-info">
					<span class="service-name">SQLCipher DB</span>
					<span class="service-details">Encrypted • Local</span>
				</div>
			</div>
		</div>
	</div>
</div>

<style>
	.system-overview {
		background: white;
		border-radius: 12px;
		padding: 2rem;
		margin-bottom: 2rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.system-overview h2 {
		margin-bottom: 1.5rem;
		color: var(--text-primary, #111827);
	}
	
	.stats-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 1rem;
		margin-bottom: 2rem;
	}
	
	.stat-card {
		background: var(--bg-secondary, #f8fafc);
		border-radius: 8px;
		padding: 1.5rem;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.stat-header {
		display: flex;
		align-items: center;
		margin-bottom: 1rem;
	}
	
	.stat-icon {
		font-size: 1.25rem;
		margin-right: 0.5rem;
	}
	
	.stat-header h3 {
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
		margin: 0;
		font-weight: 500;
	}
	
	.stat-value {
		font-size: 2rem;
		font-weight: 700;
		color: var(--text-primary, #111827);
		margin-bottom: 0.5rem;
	}
	
	.stat-description {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		margin: 0;
	}
	
	.services-status h3 {
		margin-bottom: 1rem;
		color: var(--text-primary, #111827);
	}
	
	.services-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: 1rem;
	}
	
	.service-item {
		display: flex;
		align-items: center;
		padding: 1rem;
		background: var(--bg-secondary, #f8fafc);
		border-radius: 8px;
		border: 1px solid var(--border-color, #e5e7eb);
	}
	
	.service-indicator {
		width: 12px;
		height: 12px;
		border-radius: 50%;
		margin-right: 0.75rem;
		flex-shrink: 0;
	}
	
	.service-indicator.healthy {
		background: #10b981;
	}
	
	.service-indicator.warning {
		background: #f59e0b;
	}
	
	.service-indicator.error {
		background: #ef4444;
	}
	
	.service-info {
		display: flex;
		flex-direction: column;
	}
	
	.service-name {
		font-weight: 500;
		color: var(--text-primary, #111827);
		margin-bottom: 0.125rem;
	}
	
	.service-details {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
</style>
