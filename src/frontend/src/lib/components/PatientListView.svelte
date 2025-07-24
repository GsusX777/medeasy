<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [EIV] Entitäten Immer Verschlüsselt - Nur anonymisierte Daten anzeigen
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Alle Patientendaten anonymisiert
  [SF] Schweizer Formate - DD.MM.YYYY Datumsformat
  [ZTS] Zero Tolerance Security - Sichere Datenverarbeitung
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { createEventDispatcher } from 'svelte';
  import PatientCreateModal from './PatientCreateModal.svelte';
  import PatientImportButton from './PatientImportButton.svelte';
  import Spinner from './common/Spinner.svelte';
  import SecurityBadge from './SecurityBadge.svelte';
  
  const dispatch = createEventDispatcher();
  
  // Patient Interface basierend auf Datenbankschema [EIV][AIU]
  interface Patient {
    id: string;
    anonymizedFirstName: string; // [AIU]
    anonymizedLastName: string; // [AIU]
    anonymizedDateOfBirth: string; // [AIU][SF] DD.MM.YYYY
    insuranceNumberHash: string; // Maskiert anzeigen
    sessionCount: number; // Anzahl Sessions
    created: string; // [SF] DD.MM.YYYY HH:MM
    lastModified: string; // [SF] DD.MM.YYYY HH:MM
  }
  
  // State Management [ZTS]
  let patients: Patient[] = [];
  let filteredPatients: Patient[] = [];
  let loading = true;
  let error: string | null = null;
  let searchTerm = '';
  let sortBy: 'name' | 'dateOfBirth' | 'created' | 'sessionCount' = 'name';
  let sortDirection: 'asc' | 'desc' = 'asc';
  let currentPage = 1;
  let itemsPerPage = 20;
  let showCreateModal = false;
  
  // Computed values
  $: totalPages = Math.ceil(filteredPatients.length / itemsPerPage);
  $: paginatedPatients = filteredPatients.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );
  
  onMount(async () => {
    await loadPatients();
  });
  
  async function loadPatients() {
    try {
      loading = true;
      error = null;
      
      // Mock-Daten für Entwicklung [AIU][SF]
      // In Produktion: API-Call zu .NET Backend
      patients = [
        {
          id: '1',
          anonymizedFirstName: 'Max ****',
          anonymizedLastName: 'Muster ****',
          anonymizedDateOfBirth: '15.03.1980',
          insuranceNumberHash: '756.****.****.45',
          sessionCount: 3,
          created: '10.01.2025 09:30',
          lastModified: '20.01.2025 14:15'
        },
        {
          id: '2',
          anonymizedFirstName: 'Anna ****',
          anonymizedLastName: 'Schmidt ****',
          anonymizedDateOfBirth: '22.07.1975',
          insuranceNumberHash: '756.****.****.78',
          sessionCount: 1,
          created: '12.01.2025 11:20',
          lastModified: '12.01.2025 11:20'
        },
        {
          id: '3',
          anonymizedFirstName: 'Peter ****',
          anonymizedLastName: 'Weber ****',
          anonymizedDateOfBirth: '08.11.1990',
          insuranceNumberHash: '756.****.****.23',
          sessionCount: 5,
          created: '05.01.2025 16:45',
          lastModified: '21.01.2025 10:30'
        }
      ];
      
      applyFiltersAndSort();
    } catch (err) {
      error = 'Fehler beim Laden der Patientenliste';
      console.error('Patient loading error:', err);
    } finally {
      loading = false;
    }
  }
  
  function applyFiltersAndSort() {
    // Filter by search term [ZTS]
    filteredPatients = patients.filter(patient => {
      const searchLower = searchTerm.toLowerCase();
      return (
        patient.anonymizedFirstName.toLowerCase().includes(searchLower) ||
        patient.anonymizedLastName.toLowerCase().includes(searchLower) ||
        patient.insuranceNumberHash.includes(searchLower)
      );
    });
    
    // Sort patients [ZTS]
    filteredPatients.sort((a, b) => {
      let aValue: any, bValue: any;
      
      switch (sortBy) {
        case 'name':
          aValue = `${a.anonymizedLastName} ${a.anonymizedFirstName}`;
          bValue = `${b.anonymizedLastName} ${b.anonymizedFirstName}`;
          break;
        case 'dateOfBirth':
          aValue = new Date(a.anonymizedDateOfBirth.split('.').reverse().join('-'));
          bValue = new Date(b.anonymizedDateOfBirth.split('.').reverse().join('-'));
          break;
        case 'created':
          aValue = new Date(a.created.replace(/(\d{2})\.(\d{2})\.(\d{4}) (\d{2}):(\d{2})/, '$3-$2-$1T$4:$5'));
          bValue = new Date(b.created.replace(/(\d{2})\.(\d{2})\.(\d{4}) (\d{2}):(\d{2})/, '$3-$2-$1T$4:$5'));
          break;
        case 'sessionCount':
          aValue = a.sessionCount;
          bValue = b.sessionCount;
          break;
        default:
          aValue = a.anonymizedFirstName;
          bValue = b.anonymizedFirstName;
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
      sortDirection = 'asc';
    }
    applyFiltersAndSort();
  }
  
  function handleSearch() {
    applyFiltersAndSort();
  }
  
  function openPatient(patientId: string) {
    dispatch('openPatient', { patientId });
  }
  
  function handlePatientCreated() {
    showCreateModal = false;
    loadPatients(); // Reload list
  }
</script>

<div class="patient-list-view">
  <!-- Header mit Sicherheits-Badge [AIU][ZTS] -->
  <div class="view-header">
    <div class="header-left">
      <h2>
        <span class="icon">👥</span>
        Patientenliste
      </h2>
      <SecurityBadge type="anonymization" tooltip="Alle Daten anonymisiert" />
    </div>
    
    <div class="header-actions">
      <button class="btn-primary" on:click={() => showCreateModal = true}>
        <span class="icon">➕</span>
        Patient anlegen
      </button>
      <PatientImportButton disabled={true} />
    </div>
  </div>
  
  <!-- Suchbereich [ZTS] -->
  <div class="search-section">
    <div class="search-input-group">
      <input
        type="text"
        placeholder="Patient suchen (Name, Versicherungsnummer)..."
        bind:value={searchTerm}
        on:input={handleSearch}
        class="search-input"
      />
      <button class="search-button" on:click={handleSearch}>
        🔍
      </button>
    </div>
    
    <div class="filter-controls">
      <select bind:value={itemsPerPage} on:change={applyFiltersAndSort}>
        <option value={10}>10 pro Seite</option>
        <option value={20}>20 pro Seite</option>
        <option value={50}>50 pro Seite</option>
      </select>
    </div>
  </div>
  
  <!-- Patientenliste [EIV][AIU][SF] -->
  <div class="table-container">
    {#if loading}
      <div class="loading-state">
        <Spinner />
        <p>Patientenliste wird geladen...</p>
      </div>
    {:else if error}
      <div class="error-state">
        <p class="error-message">⚠️ {error}</p>
        <button class="btn-secondary" on:click={loadPatients}>
          Erneut versuchen
        </button>
      </div>
    {:else if paginatedPatients.length === 0}
      <div class="empty-state">
        <p>Keine Patienten gefunden.</p>
        {#if searchTerm}
          <button class="btn-secondary" on:click={() => { searchTerm = ''; handleSearch(); }}>
            Filter zurücksetzen
          </button>
        {/if}
      </div>
    {:else}
      <table class="patient-table">
        <thead>
          <tr>
            <th>
              <button class="sort-button" on:click={() => handleSort('name')}>
                Name
                {#if sortBy === 'name'}
                  <span class="sort-indicator">{sortDirection === 'asc' ? '↑' : '↓'}</span>
                {/if}
              </button>
            </th>
            <th>
              <button class="sort-button" on:click={() => handleSort('dateOfBirth')}>
                Geburtsdatum [SF]
                {#if sortBy === 'dateOfBirth'}
                  <span class="sort-indicator">{sortDirection === 'asc' ? '↑' : '↓'}</span>
                {/if}
              </button>
            </th>
            <th>Versicherungsnummer</th>
            <th>
              <button class="sort-button" on:click={() => handleSort('sessionCount')}>
                Sessions
                {#if sortBy === 'sessionCount'}
                  <span class="sort-indicator">{sortDirection === 'asc' ? '↑' : '↓'}</span>
                {/if}
              </button>
            </th>
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
          {#each paginatedPatients as patient}
            <tr class="patient-row" on:click={() => openPatient(patient.id)}>
              <td class="patient-name">
                <strong>{patient.anonymizedLastName}</strong><br>
                <span class="first-name">{patient.anonymizedFirstName}</span>
              </td>
              <td>{patient.anonymizedDateOfBirth}</td>
              <td class="insurance-number">{patient.insuranceNumberHash}</td>
              <td class="session-count">
                <span class="badge">{patient.sessionCount}</span>
              </td>
              <td class="created-date">{patient.created}</td>
              <td class="modified-date">{patient.lastModified}</td>
              <td class="actions">
                <button class="btn-icon" title="Patient öffnen" on:click|stopPropagation={() => openPatient(patient.id)}>
                  👁️
                </button>
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
        ({filteredPatients.length} Patienten)
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

<!-- Patient Create Modal [ZTS] -->
{#if showCreateModal}
  <PatientCreateModal 
    on:close={() => showCreateModal = false}
    on:patientCreated={handlePatientCreated}
  />
{/if}

<style>
  .patient-list-view {
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
  
  .search-section {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
    gap: 16px;
  }
  
  .search-input-group {
    display: flex;
    flex: 1;
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
  
  .filter-controls select {
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;
  }
  
  .table-container {
    flex: 1;
    overflow: auto;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
  }
  
  .patient-table {
    width: 100%;
    border-collapse: collapse;
    background-color: #ffffff;
  }
  
  .patient-table th {
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
  
  .patient-table td {
    padding: 12px;
    border-bottom: 1px solid #f3f4f6;
  }
  
  .patient-row {
    cursor: pointer;
    transition: background-color 0.2s ease;
  }
  
  .patient-row:hover {
    background-color: #f9fafb;
  }
  
  .patient-name strong {
    color: #1f2937;
  }
  
  .first-name {
    color: #6b7280;
    font-size: 13px;
  }
  
  .insurance-number {
    font-family: 'Courier New', monospace;
    color: #6b7280;
  }
  
  .session-count .badge {
    background-color: #dbeafe;
    color: #1e40af;
    padding: 2px 8px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 500;
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
    .patient-list-view {
      padding: 8px;
    }
    
    .view-header {
      flex-direction: column;
      align-items: stretch;
      gap: 16px;
    }
    
    .search-section {
      flex-direction: column;
      align-items: stretch;
    }
    
    .patient-table {
      font-size: 13px;
    }
    
    .patient-table th,
    .patient-table td {
      padding: 8px;
    }
  }
</style>
