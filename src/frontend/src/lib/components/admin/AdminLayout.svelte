<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	
	// [ATV] Admin layout with audit trail integration
	// [ZTS] Zero tolerance security for admin access
	// [PSF] Patient safety first - admin actions logged
	
	$: currentPath = $page.url.pathname;
	
	const adminNavItems = [
		{ 
			path: '/admin', 
			label: 'Dashboard', 
			icon: '🏠',
			description: 'System-Übersicht'
		},
		{ 
			path: '/admin/logs', 
			label: 'System Logs', 
			icon: '📋',
			description: 'Alle Service-Logs'
		},
		{ 
			path: '/admin/benchmarks', 
			label: 'Benchmarks', 
			icon: '🧪',
			description: 'Whisper-Tests'
		},
		{ 
			path: '/admin/performance', 
			label: 'Performance', 
			icon: '📊',
			description: 'System-Metriken'
		},
		{ 
			path: '/admin/audit', 
			label: 'Audit Trail', 
			icon: '🔍',
			description: 'Compliance-Logs'
		}
	];
	
	function isActive(path: string): boolean {
		if (path === '/admin') {
			return currentPath === '/admin';
		}
		return currentPath.startsWith(path);
	}
</script>

<div class="admin-layout">
	<aside class="admin-sidebar">
		<div class="sidebar-header">
			<h2>🔧 Admin Panel</h2>
			<p class="version">MedEasy v1.0</p>
		</div>
		
		<nav class="admin-nav">
			{#each adminNavItems as item}
				<button
					class="nav-item"
					class:active={isActive(item.path)}
					on:click={() => goto(item.path)}
				>
					<span class="nav-icon">{item.icon}</span>
					<div class="nav-content">
						<span class="nav-label">{item.label}</span>
						<span class="nav-description">{item.description}</span>
					</div>
				</button>
			{/each}
		</nav>
		
		<div class="sidebar-footer">
			<div class="system-status">
				<div class="status-indicator healthy"></div>
				<span>System Healthy</span>
			</div>
			
			<button 
				class="back-button"
				on:click={() => goto('/')}
			>
				← Zurück zur App
			</button>
		</div>
	</aside>
	
	<main class="admin-content">
		<slot />
	</main>
</div>

<style>
	.admin-layout {
		display: flex;
		min-height: 100vh;
		background: var(--bg-secondary, #f8fafc);
	}
	
	.admin-sidebar {
		width: 280px;
		background: white;
		border-right: 1px solid var(--border-color, #e5e7eb);
		display: flex;
		flex-direction: column;
		position: fixed;
		height: 100vh;
		overflow-y: auto;
	}
	
	.sidebar-header {
		padding: 2rem 1.5rem 1rem;
		border-bottom: 1px solid var(--border-color, #e5e7eb);
	}
	
	.sidebar-header h2 {
		font-size: 1.5rem;
		color: var(--primary-color, #2563eb);
		margin-bottom: 0.25rem;
	}
	
	.version {
		color: var(--text-secondary, #6b7280);
		font-size: 0.875rem;
	}
	
	.admin-nav {
		flex: 1;
		padding: 1rem 0;
	}
	
	.nav-item {
		width: 100%;
		display: flex;
		align-items: center;
		padding: 0.75rem 1.5rem;
		border: none;
		background: none;
		cursor: pointer;
		transition: all 0.2s ease;
		text-align: left;
	}
	
	.nav-item:hover {
		background: var(--bg-hover, #f1f5f9);
	}
	
	.nav-item.active {
		background: var(--primary-bg, #eff6ff);
		border-right: 3px solid var(--primary-color, #2563eb);
	}
	
	.nav-icon {
		font-size: 1.25rem;
		margin-right: 0.75rem;
		width: 24px;
		text-align: center;
	}
	
	.nav-content {
		display: flex;
		flex-direction: column;
	}
	
	.nav-label {
		font-weight: 500;
		color: var(--text-primary, #111827);
		margin-bottom: 0.125rem;
	}
	
	.nav-description {
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.nav-item.active .nav-label {
		color: var(--primary-color, #2563eb);
	}
	
	.sidebar-footer {
		padding: 1.5rem;
		border-top: 1px solid var(--border-color, #e5e7eb);
	}
	
	.system-status {
		display: flex;
		align-items: center;
		margin-bottom: 1rem;
		font-size: 0.875rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.status-indicator {
		width: 8px;
		height: 8px;
		border-radius: 50%;
		margin-right: 0.5rem;
	}
	
	.status-indicator.healthy {
		background: #10b981;
	}
	
	.status-indicator.warning {
		background: #f59e0b;
	}
	
	.status-indicator.error {
		background: #ef4444;
	}
	
	.back-button {
		width: 100%;
		padding: 0.5rem 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		color: var(--text-secondary, #6b7280);
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.875rem;
	}
	
	.back-button:hover {
		border-color: var(--primary-color, #2563eb);
		color: var(--primary-color, #2563eb);
	}
	
	.admin-content {
		flex: 1;
		margin-left: 280px;
		min-height: 100vh;
	}
	
	@media (max-width: 768px) {
		.admin-sidebar {
			width: 100%;
			position: relative;
			height: auto;
		}
		
		.admin-content {
			margin-left: 0;
		}
	}
</style>
