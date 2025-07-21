<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<script lang="ts">
  import { onMount } from 'svelte';
  import AppLayout from '$lib/components/AppLayout.svelte';
  import { loadPatients, loadPatientSessions, patients, sessions, initDb } from '$lib/stores/database';
  import type { PatientDto, SessionDto } from '$lib/types/database';
  
  // State
  let todaySessions: SessionDto[] = [];
  let recentSessions: SessionDto[] = [];
  let openSessions: SessionDto[] = [];
  let allPatients: PatientDto[] = [];
  let allSessions: SessionDto[] = [];
  let loading = true;
  let error = '';
  
  // Aktuelles Datum [SF] - Schweizer Format DD.MM.YYYY
  const today = new Date();
  const todayString = today.toLocaleDateString('de-CH', {
    day: '2-digit',
    month: '2-digit', 
    year: 'numeric'
  });
  
  // Wochentag auf Deutsch [SF]
  const weekday = today.toLocaleDateString('de-CH', { weekday: 'long' });
  
  onMount(async () => {
    await loadDashboardData();
  });
  
  async function loadDashboardData() {
    try {
      loading = true;
      error = '';
      
      // Initialisiere Datenbank falls nötig
      const dbInitialized = await initDb();
      if (!dbInitialized) {
        error = 'Fehler beim Initialisieren der Datenbank';
        return;
      }
      
      // Lade alle Patienten
      allPatients = await loadPatients();
      
      // Lade Sessions für alle Patienten
      const sessionPromises = allPatients.map(patient => loadPatientSessions(patient.id));
      const sessionArrays = await Promise.all(sessionPromises);
      allSessions = sessionArrays.flat();
      
      // Filtere Sessions für heute
      todaySessions = allSessions.filter(session => {
        // session.session_date ist bereits im DD.MM.YYYY Format [SF]
        return session.session_date === todayString;
      });
      
      // Letzte 5 abgeschlossene Konsultationen
      recentSessions = allSessions
        .filter(session => session.status === 'Completed')
        .sort((a, b) => {
          // Konvertiere DD.MM.YYYY zu Date für Sortierung
          const dateA = new Date(a.session_date.split('.').reverse().join('-'));
          const dateB = new Date(b.session_date.split('.').reverse().join('-'));
          return dateB.getTime() - dateA.getTime();
        })
        .slice(0, 5);
      
      // Offene Konsultationen (geplant oder laufend)
      openSessions = allSessions
        .filter(session => session.status === 'Scheduled' || session.status === 'InProgress')
        .sort((a, b) => {
          // Konvertiere DD.MM.YYYY zu Date für Sortierung
          const dateA = new Date(a.session_date.split('.').reverse().join('-'));
          const dateB = new Date(b.session_date.split('.').reverse().join('-'));
          return dateA.getTime() - dateB.getTime();
        });
      
    } catch (err) {
      error = `Fehler beim Laden des Dashboards: ${err}`;
      console.error(error);
    } finally {
      loading = false;
    }
  }
  
  // Hilfsfunktion: Patient-Name finden
  function getPatientName(patientId: string): string {
    const patient = allPatients.find((p: PatientDto) => p.id === patientId);
    return patient ? `${patient.first_name} ${patient.last_name}` : 'Unbekannter Patient';
  }
  
  // Hilfsfunktion: Status-Icon
  function getStatusIcon(status: string): string {
    switch (status) {
      case 'Scheduled': return '📅';
      case 'InProgress': return '🔴';
      case 'Completed': return '✅';
      case 'Cancelled': return '❌';
      default: return '❓';
    }
  }
  
  // Hilfsfunktion: Status-Text [SF]
  function getStatusText(status: string): string {
    switch (status) {
      case 'Scheduled': return 'Geplant';
      case 'InProgress': return 'Laufend';
      case 'Completed': return 'Abgeschlossen';
      case 'Cancelled': return 'Abgebrochen';
      default: return 'Unbekannt';
    }
  }
</script>

<AppLayout title="Dashboard">
  <div class="dashboard">
    <div class="dashboard-header">
      <h1>Übersicht</h1>
      <div class="date-info">
        <span class="weekday">{weekday}</span>
        <span class="date">{todayString}</span>
      </div>
    </div>
    
    {#if loading}
      <div class="loading">
        <div class="spinner"></div>
        <p>Lade Dashboard-Daten...</p>
      </div>
    {:else if error}
      <div class="error">
        <p>⚠️ {error}</p>
        <button on:click={loadDashboardData} class="retry-button">Erneut versuchen</button>
      </div>
    {:else}
      <div class="dashboard-grid">
        
        <!-- Tagesübersicht -->
        <div class="dashboard-card today-overview">
          <h2>📅 Heute ({todayString})</h2>
          {#if todaySessions.length === 0}
            <div class="empty-state">
              <p>Keine Konsultationen für heute geplant</p>
            </div>
          {:else}
            <div class="session-list">
              {#each todaySessions as session}
                <div class="session-item today">
                  <div class="session-info">
                    <div class="session-patient">
                      <span class="patient-icon">👤</span>
                      <span class="patient-name">{getPatientName(session.patient_id)}</span>
                    </div>
                    <div class="session-time">
                      {session.start_time} - {session.end_time || 'offen'}
                    </div>
                  </div>
                  <div class="session-status">
                    <span class="status-icon">{getStatusIcon(session.status)}</span>
                    <span class="status-text">{getStatusText(session.status)}</span>
                  </div>
                </div>
              {/each}
            </div>
          {/if}
        </div>
        
        <!-- Offene Konsultationen -->
        <div class="dashboard-card open-sessions">
          <h2>📋 Offene Konsultationen</h2>
          {#if openSessions.length === 0}
            <div class="empty-state">
              <p>Keine offenen Konsultationen</p>
            </div>
          {:else}
            <div class="session-list">
              {#each openSessions as session}
                <div class="session-item open">
                  <div class="session-info">
                    <div class="session-patient">
                      <span class="patient-icon">👤</span>
                      <span class="patient-name">{getPatientName(session.patient_id)}</span>
                    </div>
                    <div class="session-date-time">
                      <span class="session-date">{session.session_date}</span>
                      <span class="session-time">{session.start_time}</span>
                    </div>
                  </div>
                  <div class="session-status">
                    <span class="status-icon">{getStatusIcon(session.status)}</span>
                    <span class="status-text">{getStatusText(session.status)}</span>
                  </div>
                </div>
              {/each}
            </div>
          {/if}
        </div>
        
        <!-- Letzte Konsultationen -->
        <div class="dashboard-card recent-sessions">
          <h2>📝 Letzte Konsultationen</h2>
          {#if recentSessions.length === 0}
            <div class="empty-state">
              <p>Noch keine abgeschlossenen Konsultationen</p>
            </div>
          {:else}
            <div class="session-list">
              {#each recentSessions as session}
                <div class="session-item recent">
                  <div class="session-info">
                    <div class="session-patient">
                      <span class="patient-icon">👤</span>
                      <span class="patient-name">{getPatientName(session.patient_id)}</span>
                    </div>
                    <div class="session-date-time">
                      <span class="session-date">{session.session_date}</span>
                      <span class="session-time">{session.start_time} - {session.end_time}</span>
                    </div>
                  </div>
                  <div class="session-status completed">
                    <span class="status-icon">✅</span>
                    <span class="status-text">Abgeschlossen</span>
                  </div>
                </div>
              {/each}
            </div>
          {/if}
        </div>
        
      </div>
    {/if}
  </div>
</AppLayout>

<style>
  .dashboard {
    max-width: 1200px;
    margin: 0 auto;
    padding: 1.5rem;
  }
  
  .dashboard-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 2px solid #e5e7eb;
  }
  
  .dashboard-header h1 {
    font-size: 2rem;
    font-weight: 700;
    color: #1f2937;
    margin: 0;
  }
  
  .date-info {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.25rem;
  }
  
  .weekday {
    font-size: 1.125rem;
    font-weight: 600;
    color: #3b82f6;
    text-transform: capitalize;
  }
  
  .date {
    font-size: 0.875rem;
    color: #6b7280;
    font-family: 'JetBrains Mono', monospace;
  }
  
  .loading, .error {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem;
    text-align: center;
  }
  
  .spinner {
    width: 2rem;
    height: 2rem;
    border: 3px solid #e5e7eb;
    border-top: 3px solid #3b82f6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 1rem;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  .error {
    color: #dc2626;
  }
  
  .retry-button {
    margin-top: 1rem;
    padding: 0.5rem 1rem;
    background: #3b82f6;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    transition: background-color 0.2s;
  }
  
  .retry-button:hover {
    background: #2563eb;
  }
  
  .dashboard-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    grid-template-rows: auto auto;
    gap: 1.5rem;
  }
  
  .dashboard-card {
    background: white;
    border-radius: 12px;
    padding: 1.5rem;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    border: 1px solid #e5e7eb;
  }
  
  .dashboard-card h2 {
    font-size: 1.25rem;
    font-weight: 600;
    color: #1f2937;
    margin: 0 0 1rem 0;
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .today-overview {
    grid-column: 1 / -1; /* Vollbreite */
    border-left: 4px solid #3b82f6;
  }
  
  .open-sessions {
    border-left: 4px solid #f59e0b;
  }
  
  .recent-sessions {
    border-left: 4px solid #10b981;
  }
  
  .empty-state {
    text-align: center;
    padding: 2rem;
    color: #6b7280;
    font-style: italic;
  }
  
  .session-list {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .session-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem;
    border-radius: 8px;
    border: 1px solid #e5e7eb;
    transition: all 0.2s;
  }
  
  .session-item:hover {
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    transform: translateY(-1px);
  }
  
  .session-item.today {
    background: linear-gradient(135deg, #dbeafe 0%, #f0f9ff 100%);
    border-color: #3b82f6;
  }
  
  .session-item.open {
    background: linear-gradient(135deg, #fef3c7 0%, #fffbeb 100%);
    border-color: #f59e0b;
  }
  
  .session-item.recent {
    background: linear-gradient(135deg, #d1fae5 0%, #f0fdf4 100%);
    border-color: #10b981;
  }
  
  .session-info {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }
  
  .session-patient {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .patient-icon {
    font-size: 1rem;
  }
  
  .patient-name {
    font-weight: 600;
    color: #1f2937;
  }
  
  .session-date-time {
    display: flex;
    gap: 0.75rem;
    font-size: 0.875rem;
    color: #6b7280;
  }
  
  .session-time {
    font-family: 'JetBrains Mono', monospace;
    font-size: 0.875rem;
    color: #6b7280;
  }
  
  .session-date {
    font-family: 'JetBrains Mono', monospace;
  }
  
  .session-status {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.875rem;
    font-weight: 500;
    background: rgba(255, 255, 255, 0.8);
    border: 1px solid rgba(0, 0, 0, 0.1);
  }
  
  .status-icon {
    font-size: 1rem;
  }
  
  .status-text {
    color: #374151;
  }
  
  /* Responsive Design */
  @media (max-width: 768px) {
    .dashboard-grid {
      grid-template-columns: 1fr;
    }
    
    .dashboard-header {
      flex-direction: column;
      align-items: flex-start;
      gap: 1rem;
    }
    
    .date-info {
      align-items: flex-start;
    }
    
    .session-item {
      flex-direction: column;
      align-items: flex-start;
      gap: 0.75rem;
    }
    
    .session-status {
      align-self: flex-end;
    }
  }
</style>
