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
  
  // Props
  export let title = 'MedEasy';
</script>

<div class="app-layout">
  <!-- [TSF][ZTS] Neuer Header mit Logo und Session-Info -->
  <Header {title} />
  
  <div class="app-content">
    <nav class="sidebar">
      <ul class="nav-menu">
        <li class="nav-item active">
          <a href="/" class="nav-link">
            <span class="nav-icon">🏠</span>
            <span class="nav-text">Dashboard</span>
          </a>
        </li>
        <li class="nav-item">
          <a href="/sessions" class="nav-link">
            <span class="nav-icon">🎙️</span>
            <span class="nav-text">Konsultationen</span>
          </a>
        </li>
        <li class="nav-item">
          <a href="/patients" class="nav-link">
            <span class="nav-icon">👥</span>
            <span class="nav-text">Patienten</span>
          </a>
        </li>
        <li class="nav-item">
          <a href="/reports" class="nav-link">
            <span class="nav-icon">📊</span>
            <span class="nav-text">Berichte</span>
          </a>
        </li>
        <li class="nav-item">
          <a href="/settings" class="nav-link">
            <span class="nav-icon">⚙️</span>
            <span class="nav-text">Einstellungen</span>
          </a>
        </li>
      </ul>
      
      <div class="sidebar-footer">
        <div class="app-version">v{$appState.version || '0.1.0'}</div>
      </div>
    </nav>
    
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
  }
  
  .sidebar {
    width: 240px;
    background-color: #1e293b;
    color: white;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    overflow-y: auto;
  }
  
  .nav-menu {
    list-style: none;
    padding: 1rem 0;
    margin: 0;
  }
  
  .nav-item {
    margin: 0.25rem 0;
  }
  
  .nav-link {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.75rem 1.5rem;
    color: #cbd5e1;
    text-decoration: none;
    transition: all 0.2s;
  }
  
  .nav-link:hover {
    background-color: rgba(255, 255, 255, 0.1);
    color: white;
  }
  
  .nav-item.active .nav-link {
    background-color: #2563eb;
    color: white;
    font-weight: 500;
  }
  
  .nav-icon {
    font-size: 1.125rem;
    width: 1.5rem;
    text-align: center;
  }
  
  .sidebar-footer {
    padding: 1rem 1.5rem;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .app-version {
    font-size: 0.75rem;
    color: #64748b;
  }
  
  .main-content {
    flex: 1;
    padding: 1.5rem;
    overflow-y: auto;
  }
</style>
