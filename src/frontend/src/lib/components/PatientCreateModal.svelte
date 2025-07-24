<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [EIV] Entitäten Immer Verschlüsselt - Patientendaten werden verschlüsselt gespeichert
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Sofortige Anonymisierung bei Eingabe
  [SF] Schweizer Formate - DD.MM.YYYY, Versicherungsnummer XXX.XXXX.XXXX.XX
  [ZTS] Zero Tolerance Security - Sichere Validierung und Verarbeitung
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import ConfirmDialog from './ConfirmDialog.svelte';
  import SecurityBadge from './SecurityBadge.svelte';
  
  const dispatch = createEventDispatcher();
  
  // Form Data [EIV][AIU][SF]
  let formData = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    insuranceNumber: ''
  };
  
  let errors: Record<string, string> = {};
  let isSubmitting = false;
  let showConfirmDialog = false;
  
  // Validation [ZTS][SF]
  function validateForm(): boolean {
    errors = {};
    
    // Vorname validieren
    if (!formData.firstName.trim()) {
      errors.firstName = 'Vorname ist erforderlich';
    } else if (formData.firstName.trim().length < 2) {
      errors.firstName = 'Vorname muss mindestens 2 Zeichen haben';
    }
    
    // Nachname validieren
    if (!formData.lastName.trim()) {
      errors.lastName = 'Nachname ist erforderlich';
    } else if (formData.lastName.trim().length < 2) {
      errors.lastName = 'Nachname muss mindestens 2 Zeichen haben';
    }
    
    // Geburtsdatum validieren [SF]
    if (!formData.dateOfBirth) {
      errors.dateOfBirth = 'Geburtsdatum ist erforderlich';
    } else {
      const birthDate = new Date(formData.dateOfBirth);
      const today = new Date();
      const age = today.getFullYear() - birthDate.getFullYear();
      
      if (birthDate > today) {
        errors.dateOfBirth = 'Geburtsdatum kann nicht in der Zukunft liegen';
      } else if (age > 150) {
        errors.dateOfBirth = 'Ungültiges Geburtsdatum';
      }
    }
    
    // Versicherungsnummer validieren [SF]
    if (!formData.insuranceNumber.trim()) {
      errors.insuranceNumber = 'Versicherungsnummer ist erforderlich';
    } else {
      const insuranceRegex = /^\d{3}\.\d{4}\.\d{4}\.\d{2}$/;
      if (!insuranceRegex.test(formData.insuranceNumber.trim())) {
        errors.insuranceNumber = 'Format: XXX.XXXX.XXXX.XX (z.B. 756.1234.5678.90)';
      }
    }
    
    return Object.keys(errors).length === 0;
  }
  
  // Anonymisierung Preview [AIU]
  function getAnonymizedPreview() {
    return {
      firstName: formData.firstName ? `${formData.firstName.charAt(0)}${'*'.repeat(Math.max(1, formData.firstName.length - 1))}` : '',
      lastName: formData.lastName ? `${formData.lastName.charAt(0)}${'*'.repeat(Math.max(1, formData.lastName.length - 1))}` : '',
      dateOfBirth: formData.dateOfBirth ? formatSwissDate(formData.dateOfBirth) : '',
      insuranceNumber: formData.insuranceNumber ? `${formData.insuranceNumber.substring(0, 3)}.****.****.${formData.insuranceNumber.substring(13, 15)}` : ''
    };
  }
  
  // Schweizer Datumsformat [SF]
  function formatSwissDate(isoDate: string): string {
    if (!isoDate) return '';
    const date = new Date(isoDate);
    return date.toLocaleDateString('de-CH', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }
  
  async function handleSubmit() {
    if (!validateForm()) {
      return;
    }
    
    showConfirmDialog = true;
  }
  
  async function confirmCreate() {
    try {
      isSubmitting = true;
      showConfirmDialog = false;
      
      // Simuliere API-Call [EIV][AIU]
      // In Produktion: Daten werden verschlüsselt und anonymisiert an .NET Backend gesendet
      const patientData = {
        ...formData,
        // Anonymisierte Versionen für UI [AIU]
        anonymizedFirstName: getAnonymizedPreview().firstName,
        anonymizedLastName: getAnonymizedPreview().lastName,
        anonymizedDateOfBirth: getAnonymizedPreview().dateOfBirth,
        insuranceNumberHash: getAnonymizedPreview().insuranceNumber
      };
      
      // Simuliere Netzwerk-Delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      console.log('Patient erstellt (verschlüsselt):', patientData);
      
      dispatch('patientCreated', { patient: patientData });
      dispatch('close');
      
    } catch (error) {
      console.error('Fehler beim Erstellen des Patienten:', error);
      errors.submit = 'Fehler beim Speichern. Bitte versuchen Sie es erneut.';
    } finally {
      isSubmitting = false;
    }
  }
  
  function handleClose() {
    if (isSubmitting) return;
    dispatch('close');
  }
  
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape' && !isSubmitting) {
      handleClose();
    }
  }
</script>

<svelte:window on:keydown={handleKeydown} />

<!-- Modal Backdrop -->
<div class="modal-backdrop" on:click={handleClose}>
  <div class="modal-content" on:click|stopPropagation>
    <!-- Header [AIU][ZTS] -->
    <div class="modal-header">
      <h3>
        <span class="icon">👤</span>
        Neuen Patienten anlegen
      </h3>
      <div class="header-badges">
        <SecurityBadge type="encrypted" text="Daten verschlüsselt" />
        <SecurityBadge type="anonymized" text="Auto-Anonymisierung" />
      </div>
      <button class="close-button" on:click={handleClose} disabled={isSubmitting}>
        ✕
      </button>
    </div>
    
    <!-- Form [EIV][AIU][SF] -->
    <form class="patient-form" on:submit|preventDefault={handleSubmit}>
      <div class="form-section">
        <h4>Persönliche Daten</h4>
        
        <div class="form-row">
          <div class="form-group">
            <label for="firstName">Vorname *</label>
            <input
              id="firstName"
              type="text"
              bind:value={formData.firstName}
              class:error={errors.firstName}
              disabled={isSubmitting}
              placeholder="Max"
              maxlength="50"
              required
            />
            {#if errors.firstName}
              <span class="error-message">{errors.firstName}</span>
            {/if}
          </div>
          
          <div class="form-group">
            <label for="lastName">Nachname *</label>
            <input
              id="lastName"
              type="text"
              bind:value={formData.lastName}
              class:error={errors.lastName}
              disabled={isSubmitting}
              placeholder="Muster"
              maxlength="50"
              required
            />
            {#if errors.lastName}
              <span class="error-message">{errors.lastName}</span>
            {/if}
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="dateOfBirth">Geburtsdatum * [SF]</label>
            <input
              id="dateOfBirth"
              type="date"
              bind:value={formData.dateOfBirth}
              class:error={errors.dateOfBirth}
              disabled={isSubmitting}
              max={new Date().toISOString().split('T')[0]}
              required
            />
            {#if errors.dateOfBirth}
              <span class="error-message">{errors.dateOfBirth}</span>
            {/if}
          </div>
          
          <div class="form-group">
            <label for="insuranceNumber">Versicherungsnummer * [SF]</label>
            <input
              id="insuranceNumber"
              type="text"
              bind:value={formData.insuranceNumber}
              class:error={errors.insuranceNumber}
              disabled={isSubmitting}
              placeholder="756.1234.5678.90"
              pattern="\d{3}\.\d{4}\.\d{4}\.\d{2}"
              maxlength="15"
              required
            />
            {#if errors.insuranceNumber}
              <span class="error-message">{errors.insuranceNumber}</span>
            {/if}
          </div>
        </div>
      </div>
      
      <!-- Anonymisierung Preview [AIU] -->
      {#if formData.firstName || formData.lastName || formData.dateOfBirth || formData.insuranceNumber}
        <div class="form-section preview-section">
          <h4>
            <span class="icon">🔒</span>
            Anonymisierte Darstellung (UI-Vorschau)
          </h4>
          <div class="preview-content">
            <div class="preview-item">
              <span class="preview-label">Name:</span>
              <span class="preview-value">
                {getAnonymizedPreview().lastName} {getAnonymizedPreview().firstName}
              </span>
            </div>
            <div class="preview-item">
              <span class="preview-label">Geburtsdatum:</span>
              <span class="preview-value">{getAnonymizedPreview().dateOfBirth}</span>
            </div>
            <div class="preview-item">
              <span class="preview-label">Versicherung:</span>
              <span class="preview-value">{getAnonymizedPreview().insuranceNumber}</span>
            </div>
          </div>
          <p class="preview-note">
            ℹ️ So werden die Daten in der Benutzeroberfläche angezeigt. 
            Die Originaldaten werden verschlüsselt gespeichert.
          </p>
        </div>
      {/if}
      
      <!-- Submit Error -->
      {#if errors.submit}
        <div class="error-message submit-error">
          ⚠️ {errors.submit}
        </div>
      {/if}
      
      <!-- Actions -->
      <div class="modal-actions">
        <button 
          type="button" 
          class="btn-secondary" 
          on:click={handleClose}
          disabled={isSubmitting}
        >
          Abbrechen
        </button>
        <button 
          type="submit" 
          class="btn-primary"
          disabled={isSubmitting}
        >
          {#if isSubmitting}
            <span class="spinner">⟳</span>
            Wird gespeichert...
          {:else}
            <span class="icon">💾</span>
            Patient anlegen
          {/if}
        </button>
      </div>
    </form>
  </div>
</div>

<!-- Confirmation Dialog [ZTS] -->
{#if showConfirmDialog}
  <ConfirmDialog
    title="Patient anlegen bestätigen"
    message="Möchten Sie den neuen Patienten mit den eingegebenen Daten anlegen? Die Daten werden verschlüsselt gespeichert und automatisch anonymisiert."
    confirmText="Patient anlegen"
    cancelText="Abbrechen"
    on:confirm={confirmCreate}
    on:cancel={() => showConfirmDialog = false}
  />
{/if}

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
    z-index: 1000;
  }
  
  .modal-content {
    background-color: #ffffff;
    border-radius: 12px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    max-width: 600px;
    width: 90%;
    max-height: 90vh;
    overflow-y: auto;
  }
  
  .modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 24px 24px 16px;
    border-bottom: 1px solid #e5e7eb;
    position: relative;
  }
  
  .modal-header h3 {
    display: flex;
    align-items: center;
    gap: 8px;
    margin: 0;
    font-size: 20px;
    font-weight: 600;
    color: #1f2937;
  }
  
  .header-badges {
    display: flex;
    gap: 8px;
    position: absolute;
    right: 50px;
    top: 50%;
    transform: translateY(-50%);
  }
  
  .close-button {
    background: none;
    border: none;
    cursor: pointer;
    font-size: 20px;
    color: #6b7280;
    padding: 4px;
    border-radius: 4px;
    transition: all 0.2s ease;
  }
  
  .close-button:hover:not(:disabled) {
    background-color: #f3f4f6;
    color: #374151;
  }
  
  .close-button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .patient-form {
    padding: 24px;
  }
  
  .form-section {
    margin-bottom: 32px;
  }
  
  .form-section h4 {
    display: flex;
    align-items: center;
    gap: 8px;
    margin: 0 0 16px 0;
    font-size: 16px;
    font-weight: 600;
    color: #374151;
  }
  
  .form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
    margin-bottom: 16px;
  }
  
  .form-group {
    display: flex;
    flex-direction: column;
  }
  
  .form-group label {
    font-size: 14px;
    font-weight: 500;
    color: #374151;
    margin-bottom: 4px;
  }
  
  .form-group input {
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;
    transition: border-color 0.2s ease;
  }
  
  .form-group input:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
  
  .form-group input.error {
    border-color: #dc2626;
  }
  
  .form-group input:disabled {
    background-color: #f9fafb;
    cursor: not-allowed;
  }
  
  .error-message {
    color: #dc2626;
    font-size: 12px;
    margin-top: 4px;
  }
  
  .submit-error {
    background-color: #fef2f2;
    border: 1px solid #fecaca;
    border-radius: 6px;
    padding: 12px;
    margin-bottom: 16px;
    font-size: 14px;
  }
  
  .preview-section {
    background-color: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 16px;
  }
  
  .preview-content {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 12px;
  }
  
  .preview-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .preview-label {
    font-weight: 500;
    color: #374151;
    font-size: 14px;
  }
  
  .preview-value {
    font-family: 'Courier New', monospace;
    color: #6b7280;
    font-size: 14px;
  }
  
  .preview-note {
    font-size: 13px;
    color: #6b7280;
    margin: 0;
    font-style: italic;
  }
  
  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding-top: 16px;
    border-top: 1px solid #e5e7eb;
  }
  
  .btn-primary {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 20px;
    background-color: #3b82f6;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: background-color 0.2s ease;
  }
  
  .btn-primary:hover:not(:disabled) {
    background-color: #2563eb;
  }
  
  .btn-primary:disabled {
    opacity: 0.7;
    cursor: not-allowed;
  }
  
  .btn-secondary {
    padding: 10px 20px;
    background-color: #f9fafb;
    color: #374151;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.2s ease;
  }
  
  .btn-secondary:hover:not(:disabled) {
    background-color: #f3f4f6;
  }
  
  .btn-secondary:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .spinner {
    animation: spin 1s linear infinite;
  }
  
  @keyframes spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
  }
  
  .icon {
    font-size: 16px;
  }
  
  /* Responsive Design [PSF] */
  @media (max-width: 768px) {
    .modal-content {
      width: 95%;
      margin: 16px;
    }
    
    .modal-header {
      padding: 16px;
    }
    
    .header-badges {
      position: static;
      transform: none;
      margin-top: 8px;
    }
    
    .patient-form {
      padding: 16px;
    }
    
    .form-row {
      grid-template-columns: 1fr;
      gap: 12px;
    }
    
    .modal-actions {
      flex-direction: column-reverse;
    }
  }
</style>
