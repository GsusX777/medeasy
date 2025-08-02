<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  MedEasy Spinner-Komponente [PSF]
  Einfacher rotierender Kreis ohne Pfeile oder komplexe Animationen
  [SF] Schweizer Benutzerfreundlichkeit - Klare, ruhige Animationen
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
  <div class="spinner {getSize()} {color}" aria-label="Wird geladen..." role="status">
    <!-- Einfacher rotierender Kreis ohne Pfeile [PSF] -->
    <svg class="animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
      <!-- Hintergrund-Kreis (statisch) -->
      <circle 
        class="opacity-25" 
        cx="12" 
        cy="12" 
        r="10" 
        stroke="currentColor" 
        stroke-width="3"
      ></circle>
      <!-- Vordergrund-Kreis (rotierend) mit Lücke -->
      <circle 
        class="opacity-75" 
        cx="12" 
        cy="12" 
        r="10" 
        stroke="currentColor" 
        stroke-width="3" 
        stroke-linecap="round"
        stroke-dasharray="50 12"
        transform-origin="12 12"
      ></circle>
    </svg>
  </div>
  {#if text}
    <span class="ml-2 text-sm text-gray-600">{text}</span>
  {/if}
</div>

<style>
  .spinner {
    display: inline-block;
  }
  
  /* Sanfte, gleichmäßige Rotation ohne "Eiern" [PSF] */
  .animate-spin {
    animation: spin 1.5s linear infinite;
  }
  
  @keyframes spin {
    from {
      transform: rotate(0deg);
    }
    to {
      transform: rotate(360deg);
    }
  }
</style>
