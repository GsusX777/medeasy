<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [ZTS] Zero Tolerance Security - Sichere Bestätigungsdialoge
  [PSF] Patient Safety First - Kritische Aktionen bestätigen
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  const dispatch = createEventDispatcher();
  
  export let title = 'Bestätigung erforderlich';
  export let message = 'Möchten Sie fortfahren?';
  export let confirmText = 'Bestätigen';
  export let cancelText = 'Abbrechen';
  export let type: 'info' | 'warning' | 'danger' = 'info';
  export let showIcon = true;
  
  function handleConfirm() {
    dispatch('confirm');
  }
  
  function handleCancel() {
    dispatch('cancel');
  }
  
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      handleCancel();
    } else if (event.key === 'Enter' && event.ctrlKey) {
      handleConfirm();
    }
  }
  
  // Icon basierend auf Typ [PSF]
  function getIcon(type: string): string {
    switch (type) {
      case 'warning':
        return '⚠️';
      case 'danger':
        return '🚨';
      default:
        return 'ℹ️';
    }
  }
  
  // Farben basierend auf Typ [ZTS]
  function getColors(type: string) {
    switch (type) {
      case 'warning':
        return {
          border: '#f59e0b',
          background: '#fef3c7',
          confirmBg: '#f59e0b',
          confirmHover: '#d97706'
        };
      case 'danger':
        return {
          border: '#dc2626',
          background: '#fee2e2',
          confirmBg: '#dc2626',
          confirmHover: '#b91c1c'
        };
      default:
        return {
          border: '#3b82f6',
          background: '#dbeafe',
          confirmBg: '#3b82f6',
          confirmHover: '#2563eb'
        };
    }
  }
  
  $: colors = getColors(type);
</script>

<svelte:window on:keydown={handleKeydown} />

<!-- Modal Backdrop -->
<div class="modal-backdrop" on:click={handleCancel} on:keydown={handleKeydown} role="dialog" aria-modal="true" aria-labelledby="dialog-title">
  <div class="modal-content" on:click|stopPropagation style="border-color: {colors.border}">
    <!-- Header -->
    <div class="modal-header" style="background-color: {colors.background}">
      <div class="header-content">
        {#if showIcon}
          <span class="dialog-icon">{getIcon(type)}</span>
        {/if}
        <h3 id="dialog-title">{title}</h3>
      </div>
    </div>
    
    <!-- Body -->
    <div class="modal-body">
      <p class="dialog-message">{message}</p>
    </div>
    
    <!-- Actions -->
    <div class="modal-actions">
      <button 
        class="btn-cancel" 
        on:click={handleCancel}
        type="button"
      >
        {cancelText}
      </button>
      <button 
        class="btn-confirm" 
        on:click={handleConfirm}
        style="background-color: {colors.confirmBg}"
        type="button"
        autofocus
      >
        {confirmText}
      </button>
    </div>
    
    <!-- Keyboard Hint -->
    <div class="keyboard-hint">
      <span>ESC: Abbrechen</span>
      <span>Ctrl+Enter: Bestätigen</span>
    </div>
  </div>
</div>

<style>
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 2000;
    backdrop-filter: blur(2px);
  }
  
  .modal-content {
    background-color: #ffffff;
    border-radius: 12px;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
    max-width: 400px;
    width: 90%;
    border: 2px solid;
    overflow: hidden;
    transform: scale(0.95);
    animation: modalEnter 0.2s ease-out forwards;
  }
  
  @keyframes modalEnter {
    to {
      transform: scale(1);
    }
  }
  
  .modal-header {
    padding: 20px 24px 16px;
    border-bottom: 1px solid #e5e7eb;
  }
  
  .header-content {
    display: flex;
    align-items: center;
    gap: 12px;
  }
  
  .dialog-icon {
    font-size: 24px;
    flex-shrink: 0;
  }
  
  .modal-header h3 {
    margin: 0;
    font-size: 18px;
    font-weight: 600;
    color: #1f2937;
    line-height: 1.4;
  }
  
  .modal-body {
    padding: 20px 24px;
  }
  
  .dialog-message {
    margin: 0;
    font-size: 14px;
    line-height: 1.6;
    color: #374151;
  }
  
  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding: 16px 24px 20px;
    border-top: 1px solid #f3f4f6;
  }
  
  .btn-cancel {
    padding: 8px 16px;
    background-color: #f9fafb;
    color: #374151;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: all 0.2s ease;
  }
  
  .btn-cancel:hover {
    background-color: #f3f4f6;
    border-color: #9ca3af;
  }
  
  .btn-confirm {
    padding: 8px 16px;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: all 0.2s ease;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
  }
  
  .btn-confirm:hover {
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  }
  
  .keyboard-hint {
    display: flex;
    justify-content: space-between;
    padding: 8px 24px 12px;
    font-size: 11px;
    color: #9ca3af;
    background-color: #f9fafb;
    border-top: 1px solid #f3f4f6;
  }
  
  /* Responsive Design [PSF] */
  @media (max-width: 768px) {
    .modal-content {
      width: 95%;
      margin: 16px;
    }
    
    .modal-header {
      padding: 16px 20px 12px;
    }
    
    .modal-body {
      padding: 16px 20px;
    }
    
    .modal-actions {
      padding: 12px 20px 16px;
      flex-direction: column-reverse;
    }
    
    .keyboard-hint {
      padding: 6px 20px 10px;
      font-size: 10px;
    }
  }
  
  /* Accessibility [PSF] */
  .btn-cancel:focus,
  .btn-confirm:focus {
    outline: 2px solid #3b82f6;
    outline-offset: 2px;
  }
  
  .btn-cancel:focus:not(:focus-visible),
  .btn-confirm:focus:not(:focus-visible) {
    outline: none;
  }
  
  /* High Contrast Mode Support [PSF] */
  @media (prefers-contrast: high) {
    .modal-content {
      border-width: 3px;
    }
    
    .dialog-message {
      color: #000000;
    }
    
    .btn-cancel {
      border-width: 2px;
      color: #000000;
    }
  }
  
  /* Reduced Motion Support [PSF] */
  @media (prefers-reduced-motion: reduce) {
    .modal-content {
      animation: none;
      transform: scale(1);
    }
    
    .btn-confirm:hover {
      transform: none;
    }
  }
  
  /* Dark Mode Support (falls später implementiert) */
  @media (prefers-color-scheme: dark) {
    .modal-content {
      background-color: #1f2937;
    }
    
    .modal-header h3 {
      color: #f9fafb;
    }
    
    .dialog-message {
      color: #d1d5db;
    }
    
    .btn-cancel {
      background-color: #374151;
      color: #d1d5db;
      border-color: #4b5563;
    }
    
    .btn-cancel:hover {
      background-color: #4b5563;
    }
    
    .keyboard-hint {
      background-color: #374151;
      color: #9ca3af;
    }
  }
</style>
