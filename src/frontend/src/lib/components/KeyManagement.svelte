<!-- "Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  MedEasy Schlüsselverwaltungs-Komponente [SP][ZTS][EIV][CT]
  Ermöglicht die Verwaltung von Verschlüsselungsschlüsseln und Sicherheitseinstellungen
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { invoke } from '@tauri-apps/api/tauri';
  import { writable, type Writable } from 'svelte/store';
  import { currentUser } from '../stores/auth';
  import { showNotification } from '../stores/notifications';
  import { isProduction } from '../utils/environment';
  import Spinner from './common/Spinner.svelte';
  import SecurityBadge from './common/SecurityBadge.svelte';
  import ConfirmDialog from './common/ConfirmDialog.svelte';

  // Schlüsselstatus-Typ [EIV]
  interface KeyStatus {
    key_type: string;
    last_rotated?: string;
    days_until_rotation: number;
    status: string;
    version: number;
  }

  // Rotationsergebnis [ATV]
  interface KeyRotationResult {
    success: boolean;
    message: string;
    timestamp: string;
  }

  // Zustand
  let keyStatusList: KeyStatus[] = [];
  let loading = true;
  let error = '';
  let selectedKeyType = '';
  let showRotationConfirm = false;
  let showPasswordChangeDialog = false;
  let showRecoveryDialog = false;
  let showShamirDialog = false;
  
  // Passwort-Änderung
  let oldPassword = '';
  let newPassword = '';
  let confirmPassword = '';
  let passwordError = '';
  
  // Wiederherstellung
  let recoveryPassword = '';
  let recoveryData = '';
  let recoveryError = '';
  
  // Shamir-Shares
  let threshold = 3;
  let totalShares = 5;
  let shamirShares: string[] = [];
  let shamirError = '';

  // Lädt den Schlüsselstatus beim Mounten der Komponente
  onMount(async () => {
    await loadKeyStatus();
  });

  // Lädt den aktuellen Schlüsselstatus [CT]
  async function loadKeyStatus() {
    try {
      loading = true;
      error = '';
      
      keyStatusList = await invoke<KeyStatus[]>('get_key_status');
      
      // Sortiere nach Tagen bis zur Rotation (dringendste zuerst)
      keyStatusList.sort((a, b) => a.days_until_rotation - b.days_until_rotation);
    } catch (err) {
      error = `Fehler beim Laden des Schlüsselstatus: ${err}`;
      console.error(error);
    } finally {
      loading = false;
    }
  }

  // Öffnet den Rotations-Dialog für einen Schlüssel [SP]
  function openRotationDialog(keyType: string) {
    selectedKeyType = keyType;
    showRotationConfirm = true;
  }

  // Rotiert einen Schlüssel [SP][ATV]
  async function rotateKey() {
    if (!selectedKeyType) return;
    
    try {
      loading = true;
      error = '';
      
      // Hole aktuelle Benutzer-ID
      const userId = $currentUser?.id || 'system';
      
      // Führe Rotation durch
      const result = await invoke<KeyRotationResult>('rotate_key', {
        keyTypeStr: selectedKeyType,
        userId
      });
      
      if (result.success) {
        showNotification({
          type: 'success',
          message: result.message,
          duration: 5000
        });
        
        // Aktualisiere Schlüsselstatus
        await loadKeyStatus();
      }
    } catch (err) {
      error = `Fehler bei der Schlüsselrotation: ${err}`;
      console.error(error);
      
      showNotification({
        type: 'error',
        message: error,
        duration: 5000
      });
    } finally {
      loading = false;
      showRotationConfirm = false;
      selectedKeyType = '';
    }
  }

  // Ändert das Master-Passwort [ZTS]
  async function changeMasterPassword() {
    // Validiere Passwörter
    if (!oldPassword || !newPassword || !confirmPassword) {
      passwordError = 'Bitte alle Felder ausfüllen';
      return;
    }
    
    if (newPassword !== confirmPassword) {
      passwordError = 'Die neuen Passwörter stimmen nicht überein';
      return;
    }
    
    if (newPassword.length < 12) {
      passwordError = 'Das neue Passwort muss mindestens 12 Zeichen lang sein';
      return;
    }
    
    try {
      loading = true;
      passwordError = '';
      
      // Ändere Passwort
      const success = await invoke<boolean>('change_master_password', {
        oldPassword,
        newPassword
      });
      
      if (success) {
        showNotification({
          type: 'success',
          message: 'Master-Passwort erfolgreich geändert',
          duration: 5000
        });
        
        // Schließe Dialog und setze Felder zurück
        showPasswordChangeDialog = false;
        oldPassword = '';
        newPassword = '';
        confirmPassword = '';
      }
    } catch (err) {
      passwordError = `Fehler bei der Passwortänderung: ${err}`;
      console.error(passwordError);
    } finally {
      loading = false;
    }
  }

  // Erstellt Notfall-Wiederherstellungsdaten [FSD]
  async function createRecoveryData() {
    // Validiere Passwort
    if (!recoveryPassword || recoveryPassword.length < 12) {
      recoveryError = 'Das Wiederherstellungspasswort muss mindestens 12 Zeichen lang sein';
      return;
    }
    
    try {
      loading = true;
      recoveryError = '';
      
      // Erstelle Wiederherstellungsdaten
      recoveryData = await invoke<string>('create_recovery_data', {
        recoveryPassword
      });
      
      showNotification({
        type: 'success',
        message: 'Wiederherstellungsdaten erfolgreich erstellt',
        duration: 5000
      });
    } catch (err) {
      recoveryError = `Fehler bei der Erstellung von Wiederherstellungsdaten: ${err}`;
      console.error(recoveryError);
    } finally {
      loading = false;
    }
  }

  // Erstellt Shamir-Shares für den Master-Schlüssel [FSD]
  async function createShamirShares() {
    // Validiere Parameter
    if (threshold < 2 || threshold > totalShares || totalShares > 10) {
      shamirError = 'Ungültige Parameter für Shamir Sharing';
      return;
    }
    
    try {
      loading = true;
      shamirError = '';
      
      // Erstelle Shamir-Shares
      shamirShares = await invoke<string[]>('create_shamir_shares', {
        params: {
          threshold,
          shares: totalShares
        }
      });
      
      showNotification({
        type: 'success',
        message: `${totalShares} Shamir-Shares erfolgreich erstellt`,
        duration: 5000
      });
    } catch (err) {
      shamirError = `Fehler bei der Erstellung von Shamir-Shares: ${err}`;
      console.error(shamirError);
    } finally {
      loading = false;
    }
  }

  // Kopiert Text in die Zwischenablage
  async function copyToClipboard(text: string) {
    try {
      await navigator.clipboard.writeText(text);
      showNotification({
        type: 'info',
        message: 'In die Zwischenablage kopiert',
        duration: 3000
      });
    } catch (err) {
      console.error('Fehler beim Kopieren in die Zwischenablage:', err);
    }
  }

  // Bestimmt die Farbe basierend auf dem Status [CT]
  function getStatusColor(status: string, daysUntil: number): string {
    if (status === 'Überfällig') return 'text-red-600';
    if (status === 'Bald fällig' || daysUntil <= 7) return 'text-yellow-600';
    return 'text-green-600';
  }
</script>

<div class="key-management p-4 bg-white rounded-lg shadow">
  <div class="flex justify-between items-center mb-6">
    <h2 class="text-xl font-bold">Schlüsselverwaltung [SP][ZTS]</h2>
    <SecurityBadge secured={true} tooltip="Alle Schlüssel werden sicher verwaltet" />
  </div>

  <!-- Fehler-Anzeige -->
  {#if error}
    <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
      <p>{error}</p>
    </div>
  {/if}

  <!-- Lade-Indikator -->
  {#if loading}
    <div class="flex justify-center my-8">
      <Spinner size="lg" />
    </div>
  {:else}
    <!-- Schlüsselstatus-Tabelle -->
    <div class="overflow-x-auto">
      <table class="min-w-full bg-white border border-gray-200">
        <thead>
          <tr>
            <th class="py-2 px-4 border-b text-left">Schlüsseltyp</th>
            <th class="py-2 px-4 border-b text-left">Letzte Rotation</th>
            <th class="py-2 px-4 border-b text-left">Status</th>
            <th class="py-2 px-4 border-b text-left">Version</th>
            <th class="py-2 px-4 border-b text-left">Aktionen</th>
          </tr>
        </thead>
        <tbody>
          {#each keyStatusList as keyStatus}
            <tr class="hover:bg-gray-50">
              <td class="py-2 px-4 border-b">{keyStatus.key_type}</td>
              <td class="py-2 px-4 border-b">{keyStatus.last_rotated || 'Nie'}</td>
              <td class="py-2 px-4 border-b">
                <span class={getStatusColor(keyStatus.status, keyStatus.days_until_rotation)}>
                  {keyStatus.status}
                  {#if keyStatus.days_until_rotation > 0}
                    (in {keyStatus.days_until_rotation} Tagen)
                  {:else if keyStatus.days_until_rotation < 0}
                    (seit {Math.abs(keyStatus.days_until_rotation)} Tagen)
                  {:else}
                    (heute)
                  {/if}
                </span>
              </td>
              <td class="py-2 px-4 border-b">{keyStatus.version}</td>
              <td class="py-2 px-4 border-b">
                <button 
                  class="bg-blue-500 hover:bg-blue-600 text-white py-1 px-3 rounded text-sm"
                  on:click={() => openRotationDialog(keyStatus.key_type)}
                >
                  Rotieren
                </button>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>

    <!-- Aktionen -->
    <div class="mt-6 flex flex-wrap gap-4">
      <button 
        class="bg-gray-800 hover:bg-gray-900 text-white py-2 px-4 rounded"
        on:click={() => showPasswordChangeDialog = true}
      >
        Master-Passwort ändern
      </button>
      
      <button 
        class="bg-yellow-600 hover:bg-yellow-700 text-white py-2 px-4 rounded"
        on:click={() => showRecoveryDialog = true}
      >
        Notfall-Wiederherstellung
      </button>
      
      <button 
        class="bg-purple-600 hover:bg-purple-700 text-white py-2 px-4 rounded"
        on:click={() => showShamirDialog = true}
      >
        Shamir Secret Sharing
      </button>
    </div>

    <!-- Produktionswarnung -->
    {#if isProduction()}
      <div class="mt-6 bg-yellow-50 border-l-4 border-yellow-400 p-4">
        <div class="flex">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
            </svg>
          </div>
          <div class="ml-3">
            <p class="text-sm text-yellow-700">
              <strong>Achtung:</strong> Schlüsseländerungen in der Produktionsumgebung sollten nur nach einem vollständigen Backup durchgeführt werden.
            </p>
          </div>
        </div>
      </div>
    {/if}
  {/if}
</div>

<!-- Rotations-Bestätigungsdialog -->
<ConfirmDialog
  show={showRotationConfirm}
  title="Schlüssel rotieren"
  message={`Sind Sie sicher, dass Sie den Schlüssel '${selectedKeyType}' rotieren möchten? Diese Aktion kann nicht rückgängig gemacht werden.`}
  confirmText="Rotieren"
  cancelText="Abbrechen"
  onConfirm={rotateKey}
  onCancel={() => showRotationConfirm = false}
/>

<!-- Passwort-Änderungsdialog -->
{#if showPasswordChangeDialog}
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full">
      <h3 class="text-lg font-bold mb-4">Master-Passwort ändern</h3>
      
      {#if passwordError}
        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-2 rounded mb-4">
          <p>{passwordError}</p>
        </div>
      {/if}
      
      <form on:submit|preventDefault={changeMasterPassword}>
        <div class="mb-4">
          <label class="block text-sm font-medium mb-1" for="old-password">Aktuelles Passwort</label>
          <input 
            type="password" 
            id="old-password" 
            bind:value={oldPassword}
            class="w-full px-3 py-2 border border-gray-300 rounded"
            required
          />
        </div>
        
        <div class="mb-4">
          <label class="block text-sm font-medium mb-1" for="new-password">Neues Passwort</label>
          <input 
            type="password" 
            id="new-password" 
            bind:value={newPassword}
            class="w-full px-3 py-2 border border-gray-300 rounded"
            required
            minlength="12"
          />
          <p class="text-xs text-gray-500 mt-1">Mindestens 12 Zeichen</p>
        </div>
        
        <div class="mb-6">
          <label class="block text-sm font-medium mb-1" for="confirm-password">Neues Passwort bestätigen</label>
          <input 
            type="password" 
            id="confirm-password" 
            bind:value={confirmPassword}
            class="w-full px-3 py-2 border border-gray-300 rounded"
            required
          />
        </div>
        
        <div class="flex justify-end gap-2">
          <button 
            type="button"
            class="px-4 py-2 border border-gray-300 rounded text-gray-700"
            on:click={() => showPasswordChangeDialog = false}
            disabled={loading}
          >
            Abbrechen
          </button>
          <button 
            type="submit"
            class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            disabled={loading}
          >
            {loading ? 'Wird geändert...' : 'Passwort ändern'}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

<!-- Wiederherstellungsdialog -->
{#if showRecoveryDialog}
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full">
      <h3 class="text-lg font-bold mb-4">Notfall-Wiederherstellung [FSD]</h3>
      
      {#if recoveryError}
        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-2 rounded mb-4">
          <p>{recoveryError}</p>
        </div>
      {/if}
      
      {#if !recoveryData}
        <form on:submit|preventDefault={createRecoveryData}>
          <div class="mb-4">
            <label class="block text-sm font-medium mb-1" for="recovery-password">Wiederherstellungspasswort</label>
            <input 
              type="password" 
              id="recovery-password" 
              bind:value={recoveryPassword}
              class="w-full px-3 py-2 border border-gray-300 rounded"
              required
              minlength="12"
            />
            <p class="text-xs text-gray-500 mt-1">Mindestens 12 Zeichen. Bewahren Sie dieses Passwort sicher auf!</p>
          </div>
          
          <div class="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-4">
            <p class="text-sm text-yellow-700">
              <strong>Wichtig:</strong> Diese Daten ermöglichen die Wiederherstellung Ihrer Verschlüsselungsschlüssel im Notfall. 
              Bewahren Sie sie an einem sicheren Ort auf, getrennt vom Wiederherstellungspasswort.
            </p>
          </div>
          
          <div class="flex justify-end gap-2">
            <button 
              type="button"
              class="px-4 py-2 border border-gray-300 rounded text-gray-700"
              on:click={() => showRecoveryDialog = false}
              disabled={loading}
            >
              Abbrechen
            </button>
            <button 
              type="submit"
              class="px-4 py-2 bg-yellow-600 text-white rounded hover:bg-yellow-700"
              disabled={loading}
            >
              {loading ? 'Wird erstellt...' : 'Wiederherstellungsdaten erstellen'}
            </button>
          </div>
        </form>
      {:else}
        <div class="mb-4">
          <label class="block text-sm font-medium mb-1" for="recovery-data">Wiederherstellungsdaten</label>
          <textarea 
            id="recovery-data"
            class="w-full px-3 py-2 border border-gray-300 rounded font-mono text-xs h-32"
            readonly
          >{recoveryData}</textarea>
        </div>
        
        <div class="bg-green-50 border-l-4 border-green-400 p-4 mb-4">
          <p class="text-sm text-green-700">
            <strong>Erfolg:</strong> Wiederherstellungsdaten wurden erstellt. Speichern Sie diese Daten sofort an einem sicheren Ort!
          </p>
        </div>
        
        <div class="flex justify-end gap-2">
          <button 
            type="button"
            class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            on:click={() => copyToClipboard(recoveryData)}
          >
            In Zwischenablage kopieren
          </button>
          <button 
            type="button"
            class="px-4 py-2 border border-gray-300 rounded text-gray-700"
            on:click={() => {
              showRecoveryDialog = false;
              recoveryData = '';
              recoveryPassword = '';
            }}
          >
            Schließen
          </button>
        </div>
      {/if}
    </div>
  </div>
{/if}

<!-- Shamir-Shares-Dialog -->
{#if showShamirDialog}
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full">
      <h3 class="text-lg font-bold mb-4">Shamir Secret Sharing [FSD]</h3>
      
      {#if shamirError}
        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-2 rounded mb-4">
          <p>{shamirError}</p>
        </div>
      {/if}
      
      {#if shamirShares.length === 0}
        <form on:submit|preventDefault={createShamirShares}>
          <div class="mb-4">
            <label class="block text-sm font-medium mb-1" for="threshold">Schwellenwert (m)</label>
            <input 
              type="number" 
              id="threshold" 
              bind:value={threshold}
              min="2"
              max="10"
              class="w-full px-3 py-2 border border-gray-300 rounded"
              required
            />
            <p class="text-xs text-gray-500 mt-1">Mindestanzahl an Shares für Wiederherstellung</p>
          </div>
          
          <div class="mb-4">
            <label class="block text-sm font-medium mb-1" for="total-shares">Gesamtanzahl Shares (n)</label>
            <input 
              type="number" 
              id="total-shares" 
              bind:value={totalShares}
              min="3"
              max="10"
              class="w-full px-3 py-2 border border-gray-300 rounded"
              required
            />
            <p class="text-xs text-gray-500 mt-1">Gesamtanzahl zu erstellender Shares</p>
          </div>
          
          <div class="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-4">
            <p class="text-sm text-yellow-700">
              <strong>Wichtig:</strong> Mit Shamir Secret Sharing können Sie den Master-Schlüssel auf mehrere Shares aufteilen. 
              Für die Wiederherstellung werden mindestens m von n Shares benötigt. Bewahren Sie die Shares an verschiedenen sicheren Orten auf.
            </p>
          </div>
          
          <div class="flex justify-end gap-2">
            <button 
              type="button"
              class="px-4 py-2 border border-gray-300 rounded text-gray-700"
              on:click={() => showShamirDialog = false}
              disabled={loading}
            >
              Abbrechen
            </button>
            <button 
              type="submit"
              class="px-4 py-2 bg-purple-600 text-white rounded hover:bg-purple-700"
              disabled={loading}
            >
              {loading ? 'Wird erstellt...' : 'Shares erstellen'}
            </button>
          </div>
        </form>
      {:else}
        <div class="mb-4">
          <h3 class="block text-sm font-medium mb-2" id="shamir-shares-heading">Shamir Shares ({threshold} von {totalShares})</h3>
          
          <div class="space-y-2 max-h-64 overflow-y-auto" aria-labelledby="shamir-shares-heading">
            {#each shamirShares as share, i}
              <div class="border border-gray-300 rounded p-2">
                <div class="flex justify-between items-center mb-1">
                  <span class="font-medium">Share #{i+1}</span>
                  <button 
                    class="text-blue-600 hover:text-blue-800 text-sm"
                    on:click={() => copyToClipboard(share)}
                  >
                    Kopieren
                  </button>
                </div>
                <div class="font-mono text-xs break-all bg-gray-50 p-2 rounded">
                  {share.substring(0, 20)}...
                </div>
              </div>
            {/each}
          </div>
        </div>
        
        <div class="bg-green-50 border-l-4 border-green-400 p-4 mb-4">
          <p class="text-sm text-green-700">
            <strong>Erfolg:</strong> {totalShares} Shamir Shares wurden erstellt. Mindestens {threshold} Shares werden für die Wiederherstellung benötigt.
            Speichern Sie diese Shares sofort an verschiedenen sicheren Orten!
          </p>
        </div>
        
        <div class="flex justify-end gap-2">
          <button 
            type="button"
            class="px-4 py-2 border border-gray-300 rounded text-gray-700"
            on:click={() => {
              showShamirDialog = false;
              shamirShares = [];
            }}
          >
            Schließen
          </button>
        </div>
      {/if}
    </div>
  </div>
{/if}

<style>
  .key-management {
    font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  }
</style>
