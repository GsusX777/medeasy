<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	
	// [ATV] Log filtering for audit trail compliance
	// [PSF] Patient safety through effective log monitoring
	
	interface LogFilters {
		level: string[];
		service: string[];
		timeRange: string;
		searchTerm: string;
	}
	
	export let filters: LogFilters;
	
	const dispatch = createEventDispatcher<{
		filtersChange: LogFilters;
	}>();
	
	const logLevels = [
		{ value: 'debug', label: 'Debug', color: '#6b7280', icon: '🔍' },
		{ value: 'info', label: 'Info', color: '#2563eb', icon: 'ℹ️' },
		{ value: 'warn', label: 'Warning', color: '#d97706', icon: '⚠️' },
		{ value: 'error', label: 'Error', color: '#dc2626', icon: '❌' }
	];
	
	const services = [
		{ value: 'backend', label: '.NET Backend', icon: '🔧' },
		{ value: 'ai-service', label: 'Python AI Service', icon: '🤖' },
		{ value: 'frontend', label: 'Svelte Frontend', icon: '🖥️' }
	];
	
	const timeRanges = [
		{ value: '15m', label: 'Letzte 15 Min' },
		{ value: '1h', label: 'Letzte Stunde' },
		{ value: '6h', label: 'Letzte 6 Stunden' },
		{ value: '24h', label: 'Letzte 24 Stunden' },
		{ value: '7d', label: 'Letzte 7 Tage' }
	];
	
	function toggleLevel(level: string) {
		if (filters.level.includes(level)) {
			filters.level = filters.level.filter(l => l !== level);
		} else {
			filters.level = [...filters.level, level];
		}
		emitChange();
	}
	
	function toggleService(service: string) {
		if (filters.service.includes(service)) {
			filters.service = filters.service.filter(s => s !== service);
		} else {
			filters.service = [...filters.service, service];
		}
		emitChange();
	}
	
	function updateTimeRange(range: string) {
		filters.timeRange = range;
		emitChange();
	}
	
	function updateSearchTerm(term: string) {
		filters.searchTerm = term;
		emitChange();
	}
	
	function emitChange() {
		dispatch('filtersChange', filters);
	}
	
	function clearFilters() {
		filters = {
			level: ['info', 'warn', 'error'],
			service: ['backend', 'ai-service', 'frontend'],
			timeRange: '1h',
			searchTerm: ''
		};
		emitChange();
	}
	
	function selectAllLevels() {
		filters.level = logLevels.map(l => l.value);
		emitChange();
	}
	
	function selectAllServices() {
		filters.service = services.map(s => s.value);
		emitChange();
	}
</script>

<div class="log-filters">
	<div class="filters-header">
		<h3>🔍 Filter & Suche</h3>
		<button class="clear-btn" on:click={clearFilters}>
			🗑️ Zurücksetzen
		</button>
	</div>
	
	<div class="filters-grid">
		<!-- Search Term -->
		<div class="filter-group">
			<label class="filter-label">
				🔎 Suche
			</label>
			<input
				type="text"
				class="search-input"
				placeholder="Suche in Nachrichten, Request-IDs..."
				bind:value={filters.searchTerm}
				on:input={(e) => updateSearchTerm(e.currentTarget.value)}
			/>
		</div>
		
		<!-- Time Range -->
		<div class="filter-group">
			<label class="filter-label">
				⏰ Zeitraum
			</label>
			<select 
				class="time-select"
				bind:value={filters.timeRange}
				on:change={(e) => updateTimeRange(e.currentTarget.value)}
			>
				{#each timeRanges as range}
					<option value={range.value}>{range.label}</option>
				{/each}
			</select>
		</div>
		
		<!-- Log Levels -->
		<div class="filter-group">
			<div class="filter-header">
				<label class="filter-label">
					📊 Log-Level
				</label>
				<button class="select-all-btn" on:click={selectAllLevels}>
					Alle
				</button>
			</div>
			<div class="checkbox-group">
				{#each logLevels as level}
					<label class="checkbox-item">
						<input
							type="checkbox"
							checked={filters.level.includes(level.value)}
							on:change={() => toggleLevel(level.value)}
						/>
						<span class="checkbox-label">
							<span class="level-icon">{level.icon}</span>
							<span class="level-text" style="color: {level.color}">
								{level.label}
							</span>
						</span>
					</label>
				{/each}
			</div>
		</div>
		
		<!-- Services -->
		<div class="filter-group">
			<div class="filter-header">
				<label class="filter-label">
					🔧 Services
				</label>
				<button class="select-all-btn" on:click={selectAllServices}>
					Alle
				</button>
			</div>
			<div class="checkbox-group">
				{#each services as service}
					<label class="checkbox-item">
						<input
							type="checkbox"
							checked={filters.service.includes(service.value)}
							on:change={() => toggleService(service.value)}
						/>
						<span class="checkbox-label">
							<span class="service-icon">{service.icon}</span>
							<span class="service-text">
								{service.label}
							</span>
						</span>
					</label>
				{/each}
			</div>
		</div>
	</div>
	
	<!-- Active Filters Summary -->
	<div class="active-filters">
		<span class="active-label">Aktive Filter:</span>
		<div class="filter-tags">
			{#each filters.level as level}
				<span class="filter-tag level-tag">
					{logLevels.find(l => l.value === level)?.icon}
					{logLevels.find(l => l.value === level)?.label}
				</span>
			{/each}
			{#each filters.service as service}
				<span class="filter-tag service-tag">
					{services.find(s => s.value === service)?.icon}
					{services.find(s => s.value === service)?.label}
				</span>
			{/each}
			<span class="filter-tag time-tag">
				⏰ {timeRanges.find(t => t.value === filters.timeRange)?.label}
			</span>
			{#if filters.searchTerm}
				<span class="filter-tag search-tag">
					🔎 "{filters.searchTerm}"
				</span>
			{/if}
		</div>
	</div>
</div>

<style>
	.log-filters {
		background: white;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 12px;
		padding: 1.5rem;
		margin-bottom: 1.5rem;
	}
	
	.filters-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 1.5rem;
	}
	
	.filters-header h3 {
		margin: 0;
		color: var(--text-primary, #111827);
	}
	
	.clear-btn {
		padding: 0.5rem 1rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		background: white;
		cursor: pointer;
		transition: all 0.2s ease;
		font-size: 0.875rem;
	}
	
	.clear-btn:hover {
		border-color: #dc2626;
		color: #dc2626;
	}
	
	.filters-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: 1.5rem;
		margin-bottom: 1.5rem;
	}
	
	.filter-group {
		display: flex;
		flex-direction: column;
		gap: 0.75rem;
	}
	
	.filter-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
	}
	
	.filter-label {
		font-weight: 500;
		color: var(--text-primary, #111827);
		font-size: 0.875rem;
	}
	
	.select-all-btn {
		padding: 0.25rem 0.5rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 4px;
		background: white;
		cursor: pointer;
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
		transition: all 0.2s ease;
	}
	
	.select-all-btn:hover {
		border-color: var(--primary-color, #2563eb);
		color: var(--primary-color, #2563eb);
	}
	
	.search-input {
		padding: 0.5rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		font-size: 0.875rem;
		transition: border-color 0.2s ease;
	}
	
	.search-input:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
		box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
	}
	
	.time-select {
		padding: 0.5rem 0.75rem;
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 6px;
		font-size: 0.875rem;
		background: white;
		cursor: pointer;
	}
	
	.time-select:focus {
		outline: none;
		border-color: var(--primary-color, #2563eb);
	}
	
	.checkbox-group {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}
	
	.checkbox-item {
		display: flex;
		align-items: center;
		cursor: pointer;
		padding: 0.25rem 0;
	}
	
	.checkbox-item input[type="checkbox"] {
		margin-right: 0.5rem;
		cursor: pointer;
	}
	
	.checkbox-label {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		font-size: 0.875rem;
	}
	
	.level-icon,
	.service-icon {
		font-size: 1rem;
	}
	
	.active-filters {
		border-top: 1px solid var(--border-color, #e5e7eb);
		padding-top: 1rem;
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		gap: 0.75rem;
	}
	
	.active-label {
		font-weight: 500;
		color: var(--text-secondary, #6b7280);
		font-size: 0.875rem;
	}
	
	.filter-tags {
		display: flex;
		flex-wrap: wrap;
		gap: 0.5rem;
	}
	
	.filter-tag {
		display: inline-flex;
		align-items: center;
		gap: 0.25rem;
		padding: 0.25rem 0.5rem;
		background: var(--bg-secondary, #f8fafc);
		border: 1px solid var(--border-color, #e5e7eb);
		border-radius: 4px;
		font-size: 0.75rem;
		color: var(--text-secondary, #6b7280);
	}
	
	.filter-tag.level-tag {
		border-color: #2563eb;
		background: #eff6ff;
		color: #2563eb;
	}
	
	.filter-tag.service-tag {
		border-color: #059669;
		background: #ecfdf5;
		color: #059669;
	}
	
	.filter-tag.time-tag {
		border-color: #d97706;
		background: #fffbeb;
		color: #d97706;
	}
	
	.filter-tag.search-tag {
		border-color: #7c3aed;
		background: #f3f4f6;
		color: #7c3aed;
	}
	
	@media (max-width: 768px) {
		.filters-grid {
			grid-template-columns: 1fr;
		}
		
		.active-filters {
			flex-direction: column;
			align-items: flex-start;
		}
	}
</style>
