<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [EIV] Entitäten Immer Verschlüsselt - Nur anonymisierte Patientendaten anzeigen
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Patientennamen anonymisiert
  [SF] Schweizer Formate - DD.MM.YYYY Datum, HH:MM Zeit
  [ZTS] Zero Tolerance Security - Sichere Datenverarbeitung
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { createEventDispatcher } from 'svelte';
  import ConsultationCreateModal from './ConsultationCreateModal.svelte';
  import SessionStatusBadge from './SessionStatusBadge.svelte';
  import Spinner from './common/Spinner.svelte';
  import SecurityBadge from './SecurityBadge.svelte';
  
  const dispatch = createEventDispatcher();
  
  // Session Interface basierend auf Datenbankschema [EIV][AIU][SF]
  interface Consultation {
    id: string;
    patientId: string;
    patientName: string; // Anonymisiert: "Max **** Muster ****" [AIU]
    sessionDate: string; // [SF] DD.MM.YYYY
    startTime: string | null; // [SF] HH:MM
    endTime: string | null; // [SF] HH:MM
    status: 'Scheduled' | 'InProgress' | 'Completed' | 'Cancelled';
    notesPreview: string; // Verschlüsselt, nur Vorschau [EIV]
    created: string; // [SF] DD.MM.YYYY HH:MM
    lastModified: string; // [SF] DD.MM.YYYY HH:MM
  }
  
  // State Management [ZTS]
  let consultations: Consultation[] = [];
  let filteredConsultations: Consultation[] = [];
  let loading = true;
  let error: string | null = null;
  let searchTerm = '';
  let statusFilter: 'all' | 'Scheduled' | 'InProgress' | 'Completed' | 'Cancelled' = 'all';
  let dateFrom = '';
  let dateTo = '';
  let sortBy: 'date' | 'patient' | 'status' | 'created' = 'date';
  let sortDirection: 'asc' | 'desc' = 'desc';
  let currentPage = 1;
  let itemsPerPage = 20;
  let showCreateModal = false;
  
  // Computed values
  $: totalPages = Math.ceil(filteredConsultations.length / itemsPerPage);
  $: paginatedConsultations = filteredConsultations.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );
  
  onMount(async () => {
    await loadConsultations();
  });
  
  async function loadConsultations() {
    try {
      loading = true;
      error = null;
      
      // Mock-Daten für Entwicklung [AIU][SF]
      // In Produktion: API-Call zu .NET Backend
      consultations = [
        {
          id: '1',
          patientId: '1',
          patientName: 'Max **** Muster ****',
          sessionDate: '22.01.2025',
          startTime: '09:30',
          endTime: '10:15',
          status: 'Completed',
          notesPreview: 'Routinekontrolle, Patient klagt über...',
          created: '20.01.2025 08:45',
          lastModified: '22.01.2025 10:20'
        },
        {
          id: '2',
          patientId: '2',
          patientName: 'Anna **** Schmidt ****',
          sessionDate: '22.01.2025',
          startTime: '14:00',
          endTime: null,
          status: 'Scheduled',
          notesPreview: '',
          created: '15.01.2025 16:30',
          lastModified: '15.01.2025 16:30'
        },
        {
          id: '3',
          patientId: '1',
          patientName: 'Max **** Muster ****',
          sessionDate: '22.01.2025',
          startTime: '11:00',
          endTime: null,
          status: 'InProgress',
          notesPreview: 'Nachkontrolle nach Behandlung...',
          created: '22.01.2025 10:55',
          lastModified: '22.01.2025 11:05'
        },
        {
          id: '4',
          patientId: '3',
          patientName: 'Peter **** Weber ****',
          sessionDate: '21.01.2025',
          startTime: '16:30',
          endTime: '17:00',
          status: 'Completed',
          notesPreview: 'Erstberatung, Anamnese durchgeführt...',
          created: '18.01.2025 14:20',
          lastModified: '21.01.2025 17:05'
        },
        {
          id: '5',
          patientId: '2',
          patientName: 'Anna **** Schmidt ****',
          sessionDate: '23.01.2025',
          startTime: '10:00',
          endTime: null,
          status: 'Cancelled',
          notesPreview: '',
          created: '20.01.2025 12:15',
          lastModified: '22.01.2025 09:30'
        }
      ];
      
      applyFiltersAndSort();
    } catch (err) {
      error = 'Fehler beim Laden der Konsultationsliste';
      console.error('Consultation loading error:', err);
    } finally {
      loading = false;
    }
  }
  
  function applyFiltersAndSort() {
    // Filter by search term [ZTS]
    filteredConsultations = consultations.filter(consultation => {
      const searchLower = searchTerm.toLowerCase();
      const matchesSearch = !searchTerm || (
        consultation.patientName.toLowerCase().includes(searchLower) ||
        consultation.notesPreview.toLowerCase().includes(searchLower)
      );
      
      // Status filter
      const matchesStatus = statusFilter === 'all' || consultation.status === statusFilter;
      
      // Date range filter [SF]
      let matchesDateRange = true;
      if (dateFrom || dateTo) {
        const consultationDate = new Date(consultation.sessionDate.split('.').reverse().join('-'));
        if (dateFrom) {
          const fromDate = new Date(dateFrom);
          matchesDateRange = matchesDateRange && consultationDate >= fromDate;
        }
        if (dateTo) {
          const toDate = new Date(dateTo);
          matchesDateRange = matchesDateRange && consultationDate <= toDate;
        }
      }
      
      return matchesSearch && matchesStatus && matchesDateRange;
    });
    
    // Sort consultations [ZTS]
    filteredConsultations.sort((a, b) => {
      let aValue: any, bValue: any;
      
      switch (sortBy) {
        case 'date':
          aValue = new Date(a.sessionDate.split('.').reverse().join('-'));
          bValue = new Date(b.sessionDate.split('.').reverse().join('-'));
          // Secondary sort by time
          if (aValue.getTime() === bValue.getTime()) {
            aValue = a.startTime || '00:00';
            bValue = b.startTime || '00:00';
          }
          break;
        case 'patient':
          aValue = a.patientName;
          bValue = b.patientName;
          break;
        case 'status':
          // Custom status order: InProgress, Scheduled, Completed, Cancelled
          const statusOrder = { InProgress: 0, Scheduled: 1, Completed: 2, Cancelled: 3 };
          aValue = statusOrder[a.status];
          bValue = statusOrder[b.status];
          break;
        case 'created':
          aValue = new Date(a.created.replace(/(\d{2})\.(\d{2})\.(\d{4}) (\d{2}):(\d{2})/, '$3-$2-$1T$4:$5'));
          bValue = new Date(b.created.replace(/(\d{2})\.(\d{2})\.(\d{4}) (\d{2}):(\d{2})/, '$3-$2-$1T$4:$5'));
          break;
        default:
          aValue = a.sessionDate;
          bValue = b.sessionDate;
      }
      
      if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
      if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
    
    // Reset to first page after filter/sort
    currentPage = 1;
  }
  
  function handleSort(column: typeof sortBy) {
    if (sortBy === column) {
      sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      sortBy = column;
      sortDirection = sortBy === 'date' ? 'desc' : 'asc'; // Default: newest first for dates
    }
    applyFiltersAndSort();
  }
  
  function handleSearch() {
    applyFiltersAndSort();
  }
  
  function openConsultation(consultationId: string) {
    dispatch('openConsultation', { consultationId });
  }
  
  function handleConsultationCreated() {
    showCreateModal = false;
    loadConsultations(); // Reload list
  }
  
  function formatTimeRange(startTime: string | null, endTime: string | null): string {
    if (!startTime) return 'Nicht geplant';
    if (!endTime) return `${startTime} - laufend`;
    return `${startTime} - ${endTime}`;
  }
  
  // Set default date range to current week [SF]
  function setCurrentWeek() {
    const today = new Date();
    const monday = new Date(today);
    monday.setDate(today.getDate() - today.getDay() + 1);
    const sunday = new Date(monday);
    sunday.setDate(monday.getDate() + 6);
    
    dateFrom = monday.toISOString().split('T')[0];
    dateTo = sunday.toISOString().split('T')[0];
    applyFiltersAndSort();
  }
  
  function clearDateFilter() {
    dateFrom = '';
    dateTo = '';
    applyFiltersAndSort();
  }
</script>

<div class="consultation-list-view">
  <!-- Header mit Sicherheits-Badge [AIU][ZTS] -->
  <div class="view-header">
    <div class="header-left">
      <h2>
        <span class="icon">📋</span>
        Konsultationsübersicht
      </h2>
      <SecurityBadge type="anonymization" tooltip="Patientendaten anonymisiert" />
    </div>
    
    <div class="header-actions">
      <button class="btn-primary" on:click={() => showCreateModal = true}>
        <span class="icon">➕</span>
        Neue Konsultation
      </button>
    </div>
  </div>
  
  <!-- Filter- und Suchbereich [ZTS][SF] -->
  <div class="filter-section">
    <div class="search-input-group">
      <input
        type="text"
        placeholder="Konsultation suchen (Patient, Notizen)..."
        bind:value={searchTerm}
        on:input={handleSearch}
        class="search-input"
      />
      <button class="search-button" on:click={handleSearch}>
        🔍
      </button>
    </div>
    
    <div class="filter-controls">
      <select bind:value={statusFilter} on:change={applyFiltersAndSort}>
        <option value="all">Alle Status</option>
        <option value="Scheduled">Geplant</option>
        <option value="InProgress">Laufend</option>
        <option value="Completed">Abgeschlossen</option>
        <option value="Cancelled">Abgebrochen</option>
      </select>
      
      <div class="date-filter">
        <input
          type="date"
          bind:value={dateFrom}
          on:change={applyFiltersAndSort}
          placeholder="Von"
          title="Von Datum"
        />
        <span class="date-separator">bis</span>
        <input
          type="date"
          bind:value={dateTo}
          on:change={applyFiltersAndSort}
          placeholder="Bis"
          title="Bis Datum"
        />
        <button class="btn-icon" on:click={setCurrentWeek} title="Aktuelle Woche">
          📅
        </button>
        <button class="btn-icon" on:click={clearDateFilter} title="Filter löschen">
          ❌
        </button>
      </div>
      
      <select bind:value={itemsPerPage} on:change={applyFiltersAndSort}>
        <option value={10}>10 pro Seite</option>
        <option value={20}>20 pro Seite</option>
        <option value={50}>50 pro Seite</option>
      </select>
    </div>
  </div>
  
  <!-- Konsultationsliste [EIV][AIU][SF] -->
  <div class="table-container">
    {#if loading}
      <div class="loading-state">
        <Spinner />
        <p>Konsultationsliste wird geladen...</p>
      </div>
    {:else if error}
      <div class="error-state">
        <p class="error-message">⚠️ {error}</p>
        <button class="btn-secondary" on:click={loadConsultations}>
          Erneut versuchen
        </button>
      </div>
    {:else if paginatedConsultations.length === 0}
      <div class="empty-state">
        <p>Keine Konsultationen gefunden.</p>
        {#if searchTerm || statusFilter !== 'all' || dateFrom || dateTo}
          <button class="btn-secondary" on:click={() => { 
            searchTerm = ''; 
            statusFilter = 'all'; 
            dateFrom = ''; 
            dateTo = ''; 
            handleSearch(); 
          }}>
            Alle Filter zurücksetzen
          </button>
        {/if}
      </div>
    {:else}
      <table class="consultation-table">
        <thead>
          <tr>
            <th>
              <button class="sort-button" on:click={() => handleSort('date')}>
                Datum & Zeit [SF]
                {#if sortBy === 'date'}
                  <span class="sort-indicator">{sortDirection === 'asc' ? '↑' : '↓'}</span>
                {/if}
              </button>
            </th>
            <th>
              <button class="sort-button" on:click={() => handleSort('patient')}>
                Patient [AIU]
                {#if sortBy === 'patient'}
                  <span class="sort-indicator">{sortDirection === 'asc' ? '↑' : '↓'}</span>
                {/if}
              </button>
            </th>
            <th>
              <button class="sort-button" on:click={() => handleSort('status')}>
                Status
                {#if sortBy === 'status'}
                  <span class="sort-indicator">{sortDirection === 'asc' ? '↑' : '↓'}</span>
                {/if}
              </button>
            </th>
            <th>Notizen [EIV]</th>
            <th>
              <button class="sort-button" on:click={() => handleSort('created')}>
                Erstellt
                {#if sortBy === 'created'}
                  <span class="sort-indicator">{sortDirection === 'asc' ? '↑' : '↓'}</span>
                {/if}
              </button>
            </th>
            <th>Letzte Änderung</th>
            <th>Aktionen</th>
          </tr>
        </thead>
        <tbody>
          {#each paginatedConsultations as consultation}
            <tr class="consultation-row" on:click={() => openConsultation(consultation.id)}>
              <td class="date-time">
                <div class="date">{consultation.sessionDate}</div>
                <div class="time">{formatTimeRange(consultation.startTime, consultation.endTime)}</div>
              </td>
              <td class="patient-name">{consultation.patientName}</td>
              <td class="status">
                <SessionStatusBadge status={consultation.status} />
              </td>
              <td class="notes-preview">
                {#if consultation.notesPreview}
                  <span class="notes-text">{consultation.notesPreview}</span>
                {:else}
                  <span class="no-notes">Keine Notizen</span>
                {/if}
              </td>
              <td class="created-date">{consultation.created}</td>
              <td class="modified-date">{consultation.lastModified}</td>
              <td class="actions">
                <button class="btn-icon" title="Konsultation öffnen" on:click|stopPropagation={() => openConsultation(consultation.id)}>
                  👁️
                </button>
                {#if consultation.status === 'Scheduled'}
                  <button class="btn-icon" title="Konsultation starten" on:click|stopPropagation={() => dispatch('startConsultation', { id: consultation.id })}>
                    ▶️
                  </button>
                {/if}
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    {/if}
  </div>
  
  <!-- Pagination [ZTS] -->
  {#if totalPages > 1}
    <div class="pagination">
      <button 
        class="btn-secondary" 
        disabled={currentPage === 1}
        on:click={() => { currentPage = 1; }}
      >
        ⏮️
      </button>
      <button 
        class="btn-secondary" 
        disabled={currentPage === 1}
        on:click={() => { currentPage--; }}
      >
        ◀️
      </button>
      
      <span class="page-info">
        Seite {currentPage} von {totalPages} 
        ({filteredConsultations.length} Konsultationen)
      </span>
      
      <button 
        class="btn-secondary" 
        disabled={currentPage === totalPages}
        on:click={() => { currentPage++; }}
      >
        ▶️
      </button>
      <button 
        class="btn-secondary" 
        disabled={currentPage === totalPages}
        on:click={() => { currentPage = totalPages; }}
      >
        ⏭️
      </button>
    </div>
  {/if}
</div>

<!-- Consultation Create Modal [ZTS] -->
{#if showCreateModal}
  <ConsultationCreateModal 
    on:close={() => showCreateModal = false}
    on:consultationCreated={handleConsultationCreated}
  />
{/if}

<style>
  .consultation-list-view {
    display: flex;
    flex-direction: column;
    height: 100%;
    padding: 16px;
    background-color: #ffffff;
  }
  
  .view-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
    padding-bottom: 16px;
    border-bottom: 1px solid #e5e7eb;
  }
  
  .header-left {
    display: flex;
    align-items: center;
    gap: 16px;
  }
  
  .header-left h2 {
    display: flex;
    align-items: center;
    gap: 8px;
    margin: 0;
    font-size: 24px;
    font-weight: 600;
    color: #1f2937;
  }
  
  .header-actions {
    display: flex;
    gap: 12px;
  }
  
  .filter-section {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 24px;
  }
  
  .search-input-group {
    display: flex;
    max-width: 400px;
  }
  
  .search-input {
    flex: 1;
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-right: none;
    border-radius: 6px 0 0 6px;
    font-size: 14px;
  }
  
  .search-button {
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-left: none;
    border-radius: 0 6px 6px 0;
    background-color: #f9fafb;
    cursor: pointer;
    font-size: 14px;
  }
  
  .filter-controls {
    display: flex;
    gap: 16px;
    align-items: center;
    flex-wrap: wrap;
  }
  
  .filter-controls select {
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;
  }
  
  .date-filter {
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .date-filter input {
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;
  }
  
  .date-separator {
    color: #6b7280;
    font-size: 14px;
  }
  
  .table-container {
    flex: 1;
    overflow: auto;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
  }
  
  .consultation-table {
    width: 100%;
    border-collapse: collapse;
    background-color: #ffffff;
  }
  
  .consultation-table th {
    background-color: #f9fafb;
    padding: 12px;
    text-align: left;
    font-weight: 600;
    color: #374151;
    border-bottom: 1px solid #e5e7eb;
    position: sticky;
    top: 0;
    z-index: 1;
  }
  
  .sort-button {
    display: flex;
    align-items: center;
    gap: 4px;
    background: none;
    border: none;
    cursor: pointer;
    font-weight: 600;
    color: #374151;
    padding: 0;
  }
  
  .sort-indicator {
    font-size: 12px;
  }
  
  .consultation-table td {
    padding: 12px;
    border-bottom: 1px solid #f3f4f6;
  }
  
  .consultation-row {
    cursor: pointer;
    transition: background-color 0.2s ease;
  }
  
  .consultation-row:hover {
    background-color: #f9fafb;
  }
  
  .date-time .date {
    font-weight: 500;
    color: #1f2937;
  }
  
  .date-time .time {
    font-size: 13px;
    color: #6b7280;
  }
  
  .patient-name {
    color: #1f2937;
    font-weight: 500;
  }
  
  .notes-preview {
    max-width: 200px;
  }
  
  .notes-text {
    color: #374151;
    font-size: 13px;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
  
  .no-notes {
    color: #9ca3af;
    font-style: italic;
    font-size: 13px;
  }
  
  .created-date, .modified-date {
    font-size: 13px;
    color: #6b7280;
  }
  
  .actions {
    text-align: center;
  }
  
  .btn-icon {
    background: none;
    border: none;
    cursor: pointer;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 16px;
    transition: background-color 0.2s ease;
    margin: 0 2px;
  }
  
  .btn-icon:hover {
    background-color: #f3f4f6;
  }
  
  .pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 8px;
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid #e5e7eb;
  }
  
  .page-info {
    margin: 0 16px;
    font-size: 14px;
    color: #6b7280;
  }
  
  .loading-state, .error-state, .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px;
    text-align: center;
  }
  
  .error-message {
    color: #dc2626;
    margin-bottom: 16px;
  }
  
  /* Button Styles */
  .btn-primary {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 16px;
    background-color: #3b82f6;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: background-color 0.2s ease;
  }
  
  .btn-primary:hover {
    background-color: #2563eb;
  }
  
  .btn-secondary {
    padding: 8px 16px;
    background-color: #f9fafb;
    color: #374151;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.2s ease;
  }
  
  .btn-secondary:hover:not(:disabled) {
    background-color: #f3f4f6;
  }
  
  .btn-secondary:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .icon {
    font-size: 16px;
  }
  
  /* Responsive Design [PSF] */
  @media (max-width: 768px) {
    .consultation-list-view {
      padding: 8px;
    }
    
    .view-header {
      flex-direction: column;
      align-items: stretch;
      gap: 16px;
    }
    
    .filter-section {
      gap: 12px;
    }
    
    .filter-controls {
      flex-direction: column;
      align-items: stretch;
    }
    
    .date-filter {
      justify-content: space-between;
    }
    
    .consultation-table {
      font-size: 13px;
    }
    
    .consultation-table th,
    .consultation-table td {
      padding: 8px;
    }
    
    .notes-preview {
      max-width: 150px;
    }
  }
</style>
