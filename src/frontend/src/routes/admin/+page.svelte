<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import AdminLayout from '$lib/components/admin/AdminLayout.svelte';
	import SystemOverview from '$lib/components/admin/SystemOverview.svelte';
	
	// [ATV] Admin access requires audit logging
	// [ZTS] Zero tolerance security - admin access must be logged
	// [PSF] Patient safety first - admin actions affect system reliability
	
	let systemStats: {
		totalLogs: number;
		activeSessions: number;
		lastBenchmark: Date | null;
		systemHealth: string;
	} = {
		totalLogs: 0,
		activeSessions: 0,
		lastBenchmark: null,
		systemHealth: 'unknown'
	};
	
	onMount(async () => {
		// Load system overview data
		await loadSystemStats();
		
		// [ATV] Log admin panel access
		console.log('Admin panel accessed', {
			timestamp: new Date().toISOString(),
			user: 'admin', // TODO: Get from auth context
			action: 'admin_panel_view'
		});
	});
	
	async function loadSystemStats() {
		try {
			// TODO: Implement API calls for system statistics
			systemStats = {
				totalLogs: 1247,
				activeSessions: 3,
				lastBenchmark: new Date(Date.now() - 3600000), // 1 hour ago
				systemHealth: 'healthy'
			};
		} catch (error) {
			console.error('Failed to load system stats:', error);
			systemStats.systemHealth = 'error';
		}
	}
</script>

<AdminLayout>
	<div class="admin-dashboard">
		<header class="dashboard-header">
			<h1>🔧 MedEasy Administration</h1>
			<p class="subtitle">System-Überwachung und Benchmark-Tests</p>
		</header>
		
		<SystemOverview {systemStats} />
		
		<div class="quick-actions">
			<div class="action-grid">
				<button 
					class="action-card logs"
					on:click={() => goto('/admin/logs')}
				>
					<div class="icon">📋</div>
					<h3>System Logs</h3>
					<p>Alle Logs aus Backend, AI-Service und Frontend</p>
					<span class="badge">{systemStats.totalLogs} Einträge</span>
				</button>
				
				<button 
					class="action-card benchmarks"
					on:click={() => goto('/admin/benchmarks')}
				>
					<div class="icon">🧪</div>
					<h3>Benchmark Tests</h3>
					<p>Whisper-Modelle testen und vergleichen</p>
					<span class="badge">4 Modelle</span>
				</button>
				
				<button 
					class="action-card performance"
					on:click={() => goto('/admin/performance')}
				>
					<div class="icon">📊</div>
					<h3>Performance Monitor</h3>
					<p>System-Performance und Ressourcen-Nutzung</p>
					<span class="badge">{systemStats.systemHealth}</span>
				</button>
				
				<button 
					class="action-card audit"
					on:click={() => goto('/admin/audit')}
				>
					<div class="icon">🔍</div>
					<h3>Audit Trail</h3>
					<p>Compliance-konforme Nachverfolgung</p>
					<span class="badge">Aktiv</span>
				</button>
			</div>
		</div>
	</div>
</AdminLayout>

<style>
	.admin-dashboard {
		padding: 2rem;
		max-width: 1200px;
		margin: 0 auto;
	}
	
	.dashboard-header {
		text-align: center;
		margin-bottom: 3rem;
	}
	
	.dashboard-header h1 {
		font-size: 2.5rem;
		color: var(--primary-color, #2563eb);
		margin-bottom: 0.5rem;
	}
	
	.subtitle {
		color: var(--text-secondary, #6b7280);
		font-size: 1.1rem;
	}
	
	.quick-actions {
		margin-top: 2rem;
	}
	
	.action-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
		gap: 1.5rem;
	}
	
	.action-card {
		background: white;
		border: 2px solid var(--border-color, #e5e7eb);
		border-radius: 12px;
		padding: 2rem;
		text-align: center;
		cursor: pointer;
		transition: all 0.2s ease;
		position: relative;
		overflow: hidden;
	}
	
	.action-card:hover {
		border-color: var(--primary-color, #2563eb);
		transform: translateY(-2px);
		box-shadow: 0 8px 25px rgba(37, 99, 235, 0.15);
	}
	
	.action-card .icon {
		font-size: 3rem;
		margin-bottom: 1rem;
	}
	
	.action-card h3 {
		font-size: 1.3rem;
		margin-bottom: 0.5rem;
		color: var(--text-primary, #111827);
	}
	
	.action-card p {
		color: var(--text-secondary, #6b7280);
		margin-bottom: 1rem;
		line-height: 1.5;
	}
	
	.badge {
		display: inline-block;
		background: var(--primary-color, #2563eb);
		color: white;
		padding: 0.25rem 0.75rem;
		border-radius: 20px;
		font-size: 0.875rem;
		font-weight: 500;
	}
	
	.action-card.logs:hover {
		border-color: #059669;
	}
	
	.action-card.logs .badge {
		background: #059669;
	}
	
	.action-card.benchmarks:hover {
		border-color: #dc2626;
	}
	
	.action-card.benchmarks .badge {
		background: #dc2626;
	}
	
	.action-card.performance:hover {
		border-color: #d97706;
	}
	
	.action-card.performance .badge {
		background: #d97706;
	}
	
	.action-card.audit:hover {
		border-color: #7c3aed;
	}
	
	.action-card.audit .badge {
		background: #7c3aed;
	}
</style>
