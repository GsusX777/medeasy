<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [EIV] Entitäten Immer Verschlüsselt - Konsultationsdaten werden verschlüsselt gespeichert
  [AIU] Anonymisierung ist UNVERÄNDERLICH - Patientenauswahl anonymisiert
  [SF] Schweizer Formate - DD.MM.YYYY, HH:MM Zeitformate
  [ZTS] Zero Tolerance Security - Sichere Validierung und Verarbeitung
-->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import ConfirmDialog from './ConfirmDialog.svelte';
  import SecurityBadge from './SecurityBadge.svelte';
  
  const dispatch = createEventDispatcher();
  
  // Form Data [EIV][SF]
  let formData = {
    patientId: '',
    sessionDate: '',
    startTime: '',
    endTime: '',
    notes: ''
  };
  
  let errors: Record<string, string> = {};
  let isSubmitting = false;
  let showConfirmDialog = false;
  
  // Mock-Patientenliste [AIU]
  const patients = [
    { id: '1', name: 'Max **** Muster ****' },
    { id: '2', name: 'Anna **** Schmidt ****' },
    { id: '3', name: 'Peter **** Weber ****' }
  ];
  
  // Validation [ZTS][SF]
  function validateForm(): boolean {
    errors = {};
    
    // Patient validieren
    if (!formData.patientId) {
      errors.patientId = 'Bitte wählen Sie einen Patienten aus';
    }
    
    // Datum validieren [SF]
    if (!formData.sessionDate) {
      errors.sessionDate = 'Konsultationsdatum ist erforderlich';
    } else {
      const sessionDate = new Date(formData.sessionDate);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      
      if (sessionDate < today) {
        errors.sessionDate = 'Konsultationsdatum kann nicht in der Vergangenheit liegen';
      }
    }
    
    // Zeiten validieren [SF]
    if (formData.startTime && formData.endTime) {
      const start = new Date(`2000-01-01T${formData.startTime}`);
      const end = new Date(`2000-01-01T${formData.endTime}`);
      
      if (end <= start) {
        errors.endTime = 'Endzeit muss nach der Startzeit liegen';
      }
    }
    
    return Object.keys(errors).length === 0;
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
      
      // Simuliere API-Call [EIV]
      // In Produktion: Daten werden verschlüsselt an .NET Backend gesendet
      const consultationData = {
        ...formData,
        status: 'Scheduled',
        patientName: patients.find(p => p.id === formData.patientId)?.name || '',
        sessionDateFormatted: formatSwissDate(formData.sessionDate)
      };
      
      // Simuliere Netzwerk-Delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      console.log('Konsultation erstellt (verschlüsselt):', consultationData);
      
      dispatch('consultationCreated', { consultation: consultationData });
      dispatch('close');
      
    } catch (error) {
      console.error('Fehler beim Erstellen der Konsultation:', error);
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
  
  // Set default date to today [SF]
  function setToday() {
    const today = new Date();
    formData.sessionDate = today.toISOString().split('T')[0];
  }
  
  // Set default time to next hour
  function setNextHour() {
    const now = new Date();
    now.setHours(now.getHours() + 1, 0, 0, 0);
    formData.startTime = now.toTimeString().substring(0, 5);
    
    // Set end time to 45 minutes later
    now.setMinutes(45);
    formData.endTime = now.toTimeString().substring(0, 5);
  }
</script>

<svelte:window on:keydown={handleKeydown} />

<!-- Modal Backdrop -->
<div class="modal-backdrop" on:click={handleClose}>
  <div class="modal-content" on:click|stopPropagation>
    <!-- Header [AIU][ZTS] -->
    <div class="modal-header">
      <h3>
        <span class="icon">📋</span>
        Neue Konsultation planen
      </h3>
      <div class="header-badges">
        <SecurityBadge type="encrypted" text="Daten verschlüsselt" />
        <SecurityBadge type="anonymized" text="Patienten anonymisiert" />
      </div>
      <button class="close-button" on:click={handleClose} disabled={isSubmitting}>
        ✕
      </button>
    </div>
    
    <!-- Form [EIV][AIU][SF] -->
    <form class="consultation-form" on:submit|preventDefault={handleSubmit}>
      <div class="form-section">
        <h4>Konsultationsdetails</h4>
        
        <div class="form-group">
          <label for="patientId">Patient * [AIU]</label>
          <select
            id="patientId"
            bind:value={formData.patientId}
            class:error={errors.patientId}
            disabled={isSubmitting}
            required
          >
            <option value="">-- Patient auswählen --</option>
            {#each patients as patient}
              <option value={patient.id}>{patient.name}</option>
            {/each}
          </select>
          {#if errors.patientId}
            <span class="error-message">{errors.patientId}</span>
          {/if}
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="sessionDate">Datum * [SF]</label>
            <div class="input-with-button">
              <input
                id="sessionDate"
                type="date"
                bind:value={formData.sessionDate}
                class:error={errors.sessionDate}
                disabled={isSubmitting}
                min={new Date().toISOString().split('T')[0]}
                required
              />
              <button type="button" class="btn-icon" on:click={setToday} title="Heute">
                📅
              </button>
            </div>
            {#if errors.sessionDate}
              <span class="error-message">{errors.sessionDate}</span>
            {/if}
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="startTime">Startzeit [SF]</label>
            <input
              id="startTime"
              type="time"
              bind:value={formData.startTime}
              class:error={errors.startTime}
              disabled={isSubmitting}
              step="300"
            />
            {#if errors.startTime}
              <span class="error-message">{errors.startTime}</span>
            {/if}
          </div>
          
          <div class="form-group">
            <label for="endTime">Endzeit [SF]</label>
            <div class="input-with-button">
              <input
                id="endTime"
                type="time"
                bind:value={formData.endTime}
                class:error={errors.endTime}
                disabled={isSubmitting}
                step="300"
              />
              <button type="button" class="btn-icon" on:click={setNextHour} title="Nächste Stunde">
                🕐
              </button>
            </div>
            {#if errors.endTime}
              <span class="error-message">{errors.endTime}</span>
            {/if}
          </div>
        </div>
        
        <div class="form-group">
          <label for="notes">Notizen [EIV]</label>
          <textarea
            id="notes"
            bind:value={formData.notes}
            disabled={isSubmitting}
            placeholder="Grund der Konsultation, Vorbereitung, etc..."
            rows="3"
            maxlength="500"
          ></textarea>
          <div class="char-count">
            {formData.notes.length}/500 Zeichen
          </div>
        </div>
      </div>
      
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
            <span class="icon">📅</span>
            Konsultation planen
          {/if}
        </button>
      </div>
    </form>
  </div>
</div>

<!-- Confirmation Dialog [ZTS] -->
{#if showConfirmDialog}
  <ConfirmDialog
    title="Konsultation planen bestätigen"
    message="Möchten Sie die neue Konsultation mit den eingegebenen Daten planen? Die Daten werden verschlüsselt gespeichert."
    confirmText="Konsultation planen"
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
    max-width: 500px;
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
  
  .consultation-form {
    padding: 24px;
  }
  
  .form-section {
    margin-bottom: 32px;
  }
  
  .form-section h4 {
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
    margin-bottom: 16px;
  }
  
  .form-group label {
    font-size: 14px;
    font-weight: 500;
    color: #374151;
    margin-bottom: 4px;
  }
  
  .form-group input,
  .form-group select,
  .form-group textarea {
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;
    transition: border-color 0.2s ease;
  }
  
  .form-group input:focus,
  .form-group select:focus,
  .form-group textarea:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
  
  .form-group input.error,
  .form-group select.error,
  .form-group textarea.error {
    border-color: #dc2626;
  }
  
  .form-group input:disabled,
  .form-group select:disabled,
  .form-group textarea:disabled {
    background-color: #f9fafb;
    cursor: not-allowed;
  }
  
  .input-with-button {
    display: flex;
    gap: 8px;
  }
  
  .input-with-button input {
    flex: 1;
  }
  
  .btn-icon {
    background: none;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    cursor: pointer;
    padding: 8px;
    font-size: 14px;
    transition: all 0.2s ease;
  }
  
  .btn-icon:hover {
    background-color: #f3f4f6;
  }
  
  .char-count {
    font-size: 12px;
    color: #6b7280;
    text-align: right;
    margin-top: 4px;
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
    
    .consultation-form {
      padding: 16px;
    }
    
    .form-row {
      grid-template-columns: 1fr;
    }
    
    .modal-actions {
      flex-direction: column-reverse;
    }
  }
</style>
