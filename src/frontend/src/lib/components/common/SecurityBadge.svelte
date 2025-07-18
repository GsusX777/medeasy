<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  MedEasy SecurityBadge-Komponente [ZTS][CT]
  Zeigt den Sicherheitsstatus einer Funktion an
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  // Gibt an, ob die Funktion gesichert ist
  export let secured: boolean = true;
  
  // Tooltip-Text
  export let tooltip: string = '';
  
  // Größe des Badges
  export let size: 'sm' | 'md' | 'lg' = 'md';
  
  // Event-Dispatcher
  const dispatch = createEventDispatcher();
  
  // Bestimmt die Größe basierend auf dem size-Parameter
  function getSize(): string {
    switch (size) {
      case 'sm': return 'text-xs px-1.5 py-0.5';
      case 'lg': return 'text-base px-3 py-1';
      default: return 'text-sm px-2 py-0.5';
    }
  }
  
  // Bestimmt die Farbe basierend auf dem secured-Parameter
  function getColor(): string {
    return secured 
      ? 'bg-green-100 text-green-800 border-green-300' 
      : 'bg-red-100 text-red-800 border-red-300';
  }
  
  // Bestimmt das Icon basierend auf dem secured-Parameter
  function getIcon(): string {
    return secured 
      ? '🔒' 
      : '⚠️';
  }
  
  // Bestimmt den Text basierend auf dem secured-Parameter
  function getText(): string {
    return secured 
      ? 'Gesichert' 
      : 'Ungesichert';
  }
  
  // Zeigt Details an
  function showDetails() {
    dispatch('click');
  }
</script>

<div 
  class="security-badge inline-flex items-center border rounded {getSize()} {getColor()}"
  on:click={showDetails}
  on:keydown={(e) => e.key === 'Enter' && showDetails()}
  title={tooltip}
  role="button"
  tabindex="0"
>
  <span class="mr-1">{getIcon()}</span>
  <span>{getText()}</span>
</div>

<style>
  .security-badge {
    cursor: pointer;
    transition: all 0.2s;
  }
  
  .security-badge:hover {
    opacity: 0.9;
  }
</style>
