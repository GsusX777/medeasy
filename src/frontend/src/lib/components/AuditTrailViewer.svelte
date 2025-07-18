<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- MedEasy Audit Trail Viewer [ATV] -->
<script lang="ts">
  import { onMount } from 'svelte';
  import { auditApi } from '$lib/api/database';
  import type { AuditLogDto, AuditStatisticsDto } from '$lib/types/database';

  // Component props
  export let entityName: string | null = null;
  export let entityId: string | null = null;
  export let limit: number = 50;
  export let showStatistics: boolean = true;
  export let adminOnly: boolean = true;

  // Component state
  let loading = true;
  let error: string | null = null;
  let auditLogs: AuditLogDto[] = [];
  let statistics: AuditStatisticsDto | null = null;
  let isAdmin = false; // This should be determined by actual auth system
  let offset = 0;
  let hasMore = false;

  // Filter options
  let filterAction = '';
  let filterUser = '';
  let filterSensitiveOnly = false;
  let dateFrom: string | null = null;
  let dateTo: string | null = null;

  onMount(async () => {
    // In a real implementation, this would come from authentication
    isAdmin = localStorage.getItem('medeasy_is_admin') === 'true';
    
    if (adminOnly && !isAdmin) {
      error = 'Nur Administratoren können auf den Audit-Trail zugreifen.';
      loading = false;
      return;
    }
    
    await loadAuditLogs();
  });

  /**
   * Load audit logs based on current filters
   */
  async function loadAuditLogs(resetOffset = true): Promise<void> {
    try {
      loading = true;
      error = null;
      
      if (resetOffset) {
        offset = 0;
      }
      
      // If entity name and ID are provided, load entity-specific logs
      if (entityName && entityId) {
        auditLogs = await auditApi.getEntityAuditLogs(
          entityName,
          entityId,
          limit,
          offset
        );
      } else {
        // Otherwise load recent logs
        auditLogs = await auditApi.getRecentAuditLogs(limit);
      }
      
      // Apply filters client-side
      applyFilters();
      
      // Check if there might be more logs
      hasMore = auditLogs.length === limit;
      
      // Load statistics if needed
      if (showStatistics) {
        try {
          // This would call a statistics endpoint in a real implementation
          // For now we'll calculate simple statistics from the loaded logs
          calculateStatistics();
        } catch (err) {
          console.error('Failed to load audit statistics:', err);
        }
      }
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
    } finally {
      loading = false;
    }
  }

  /**
   * Apply filters to the loaded audit logs
   */
  function applyFilters(): void {
    let filtered = [...auditLogs];
    
    if (filterAction) {
      filtered = filtered.filter(log => log.action === filterAction);
    }
    
    if (filterUser) {
      filtered = filtered.filter(log => log.user_id.includes(filterUser));
    }
    
    if (filterSensitiveOnly) {
      filtered = filtered.filter(log => log.contains_sensitive_data);
    }
    
    if (dateFrom) {
      const fromDate = new Date(dateFrom);
      filtered = filtered.filter(log => new Date(log.timestamp) >= fromDate);
    }
    
    if (dateTo) {
      const toDate = new Date(dateTo);
      toDate.setHours(23, 59, 59, 999); // End of day
      filtered = filtered.filter(log => new Date(log.timestamp) <= toDate);
    }
    
    auditLogs = filtered;
  }

  /**
   * Calculate simple statistics from the loaded logs
   */
  function calculateStatistics(): void {
    const actionCounts: Record<string, number> = {};
    const entityCounts: Record<string, number> = {};
    const userCounts: Record<string, number> = {};
    let sensitiveCount = 0;
    
    auditLogs.forEach(log => {
      // Count actions
      actionCounts[log.action] = (actionCounts[log.action] || 0) + 1;
      
      // Count entities
      entityCounts[log.entity_name] = (entityCounts[log.entity_name] || 0) + 1;
      
      // Count users
      userCounts[log.user_id] = (userCounts[log.user_id] || 0) + 1;
      
      // Count sensitive data access
      if (log.contains_sensitive_data) {
        sensitiveCount++;
      }
    });
    
    statistics = {
      action_counts: actionCounts,
      entity_counts: entityCounts,
      user_counts: userCounts,
      sensitive_data_access_count: sensitiveCount,
      total_logs: auditLogs.length
    };
  }

  /**
   * Load more audit logs
   */
  async function loadMore(): Promise<void> {
    offset += limit;
    await loadAuditLogs(false);
  }

  /**
   * Reset all filters
   */
  function resetFilters(): void {
    filterAction = '';
    filterUser = '';
    filterSensitiveOnly = false;
    dateFrom = null;
    dateTo = null;
    loadAuditLogs();
  }

  /**
   * Format date for display
   */
  function formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString('de-CH');
  }

  /**
   * Get CSS class for action type
   */
  function getActionClass(action: string): string {
    switch (action) {
      case 'INSERT': return 'insert';
      case 'UPDATE': return 'update';
      case 'DELETE': return 'delete';
      case 'READ': return 'read';
      default: return '';
    }
  }
</script>

<div class="audit-trail">
  <h2>Audit-Trail {entityName ? `für ${entityName} ${entityId}` : ''}</h2>
  
  {#if loading && auditLogs.length === 0}
    <div class="loading">
      <span class="loader"></span>
      <p>Lade Audit-Logs...</p>
    </div>
  {:else if error}
    <div class="error">
      <p>{error}</p>
      {#if !adminOnly || isAdmin}
        <button on:click={() => loadAuditLogs()}>
          Erneut versuchen
        </button>
      {/if}
    </div>
  {:else}
    {#if showStatistics && statistics}
      <div class="statistics">
        <h3>Audit-Statistiken</h3>
        <div class="stats-grid">
          <div class="stat-card">
            <h4>Aktionen</h4>
            <ul>
              {#each Object.entries(statistics.action_counts) as [action, count]}
                <li>
                  <span class={`action-badge ${getActionClass(action)}`}>{action}</span>
                  <span class="count">{count}</span>
                </li>
              {/each}
            </ul>
          </div>
          
          <div class="stat-card">
            <h4>Entitäten</h4>
            <ul>
              {#each Object.entries(statistics.entity_counts) as [entity, count]}
                <li>
                  <span class="entity">{entity}</span>
                  <span class="count">{count}</span>
                </li>
              {/each}
            </ul>
          </div>
          
          <div class="stat-card">
            <h4>Benutzer</h4>
            <ul>
              {#each Object.entries(statistics.user_counts) as [user, count]}
                <li>
                  <span class="user">{user}</span>
                  <span class="count">{count}</span>
                </li>
              {/each}
            </ul>
          </div>
          
          <div class="stat-card sensitive">
            <h4>Sensible Daten</h4>
            <div class="sensitive-count">
              <span class="count">{statistics.sensitive_data_access_count}</span>
              <span class="label">Zugriffe auf sensible Daten</span>
            </div>
          </div>
        </div>
      </div>
    {/if}
    
    <div class="filters">
      <h3>Filter</h3>
      <div class="filter-grid">
        <div class="filter-item">
          <label for="filter-action">Aktion</label>
          <select id="filter-action" bind:value={filterAction}>
            <option value="">Alle Aktionen</option>
            <option value="INSERT">INSERT</option>
            <option value="UPDATE">UPDATE</option>
            <option value="DELETE">DELETE</option>
            <option value="READ">READ</option>
          </select>
        </div>
        
        <div class="filter-item">
          <label for="filter-user">Benutzer</label>
          <input 
            type="text" 
            id="filter-user" 
            bind:value={filterUser} 
            placeholder="Benutzer-ID"
          />
        </div>
        
        <div class="filter-item">
          <label for="filter-date-from">Von Datum</label>
          <input 
            type="date" 
            id="filter-date-from" 
            bind:value={dateFrom}
          />
        </div>
        
        <div class="filter-item">
          <label for="filter-date-to">Bis Datum</label>
          <input 
            type="date" 
            id="filter-date-to" 
            bind:value={dateTo}
          />
        </div>
        
        <div class="filter-item checkbox">
          <label for="filter-sensitive">
            <input 
              type="checkbox" 
              id="filter-sensitive" 
              bind:checked={filterSensitiveOnly}
            />
            Nur sensible Daten
          </label>
        </div>
        
        <div class="filter-actions">
          <button on:click={() => applyFilters()}>Filtern</button>
          <button class="reset" on:click={resetFilters}>Zurücksetzen</button>
        </div>
      </div>
    </div>
    
    {#if auditLogs.length === 0}
      <div class="empty-state">
        <p>Keine Audit-Logs gefunden.</p>
      </div>
    {:else}
      <div class="audit-logs">
        <table>
          <thead>
            <tr>
              <th>Zeitstempel</th>
              <th>Aktion</th>
              <th>Entität</th>
              <th>ID</th>
              <th>Benutzer</th>
              <th>Sensibel</th>
            </tr>
          </thead>
          <tbody>
            {#each auditLogs as log}
              <tr class={log.contains_sensitive_data ? 'sensitive' : ''}>
                <td>{formatDate(log.timestamp)}</td>
                <td>
                  <span class={`action-badge ${getActionClass(log.action)}`}>
                    {log.action}
                  </span>
                </td>
                <td>{log.entity_name}</td>
                <td class="entity-id">{log.entity_id}</td>
                <td>{log.user_id}</td>
                <td>
                  {#if log.contains_sensitive_data}
                    <span class="sensitive-icon">🔒</span>
                  {/if}
                </td>
              </tr>
              {#if log.changes}
                <tr class="changes-row">
                  <td colspan="6">
                    <div class="changes">
                      <strong>Änderungen:</strong>
                      <pre>{log.changes}</pre>
                    </div>
                  </td>
                </tr>
              {/if}
            {/each}
          </tbody>
        </table>
        
        {#if hasMore}
          <div class="load-more">
            <button on:click={loadMore} disabled={loading}>
              {loading ? 'Wird geladen...' : 'Mehr laden'}
            </button>
          </div>
        {/if}
      </div>
    {/if}
  {/if}
</div>

<style>
  .audit-trail {
    width: 100%;
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
  }
  
  h2 {
    color: #2c3e50;
    margin-bottom: 20px;
    border-bottom: 2px solid #3498db;
    padding-bottom: 10px;
  }
  
  h3 {
    color: #2c3e50;
    margin: 15px 0;
  }
  
  .loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 0;
  }
  
  .loader {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #3498db;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 20px;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  .error {
    background-color: #ffecec;
    color: #e74c3c;
    padding: 15px;
    border-radius: 5px;
    margin: 20px 0;
    border-left: 4px solid #e74c3c;
  }
  
  .empty-state {
    text-align: center;
    padding: 40px;
    color: #7f8c8d;
  }
  
  /* Statistics styles */
  .statistics {
    background-color: #f8f9fa;
    padding: 15px;
    border-radius: 5px;
    margin-bottom: 20px;
  }
  
  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 15px;
  }
  
  .stat-card {
    background-color: white;
    padding: 15px;
    border-radius: 5px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  
  .stat-card h4 {
    margin-top: 0;
    color: #34495e;
    border-bottom: 1px solid #eee;
    padding-bottom: 8px;
  }
  
  .stat-card ul {
    list-style: none;
    padding: 0;
    margin: 0;
  }
  
  .stat-card li {
    display: flex;
    justify-content: space-between;
    padding: 5px 0;
    border-bottom: 1px solid #f5f5f5;
  }
  
  .stat-card.sensitive {
    background-color: #f8f5ff;
  }
  
  .sensitive-count {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
  }
  
  .sensitive-count .count {
    font-size: 2rem;
    font-weight: bold;
    color: #8e44ad;
  }
  
  .sensitive-count .label {
    text-align: center;
    color: #7f8c8d;
    font-size: 0.9rem;
  }
  
  /* Filter styles */
  .filters {
    background-color: #f8f9fa;
    padding: 15px;
    border-radius: 5px;
    margin-bottom: 20px;
  }
  
  .filter-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 15px;
  }
  
  .filter-item {
    display: flex;
    flex-direction: column;
  }
  
  .filter-item label {
    margin-bottom: 5px;
    font-weight: bold;
  }
  
  .filter-item input,
  .filter-item select {
    padding: 8px;
    border: 1px solid #ddd;
    border-radius: 4px;
  }
  
  .filter-item.checkbox {
    flex-direction: row;
    align-items: center;
  }
  
  .filter-item.checkbox input {
    margin-right: 8px;
  }
  
  .filter-actions {
    display: flex;
    gap: 10px;
    align-items: flex-end;
  }
  
  /* Table styles */
  table {
    width: 100%;
    border-collapse: collapse;
    margin: 20px 0;
  }
  
  th, td {
    padding: 12px 15px;
    text-align: left;
    border-bottom: 1px solid #ddd;
  }
  
  th {
    background-color: #f8f9fa;
    font-weight: bold;
  }
  
  tr:hover {
    background-color: #f5f5f5;
  }
  
  tr.sensitive {
    background-color: #fff5f5;
  }
  
  tr.sensitive:hover {
    background-color: #ffe5e5;
  }
  
  .action-badge {
    display: inline-block;
    padding: 3px 8px;
    border-radius: 3px;
    font-size: 0.8rem;
    font-weight: bold;
    color: white;
  }
  
  .action-badge.insert {
    background-color: #2ecc71;
  }
  
  .action-badge.update {
    background-color: #3498db;
  }
  
  .action-badge.delete {
    background-color: #e74c3c;
  }
  
  .action-badge.read {
    background-color: #95a5a6;
  }
  
  .entity-id {
    font-family: monospace;
    font-size: 0.9rem;
  }
  
  .sensitive-icon {
    font-size: 1.2rem;
    color: #e74c3c;
  }
  
  .changes-row {
    background-color: #f9f9f9;
  }
  
  .changes {
    padding: 10px;
    font-size: 0.9rem;
  }
  
  .changes pre {
    margin: 5px 0 0;
    padding: 10px;
    background-color: #f5f5f5;
    border-radius: 3px;
    overflow-x: auto;
    white-space: pre-wrap;
  }
  
  /* Button styles */
  button {
    padding: 8px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    background-color: #3498db;
    color: white;
    font-weight: bold;
    transition: background-color 0.3s;
  }
  
  button:hover {
    background-color: #2980b9;
  }
  
  button.reset {
    background-color: #95a5a6;
  }
  
  button.reset:hover {
    background-color: #7f8c8d;
  }
  
  button:disabled {
    background-color: #bdc3c7;
    cursor: not-allowed;
  }
  
  .load-more {
    text-align: center;
    margin: 20px 0;
  }
</style>
