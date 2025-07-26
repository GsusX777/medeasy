<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [ZTS] Zero Tolerance Security - Einstellungen-Modal für medizinische Software
  [PSF] Patient Safety First - Keine Ablenkung vom Hauptworkflow
  [SF] Schweizer Formate - Deutsche Sprache
  [UX] User Experience - Modal-Design für Einstellungen
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import AudioSettings from './AudioSettings.svelte';
  import SecuritySettings from './SecuritySettings.svelte';
  import DatabaseSecuritySettings from './DatabaseSecuritySettings.svelte';
  
  // Props
  export let show = false;
  
  // Event Dispatcher
  const dispatch = createEventDispatcher();
  
  // Modal State [UX]
  let activeTab: 'audio' | 'security' | 'database' = 'audio';
  let modalElement: HTMLElement;
  
  // Tab Management [UX]
  function setActiveTab(tab: typeof activeTab) {
    activeTab = tab;
  }
  
  // Close Modal [UX]
  function closeModal() {
    show = false;
    dispatch('close');
  }
  
  // Handle Escape Key [UX]
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      closeModal();
    }
  }
  
  // Handle Backdrop Click [UX]
  function handleBackdropClick(event: MouseEvent) {
    if (event.target === modalElement) {
      closeModal();
    }
  }
  
  // Tab Configuration [SF]
  const tabs = [
    {
      id: 'audio' as const,
      label: '🎤 Audio',
      description: 'Mikrofon, Qualität, Schweizerdeutsch'
    },
    {
      id: 'security' as const,
      label: '🔒 Sicherheit',
      description: 'Verschlüsselung, Anonymisierung'
    },
    {
      id: 'database' as const,
      label: '🗄️ Datenbank',
      description: 'SQLCipher, Backup, Audit'
    }
  ];
</script>

<!-- Modal Backdrop -->
{#if show}
  <!-- svelte-ignore a11y-no-static-element-interactions -->
  <div 
    class="modal-backdrop" 
    bind:this={modalElement}
    on:click={handleBackdropClick}
    on:keydown={handleKeydown}
    tabindex="-1"
  >
    <!-- Modal Container -->
    <div class="modal-container" role="dialog" aria-labelledby="settings-title" aria-modal="true">
      <!-- Modal Header [SF] -->
      <div class="modal-header">
        <div class="modal-title-section">
          <h2 id="settings-title">⚙️ Einstellungen</h2>
          <p class="modal-subtitle">MedEasy Konfiguration</p>
        </div>
        
        <button 
          class="modal-close" 
          on:click={closeModal}
          title="Einstellungen schließen"
          aria-label="Einstellungen schließen"
        >
          ✕
        </button>
      </div>
      
      <!-- Tab Navigation [UX] -->
      <div class="tab-navigation">
        {#each tabs as tab}
          <button
            class="tab-button {activeTab === tab.id ? 'active' : ''}"
            on:click={() => setActiveTab(tab.id)}
            title={tab.description}
          >
            <span class="tab-label">{tab.label}</span>
            <span class="tab-description">{tab.description}</span>
          </button>
        {/each}
      </div>
      
      <!-- Modal Content [PSF] -->
      <div class="modal-content">
        {#if activeTab === 'audio'}
          <div class="tab-panel" role="tabpanel" aria-labelledby="audio-tab">
            <AudioSettings />
          </div>
        {:else if activeTab === 'security'}
          <div class="tab-panel" role="tabpanel" aria-labelledby="security-tab">
            <SecuritySettings />
          </div>
        {:else if activeTab === 'database'}
          <div class="tab-panel" role="tabpanel" aria-labelledby="database-tab">
            <DatabaseSecuritySettings />
          </div>
        {/if}
      </div>
      
      <!-- Modal Footer [UX] -->
      <div class="modal-footer">
        <div class="footer-info">
          <span class="footer-text">
            💡 Änderungen werden automatisch gespeichert
          </span>
        </div>
        
        <div class="footer-actions">
          <button class="btn-secondary" on:click={closeModal}>
            Schließen
          </button>
        </div>
      </div>
    </div>
  </div>
{/if}

<style>
  /* Modal Backdrop [UX] */
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.6);
    backdrop-filter: blur(4px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    padding: 20px;
    animation: fadeIn 0.2s ease-out;
  }
  
  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }
  
  /* Modal Container [PSF] */
  .modal-container {
    background: white;
    border-radius: 16px;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
    max-width: 1000px;
    max-height: 90vh;
    width: 100%;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    animation: slideIn 0.3s ease-out;
  }
  
  @keyframes slideIn {
    from { 
      opacity: 0;
      transform: translateY(-20px) scale(0.95);
    }
    to { 
      opacity: 1;
      transform: translateY(0) scale(1);
    }
  }
  
  /* Modal Header [SF] */
  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 24px 32px 20px 32px;
    border-bottom: 1px solid #E5E7EB;
    background: linear-gradient(135deg, #F8FAFC 0%, #F1F5F9 100%);
  }
  
  .modal-title-section h2 {
    margin: 0;
    font-size: 1.75rem;
    font-weight: 600;
    color: #1F2937;
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .modal-subtitle {
    margin: 4px 0 0 0;
    font-size: 0.9rem;
    color: #6B7280;
    font-weight: 400;
  }
  
  .modal-close {
    width: 40px;
    height: 40px;
    border: none;
    background: #F3F4F6;
    border-radius: 8px;
    color: #6B7280;
    font-size: 1.2rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s ease;
  }
  
  .modal-close:hover {
    background: #E5E7EB;
    color: #374151;
  }
  
  /* Tab Navigation [UX] */
  .tab-navigation {
    display: flex;
    background: #F9FAFB;
    border-bottom: 1px solid #E5E7EB;
    padding: 0 32px;
  }
  
  .tab-button {
    flex: 1;
    padding: 16px 20px;
    border: none;
    background: transparent;
    cursor: pointer;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    transition: all 0.2s ease;
    border-bottom: 3px solid transparent;
    position: relative;
  }
  
  .tab-button:hover {
    background: rgba(59, 130, 246, 0.05);
  }
  
  .tab-button.active {
    background: white;
    border-bottom-color: #3B82F6;
  }
  
  .tab-label {
    font-size: 1rem;
    font-weight: 500;
    color: #374151;
  }
  
  .tab-button.active .tab-label {
    color: #3B82F6;
    font-weight: 600;
  }
  
  .tab-description {
    font-size: 0.75rem;
    color: #6B7280;
    text-align: center;
  }
  
  .tab-button.active .tab-description {
    color: #3B82F6;
  }
  
  /* Modal Content [PSF] */
  .modal-content {
    flex: 1;
    overflow-y: auto;
    padding: 0;
    min-height: 400px;
    max-height: calc(90vh - 200px);
  }
  
  .tab-panel {
    padding: 32px;
    animation: fadeInContent 0.3s ease-out;
  }
  
  @keyframes fadeInContent {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
  }
  
  /* Modal Footer [UX] */
  .modal-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 20px 32px;
    border-top: 1px solid #E5E7EB;
    background: #F9FAFB;
  }
  
  .footer-info {
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .footer-text {
    font-size: 0.85rem;
    color: #6B7280;
  }
  
  .footer-actions {
    display: flex;
    gap: 12px;
  }
  
  .btn-secondary {
    padding: 10px 20px;
    border: 1px solid #D1D5DB;
    background: white;
    color: #374151;
    border-radius: 8px;
    font-size: 0.9rem;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  
  .btn-secondary:hover {
    background: #F3F4F6;
    border-color: #9CA3AF;
  }
  
  /* Responsive Design [UX] */
  @media (max-width: 768px) {
    .modal-backdrop {
      padding: 10px;
    }
    
    .modal-container {
      max-height: 95vh;
      border-radius: 12px;
    }
    
    .modal-header {
      padding: 20px 24px 16px 24px;
    }
    
    .modal-title-section h2 {
      font-size: 1.5rem;
    }
    
    .tab-navigation {
      padding: 0 24px;
    }
    
    .tab-button {
      padding: 12px 16px;
    }
    
    .tab-label {
      font-size: 0.9rem;
    }
    
    .tab-description {
      font-size: 0.7rem;
    }
    
    .tab-panel {
      padding: 24px;
    }
    
    .modal-footer {
      padding: 16px 24px;
      flex-direction: column;
      gap: 12px;
      align-items: stretch;
    }
    
    .footer-actions {
      justify-content: center;
    }
  }
  
  /* Accessibility [PSF] */
  .modal-backdrop:focus {
    outline: none;
  }
  
  .tab-button:focus {
    outline: 2px solid #3B82F6;
    outline-offset: -2px;
  }
  
  .modal-close:focus {
    outline: 2px solid #3B82F6;
    outline-offset: 2px;
  }
</style>
