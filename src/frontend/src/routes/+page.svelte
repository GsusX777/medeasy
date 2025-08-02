<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [TSF] Technologie-Stack Fest - Svelte 4 + TypeScript
  [ZTS] Zero Tolerance Security - Sichere Initialisierung
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Alle Tabs respektieren Anonymisierung
  [SP] SQLCipher Pflicht - Datenbank wird verschlüsselt initialisiert
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import AppLayout from '$lib/components/AppLayout.svelte';
  import ContentTabs from '$lib/components/ContentTabs.svelte';
  import Spinner from '$lib/components/common/Spinner.svelte';
  import { initDb } from '$lib/stores/database';
  
  // State Management [ZTS]
  let loading = true;
  let error = '';
  let activeTab = 'transcript';
  
  onMount(async () => {
    await initializeApp();
  });
  
  async function initializeApp() {
    try {
      loading = true;
      error = '';
      
      // Initialisiere Datenbank falls nötig [SP][ZTS]
      const dbInitialized = await initDb();
      if (!dbInitialized) {
        error = 'Fehler beim Initialisieren der Datenbank';
        return;
      }
      
      console.log('MedEasy erfolgreich initialisiert [AIU][ATV][SP]');
      
    } catch (err) {
      console.error('Initialisierungsfehler:', err);
      error = 'Fehler beim Starten der Anwendung';
    } finally {
      loading = false;
    }
  }
  
  function handleTabChanged(event: CustomEvent) {
    activeTab = event.detail.activeTab;
    console.log('Tab gewechselt zu:', activeTab);
  }
</script>

<AppLayout title="MedEasy">
  <div class="main-container">
    {#if loading}
      <div class="loading-state">
        <!-- Einfacher inline Spinner zum Debuggen [PSF] -->
        <div class="inline-spinner">
          <svg class="animate-spin" width="48" height="48" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <!-- Hintergrund-Kreis (schwach) -->
            <circle cx="12" cy="12" r="10" stroke="#e5e7eb" stroke-width="3" fill="none"></circle>
            <!-- Rotierender Kreis (nur 3/4 sichtbar) -->
            <circle cx="12" cy="12" r="10" stroke="#3b82f6" stroke-width="3" fill="none" stroke-linecap="round" stroke-dasharray="47" stroke-dashoffset="12" transform-origin="12 12"></circle>
          </svg>
        </div>
        <p>MedEasy wird gestartet...</p>
        <div class="loading-details">
          <span class="loading-step">🔒 Datenbank wird initialisiert...</span>
          <span class="loading-step">🛡️ Sicherheitsfeatures werden geladen...</span>
          <span class="loading-step">📝 Anonymisierung wird aktiviert...</span>
        </div>
      </div>
    {:else if error}
      <div class="error-state">
        <div class="error-icon">⚠️</div>
        <h3>Fehler beim Starten</h3>
        <p class="error-message">{error}</p>
        <button class="btn-retry" on:click={initializeApp}>
          <span class="icon">🔄</span>
          Erneut versuchen
        </button>
      </div>
    {:else}
      <!-- Hauptbereich mit 3 Tabs [TSF][ZTS][AIU] -->
      <div class="content-wrapper">
        <ContentTabs on:tabChanged={handleTabChanged} />
      </div>
    {/if}
  </div>
</AppLayout>

<style>
  .main-container {
    display: flex;
    flex-direction: column;
    height: 100%;
    background-color: #f9fafb;
  }

  .loading-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px;
    text-align: center;
    height: 100%;
    gap: 24px;
  }

  /* Inline Spinner für Debugging [PSF] */
  .inline-spinner {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-bottom: 16px;
  }
  
  .animate-spin {
    animation: spin 1.5s linear infinite;
  }
  
  @keyframes spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
  }

  .loading-state p {
    color: #374151;
    font-size: 18px;
    font-weight: 500;
    margin: 0;
  }

  .loading-details {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-top: 16px;
  }

  .loading-step {
    color: #6b7280;
    font-size: 14px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
  }

  .error-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px;
    text-align: center;
    height: 100%;
    gap: 16px;
  }

  .error-icon {
    font-size: 64px;
    color: #dc2626;
  }

  .error-state h3 {
    color: #1f2937;
    font-size: 24px;
    font-weight: 600;
    margin: 0;
  }

  .error-message {
    color: #dc2626;
    font-size: 16px;
    margin: 0;
    max-width: 400px;
  }

  .btn-retry {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 24px;
    background-color: #3b82f6;
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 500;
    transition: all 0.2s ease;
    box-shadow: 0 2px 4px rgba(59, 130, 246, 0.2);
  }

  .btn-retry:hover {
    background-color: #2563eb;
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(59, 130, 246, 0.3);
  }

  .btn-retry:active {
    transform: translateY(0);
  }

  .content-wrapper {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  .icon {
    font-size: 16px;
  }

  /* Responsive Design [PSF] */
  @media (max-width: 768px) {
    .loading-state, .error-state {
      padding: 24px 16px;
    }

    .loading-state p {
      font-size: 16px;
    }

    .error-state h3 {
      font-size: 20px;
    }

    .error-message {
      font-size: 14px;
    }

    .btn-retry {
      padding: 10px 20px;
      font-size: 14px;
    }

    .spinner {
      font-size: 36px;
    }

    .error-icon {
      font-size: 48px;
    }
  }

  /* Accessibility [PSF] */
  .btn-retry:focus {
    outline: 2px solid #3b82f6;
    outline-offset: 2px;
  }

  .btn-retry:focus:not(:focus-visible) {
    outline: none;
  }

  /* Loading Animation Enhancement */
  .loading-step {
    opacity: 0;
    animation: fadeInUp 0.6s ease forwards;
  }

  .loading-step:nth-child(1) {
    animation-delay: 0.2s;
  }

  .loading-step:nth-child(2) {
    animation-delay: 0.4s;
  }

  .loading-step:nth-child(3) {
    animation-delay: 0.6s;
  }

  @keyframes fadeInUp {
    from {
      opacity: 0;
      transform: translateY(10px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  /* High Contrast Mode Support [PSF] */
  @media (prefers-contrast: high) {
    .loading-state p {
      color: #000000;
    }

    .loading-step {
      color: #333333;
    }

    .error-message {
      color: #cc0000;
    }
  }

  /* Reduced Motion Support [PSF] */
  @media (prefers-reduced-motion: reduce) {
    .spinner {
      animation: none;
    }

    .loading-step {
      animation: none;
      opacity: 1;
    }

    .btn-retry:hover {
      transform: none;
    }
  }
</style>
