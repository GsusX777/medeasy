<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  MedEasy Spinner-Komponente
  Zeigt einen Ladeindikator an
-->
<script lang="ts">
  // Größe des Spinners: sm, md, lg
  export let size: 'sm' | 'md' | 'lg' = 'md';
  
  // Text, der neben dem Spinner angezeigt wird
  export let text: string = '';
  
  // Farbe des Spinners
  export let color: string = 'text-blue-600';
  
  // Bestimmt die Größe basierend auf dem size-Parameter
  function getSize(): string {
    switch (size) {
      case 'sm': return 'w-4 h-4';
      case 'lg': return 'w-8 h-8';
      default: return 'w-6 h-6';
    }
  }
</script>

<div class="flex items-center">
  <div class="spinner {getSize()} {color}" aria-label="Wird geladen...">
    <svg class="animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
    </svg>
  </div>
  {#if text}
    <span class="ml-2">{text}</span>
  {/if}
</div>

<style>
  .spinner {
    display: inline-block;
  }
</style>
