<!-- ÑDer Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein ñ ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [CT] Cloud-Transparenz - UI zeigt immer Datenverarbeitungsort
-->
<script lang="ts">
  import type { AppState, Session } from '$lib/types/app-state';
import { processingLocationIcon } from '$lib/stores/session';
</script>

<div class="processing-indicator" class:cloud={$processingLocationIcon.includes('Cloud')}>
  <span class="icon">{$processingLocationIcon.includes('Cloud') ? '‚òÅÔ∏è' : 'üîí'}</span>
  <span class="text">{$processingLocationIcon}</span>
</div>

<style>
  .processing-indicator {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    font-size: 0.875rem;
    font-weight: 500;
    background-color: #e5f2ff;
    color: #0050b3;
    border: 1px solid #b3d7ff;
  }
  
  .processing-indicator.cloud {
    background-color: #fff7e6;
    color: #d46b08;
    border: 1px solid #ffe7ba;
  }
  
  .icon {
    font-size: 1rem;
  }
</style>
