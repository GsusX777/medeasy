<!-- �Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein � ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- MedEasy Anonymization Review [AIU] [ARQ] -->
<script lang="ts">
  import { onMount } from 'svelte';
  import { transcriptsNeedingReview, loadTranscriptsNeedingReview } from '$lib/stores/database';
  import type { TranscriptDto } from '$lib/types/database';
  import { invoke } from '@tauri-apps/api/tauri';

  let loading = true;
  let error: string | null = null;
  
  // Review status options
  const reviewStatuses = {
    APPROVED: 'Approved',
    REJECTED: 'Rejected',
    WHITELIST: 'Whitelisted'
  };
  
  // Track review notes for each transcript
  let reviewNotes: Record<string, string> = {};
  
  // Track which transcript is being reviewed
  let currentReviewId: string | null = null;
  
  onMount(async () => {
    try {
      await loadTranscriptsNeedingReview();
      loading = false;
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
      loading = false;
    }
  });
  
  /**
   * Start reviewing a transcript
   */
  function startReview(transcriptId: string) {
    currentReviewId = transcriptId;
    if (!reviewNotes[transcriptId]) {
      reviewNotes[transcriptId] = '';
    }
  }
  
  /**
   * Submit a review decision
   */
  async function submitReview(transcriptId: string, status: string) {
    try {
      loading = true;
      
      // Call Tauri command to update review status
      await invoke('update_transcript_review_status', {
        transcriptId,
        status,
        notes: reviewNotes[transcriptId] || '',
        userId: localStorage.getItem('medeasy_user_id') || 'default_user'
      });
      
      // Remove from the list of transcripts needing review
      transcriptsNeedingReview.update(items => 
        items.filter(item => item.id !== transcriptId)
      );
      
      currentReviewId = null;
    } catch (err) {
      error = err instanceof Error ? err.message : String(err);
    } finally {
      loading = false;
    }
  }
  
  /**
   * Cancel current review
   */
  function cancelReview() {
    currentReviewId = null;
  }
  
  /**
   * Format confidence as percentage
   */
  function formatConfidence(confidence: number | undefined): string {
    if (confidence === undefined) return 'Unbekannt';
    return `${Math.round(confidence * 100)}%`;
  }
</script>

<div class="anonymization-review">
  <h2>Anonymisierungs-Review [ARQ]</h2>
  
  {#if loading}
    <div class="loading">
      <span class="loader"></span>
      <p>Lade Transkripte zur Überprüfung...</p>
    </div>
  {:else if error}
    <div class="error">
      <p>Fehler beim Laden der Review-Items: {error}</p>
      <button on:click={() => { error = null; loadTranscriptsNeedingReview(); }}>
        Erneut versuchen
      </button>
    </div>
  {:else if $transcriptsNeedingReview.length === 0}
    <div class="empty-state">
      <p>Keine Transkripte zur Überprüfung vorhanden.</p>
    </div>
  {:else}
    <div class="review-list">
      {#if currentReviewId === null}
        <p class="review-info">
          {$transcriptsNeedingReview.length} Transkript(e) benötigen eine Überprüfung aufgrund niedriger Anonymisierungskonfidenz.
        </p>
        <table>
          <thead>
            <tr>
              <th>Erstellungsdatum</th>
              <th>Konfidenz</th>
              <th>Session ID</th>
              <th>Aktionen</th>
            </tr>
          </thead>
          <tbody>
            {#each $transcriptsNeedingReview as transcript}
              <tr>
                <td>{new Date(transcript.created).toLocaleString('de-CH')}</td>
                <td class="confidence {transcript.anonymization_confidence && transcript.anonymization_confidence < 0.7 ? 'low' : ''}">
                  {formatConfidence(transcript.anonymization_confidence)}
                </td>
                <td>{transcript.session_id}</td>
                <td>
                  <button on:click={() => startReview(transcript.id)}>
                    Überprüfen
                  </button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      {:else}
        <!-- Detailansicht eines Transkripts zur Überprüfung [AIU] -->
        {#if $transcriptsNeedingReview.some(t => t.id === currentReviewId)}
          {#each $transcriptsNeedingReview.filter(t => t.id === currentReviewId) as transcript}
            <div class="review-item">
              <h3>Transkript überprüfen</h3>
              
              <div class="review-details">
                <p><strong>Session ID:</strong> {transcript.session_id}</p>
                <p><strong>Erstellt am:</strong> {new Date(transcript.created).toLocaleString('de-CH')}</p>
                <p><strong>Anonymisierungskonfidenz:</strong> 
                  <span class="confidence {transcript.anonymization_confidence && transcript.anonymization_confidence < 0.7 ? 'low' : ''}">
                    {formatConfidence(transcript.anonymization_confidence)}
                  </span>
                </p>
              </div>
              
              <div class="review-content">
                <h4>Anonymisierter Text:</h4>
                <div class="transcript-text">
                  {transcript.anonymized_text}
                </div>
              </div>
              
              <div class="review-notes">
                <label for="review-notes">Notizen zur Überprüfung:</label>
                <textarea 
                  id="review-notes"
                  bind:value={reviewNotes[transcript.id]} 
                  placeholder="Fügen Sie hier Ihre Notizen zur Überprüfung hinzu..."
                ></textarea>
              </div>
              
              <div class="review-actions">
                <button 
                  class="approve" 
                  on:click={() => submitReview(transcript.id, reviewStatuses.APPROVED)}
                >
                  Genehmigen
                </button>
                <button 
                  class="whitelist" 
                  on:click={() => submitReview(transcript.id, reviewStatuses.WHITELIST)}
                >
                  Zur Whitelist hinzufügen
                </button>
                <button 
                  class="reject" 
                  on:click={() => submitReview(transcript.id, reviewStatuses.REJECTED)}
                >
                  Ablehnen
                </button>
                <button 
                  class="cancel" 
                  on:click={cancelReview}
                >
                  Abbrechen
                </button>
              </div>
            </div>
          {/each}
        {:else}
          <p>Transkript nicht gefunden.</p>
          <button on:click={cancelReview}>Zurück zur Liste</button>
        {/if}
      {/if}
    </div>
  {/if}
</div>

<style>
  .anonymization-review {
    width: 100%;
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
  }
  
  h2 {
    color: #2c3e50;
    margin-bottom: 20px;
    border-bottom: 2px solid #3498db;
    padding-bottom: 10px;
  }
  
  .loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 0;
  }
  
  .loader {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #3498db;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 20px;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  .error {
    background-color: #ffecec;
    color: #e74c3c;
    padding: 15px;
    border-radius: 5px;
    margin: 20px 0;
    border-left: 4px solid #e74c3c;
  }
  
  .empty-state {
    text-align: center;
    padding: 40px;
    color: #7f8c8d;
  }
  
  table {
    width: 100%;
    border-collapse: collapse;
    margin: 20px 0;
  }
  
  th, td {
    padding: 12px 15px;
    text-align: left;
    border-bottom: 1px solid #ddd;
  }
  
  th {
    background-color: #f8f9fa;
    font-weight: bold;
  }
  
  tr:hover {
    background-color: #f5f5f5;
  }
  
  .confidence {
    font-weight: bold;
  }
  
  .confidence.low {
    color: #e74c3c;
  }
  
  button {
    padding: 8px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    background-color: #3498db;
    color: white;
    font-weight: bold;
    transition: background-color 0.3s;
  }
  
  button:hover {
    background-color: #2980b9;
  }
  
  .review-item {
    background-color: #f8f9fa;
    padding: 20px;
    border-radius: 5px;
    border: 1px solid #ddd;
  }
  
  .review-details {
    margin-bottom: 20px;
    padding-bottom: 15px;
    border-bottom: 1px solid #ddd;
  }
  
  .transcript-text {
    background-color: white;
    padding: 15px;
    border: 1px solid #ddd;
    border-radius: 4px;
    margin: 10px 0 20px;
    white-space: pre-wrap;
    max-height: 300px;
    overflow-y: auto;
  }
  
  .review-notes {
    margin-bottom: 20px;
  }
  
  .review-notes label {
    display: block;
    margin-bottom: 5px;
    font-weight: bold;
  }
  
  .review-notes textarea {
    width: 100%;
    min-height: 100px;
    padding: 10px;
    border: 1px solid #ddd;
    border-radius: 4px;
    resize: vertical;
  }
  
  .review-actions {
    display: flex;
    gap: 10px;
    justify-content: flex-end;
  }
  
  .approve {
    background-color: #2ecc71;
  }
  
  .approve:hover {
    background-color: #27ae60;
  }
  
  .whitelist {
    background-color: #f39c12;
  }
  
  .whitelist:hover {
    background-color: #e67e22;
  }
  
  .reject {
    background-color: #e74c3c;
  }
  
  .reject:hover {
    background-color: #c0392b;
  }
  
  .cancel {
    background-color: #7f8c8d;
  }
  
  .cancel:hover {
    background-color: #6c7a89;
  }
  
  .review-info {
    background-color: #eaf2f8;
    padding: 10px 15px;
    border-left: 4px solid #3498db;
    margin-bottom: 15px;
  }
</style>
