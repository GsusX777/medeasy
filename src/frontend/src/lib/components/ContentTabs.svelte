<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [TSF] Technologie-Stack Fest - Svelte 4 + TypeScript
  [ZTS] Zero Tolerance Security - Tab-State sicher verwalten
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Alle Tabs respektieren Anonymisierung
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import TranscriptSplitView from './TranscriptSplitView.svelte';
  import PatientListView from './PatientListView.svelte';
  import ConsultationListView from './ConsultationListView.svelte';
  
  const dispatch = createEventDispatcher();
  
  // Tab-State Management [ZTS]
  let activeTab: 'transcript' | 'patients' | 'consultations' = 'transcript';
  
  interface Tab {
    id: 'transcript' | 'patients' | 'consultations';
    label: string;
    icon: string;
    component: any;
  }
  
  const tabs: Tab[] = [
    {
      id: 'transcript',
      label: 'Transkription',
      icon: '📝',
      component: TranscriptSplitView
    },
    {
      id: 'patients',
      label: 'Patienten',
      icon: '👥',
      component: PatientListView
    },
    {
      id: 'consultations',
      label: 'Konsultationen',
      icon: '📋',
      component: ConsultationListView
    }
  ];
  
  function switchTab(tabId: 'transcript' | 'patients' | 'consultations') {
    activeTab = tabId;
    dispatch('tabChanged', { activeTab: tabId });
  }
  
  // Keyboard Navigation [PSF]
  function handleKeydown(event: KeyboardEvent) {
    if (event.ctrlKey) {
      switch (event.key) {
        case '1':
          event.preventDefault();
          switchTab('transcript');
          break;
        case '2':
          event.preventDefault();
          switchTab('patients');
          break;
        case '3':
          event.preventDefault();
          switchTab('consultations');
          break;
      }
    }
  }
</script>

<svelte:window on:keydown={handleKeydown} />

<div class="content-tabs">
  <!-- Tab Navigation [TSF] -->
  <div class="tab-navigation">
    {#each tabs as tab}
      <button
        class="tab-button"
        class:active={activeTab === tab.id}
        on:click={() => switchTab(tab.id)}
        title="Tastenkürzel: Ctrl+{tabs.indexOf(tab) + 1}"
      >
        <span class="tab-icon">{tab.icon}</span>
        <span class="tab-label">{tab.label}</span>
      </button>
    {/each}
  </div>
  
  <!-- Tab Content [AIU][ZTS] -->
  <div class="tab-content">
    {#if activeTab === 'transcript'}
      <TranscriptSplitView />
    {:else if activeTab === 'patients'}
      <PatientListView />
    {:else if activeTab === 'consultations'}
      <ConsultationListView />
    {/if}
  </div>
</div>

<style>
  .content-tabs {
    display: flex;
    flex-direction: column;
    height: 100%;
    background-color: #ffffff;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }
  
  .tab-navigation {
    display: flex;
    border-bottom: 1px solid #e5e7eb;
    background-color: #f9fafb;
    border-radius: 8px 8px 0 0;
    padding: 0 16px;
  }
  
  .tab-button {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 16px;
    border: none;
    background: none;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    color: #6b7280;
    border-bottom: 2px solid transparent;
    transition: all 0.2s ease;
    position: relative;
  }
  
  .tab-button:hover {
    color: #374151;
    background-color: #f3f4f6;
  }
  
  .tab-button.active {
    color: #1f2937;
    border-bottom-color: #3b82f6;
    background-color: #ffffff;
  }
  
  .tab-button.active::after {
    content: '';
    position: absolute;
    bottom: -1px;
    left: 0;
    right: 0;
    height: 1px;
    background-color: #ffffff;
  }
  
  .tab-icon {
    font-size: 16px;
  }
  
  .tab-label {
    font-weight: 500;
  }
  
  .tab-content {
    flex: 1;
    overflow: hidden;
    padding: 0;
  }
  
  /* Responsive Design [PSF] */
  @media (max-width: 768px) {
    .tab-navigation {
      padding: 0 8px;
    }
    
    .tab-button {
      padding: 8px 12px;
      font-size: 13px;
    }
    
    .tab-label {
      display: none;
    }
    
    .tab-icon {
      font-size: 18px;
    }
  }
  
  /* Accessibility [PSF] */
  .tab-button:focus {
    outline: 2px solid #3b82f6;
    outline-offset: -2px;
  }
  
  .tab-button:focus:not(:focus-visible) {
    outline: none;
  }
</style>
