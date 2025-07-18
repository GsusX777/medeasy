<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  MedEasy ConfirmDialog-Komponente
  Zeigt einen Bestätigungsdialog an
-->
<script lang="ts">
  // Dialog-Eigenschaften
  export let show: boolean = false;
  export let title: string = 'Bestätigen';
  export let message: string = 'Sind Sie sicher?';
  export let confirmText: string = 'Bestätigen';
  export let cancelText: string = 'Abbrechen';
  export let confirmButtonClass: string = 'bg-blue-600 hover:bg-blue-700 text-white';
  export let cancelButtonClass: string = 'bg-gray-200 hover:bg-gray-300 text-gray-800';
  export let dangerous: boolean = false;
  
  // Callback-Funktionen
  export let onConfirm: () => void = () => {};
  export let onCancel: () => void = () => {};
  
  // Wenn gefährliche Aktion, ändere Bestätigungsbutton-Farbe
  $: {
    if (dangerous) {
      confirmButtonClass = 'bg-red-600 hover:bg-red-700 text-white';
    }
  }
  
  // Schließt den Dialog
  function close() {
    show = false;
    onCancel();
  }
  
  // Bestätigt die Aktion
  function confirm() {
    show = false;
    onConfirm();
  }
  
  // Behandelt Escape-Taste
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape' && show) {
      close();
    }
  }
</script>

<svelte:window on:keydown={handleKeydown} />

{#if show}
  <div class="fixed inset-0 z-50 overflow-y-auto" aria-labelledby={title} role="dialog" aria-modal="true">
    <!-- Hintergrund-Overlay -->
    <div 
      class="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
      on:click={close}
      on:keydown={(e) => e.key === 'Escape' && close()}
      role="presentation"
    ></div>
    
    <!-- Dialog -->
    <div class="flex min-h-full items-center justify-center p-4 text-center">
      <div 
        class="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all w-full max-w-md"
      >
        <!-- Header -->
        <div class="bg-gray-50 px-4 py-3 border-b">
          <h3 class="text-lg font-medium text-gray-900" id="modal-title">{title}</h3>
        </div>
        
        <!-- Content -->
        <div class="px-4 py-4">
          <p class="text-sm text-gray-500">{message}</p>
        </div>
        
        <!-- Footer -->
        <div class="px-4 py-3 bg-gray-50 flex justify-end gap-2 border-t">
          <button
            type="button"
            class={`px-4 py-2 rounded text-sm font-medium ${cancelButtonClass}`}
            on:click={close}
          >
            {cancelText}
          </button>
          <button
            type="button"
            class={`px-4 py-2 rounded text-sm font-medium ${confirmButtonClass}`}
            on:click={confirm}
          >
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  </div>
{/if}
