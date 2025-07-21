<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
  [SF] Schweizer Formate - Datum: DD.MM.YYYY
  [MFD] Medizinische Fachbegriffe DE-CH
-->
<script lang="ts">
  import type { AppState, Session } from '$lib/types/app-state';
  import { appState } from '$lib/stores/session';
  import Header from './Header.svelte';
  import SubHeader from './SubHeader.svelte';
  import Sidebar from './Sidebar.svelte';
  
  // Props
  export let title = 'MedEasy';
</script>

<div class="app-layout">
  <!-- [TSF][ZTS] Hauptheader mit Logo, Datum/Zeit, Killswitch -->
  <Header {title} />
  
  <!-- [TSF][ZTS] SubHeader full-width aber Content nach Sidebar-Breite [SF] -->
  <div class="subheader-wrapper">
    <SubHeader />
  </div>
  
  <div class="app-content">
    <!-- [TSF][ZTS] Neue Sidebar-Komponente -->
    <Sidebar />
    
    <main class="main-content">
      <slot></slot>
    </main>
  </div>
</div>

<style>
  .app-layout {
    display: flex;
    flex-direction: column;
    height: 100vh;
    background-color: #f9fafb;
  }
  
  /* SubHeader wrapper für full-width Layout [SF] */
  .subheader-wrapper {
    width: 100%;
    position: relative;
    z-index: 10;
  }
  
  .app-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 1.5rem;
    background-color: white;
    border-bottom: 1px solid #e5e7eb;
    height: 64px;
  }
  
  .header-left, .header-right {
    display: flex;
    align-items: center;
    gap: 1.5rem;
  }
  
  .logo {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .logo-icon {
    font-size: 1.5rem;
  }
  
  h1 {
    margin: 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: #2563eb;
  }
  
  .date {
    font-size: 0.875rem;
    color: #64748b;
    padding-left: 1rem;
    border-left: 1px solid #e5e7eb;
  }
  
  .user-menu {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem;
    border-radius: 6px;
    cursor: pointer;
    transition: background-color 0.2s;
  }
  
  .user-menu:hover {
    background-color: #f1f5f9;
  }
  
  .user-icon {
    font-size: 1.25rem;
  }
  
  .user-name {
    font-size: 0.875rem;
    font-weight: 500;
    color: #334155;
  }
  
  .app-content {
    display: flex;
    flex: 1;
    overflow: hidden;
    position: relative;
  }
  

  
  .main-content {
    flex: 1;
    overflow-y: auto;
    padding: 1.5rem;
    margin-left: 0; /* Sidebar ist bereits positioned */
  }
</style>
