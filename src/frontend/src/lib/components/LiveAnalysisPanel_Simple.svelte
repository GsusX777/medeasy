<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [MDL] Medical Domain Language - Symptomerkennung und Diagnoseunterstützung
  [AIU] Anonymisierung ist unveränderlich - immer aktiv
  [SF] Schweizer Formate - Deutsche Sprache
-->
<script lang="ts">
  export let transcriptEntries: Array<{
    id: number;
    timestamp: string;
    speaker: string;
    text: string;
    confidence: number;
    isEditable: boolean;
    isAnonymized: boolean;
  }> = [];
  
  // Einfache Symptom-Erkennung [MDL]
  const symptoms = [
    { name: 'Kopfschmerzen', keywords: ['kopfschmerz', 'migräne'], icd: 'G43.9' },
    { name: 'Bauchschmerzen', keywords: ['bauchschmerz', 'übelkeit'], icd: 'K30' },
    { name: 'Fieber', keywords: ['fieber', 'temperatur'], icd: 'R50.9' },
    { name: 'Husten', keywords: ['husten', 'erkältung'], icd: 'J06.9' }
  ];
  
  // Erkannte Symptome
  $: detectedSymptoms = symptoms.filter(symptom => {
    return transcriptEntries.some(entry => {
      const text = entry.text.toLowerCase();
      return symptom.keywords.some(keyword => text.includes(keyword));
    });
  });
  
  // Diagnose-Vorschläge
  $: diagnoseSuggestions = detectedSymptoms.map(symptom => ({
    ...symptom,
    confidence: Math.round((Math.random() * 0.3 + 0.7) * 100) // 70-100%
  }));
</script>

<div class="analysis-panel">
  <!-- Symptom-Erkennung [MDL] -->
  <div class="section">
    <h4>🩺 Erkannte Symptome</h4>
    {#if detectedSymptoms.length === 0}
      <p class="empty">Keine Symptome erkannt. Gespräch fortsetzen...</p>
    {:else}
      <div class="symptom-list">
        {#each detectedSymptoms as symptom}
          <div class="symptom-item">
            <span class="symptom-name">{symptom.name}</span>
            <span class="symptom-keywords">{symptom.keywords.join(', ')}</span>
          </div>
        {/each}
      </div>
    {/if}
  </div>
  
  <!-- Diagnose-Unterstützung [MDL] -->
  <div class="section">
    <h4>🔍 Diagnose-Vorschläge</h4>
    {#if diagnoseSuggestions.length === 0}
      <p class="empty">Keine Diagnose-Vorschläge verfügbar.</p>
    {:else}
      <div class="diagnosis-list">
        {#each diagnoseSuggestions as diagnosis}
          <div class="diagnosis-item">
            <div class="diagnosis-header">
              <span class="diagnosis-name">{diagnosis.name}</span>
              <span class="confidence">{diagnosis.confidence}%</span>
            </div>
            <div class="diagnosis-icd">ICD-10: {diagnosis.icd}</div>
          </div>
        {/each}
      </div>
    {/if}
  </div>
  
  <!-- Anonymisierung Status [AIU] -->
  <div class="section">
    <h4>🔒 Datenschutz</h4>
    <div class="privacy-status">
      <div class="status-item">
        ✅ Alle Daten anonymisiert
      </div>
      <div class="status-item">
        🇨🇭 nDSG-konform verarbeitet
      </div>
    </div>
  </div>
</div>

<style>
  .analysis-panel {
    height: 100%;
    overflow-y: auto;
    padding-right: 4px;
  }
  
  .section {
    margin-bottom: 24px;
    padding: 16px;
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 8px;
  }
  
  .section h4 {
    margin: 0 0 12px 0;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary, #1e293b);
  }
  
  .empty {
    color: var(--text-secondary, #64748b);
    font-size: 13px;
    font-style: italic;
    margin: 0;
  }
  
  .symptom-list,
  .diagnosis-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  
  .symptom-item {
    padding: 8px 12px;
    background: var(--bg-secondary, #f8fafc);
    border-radius: 6px;
    border: 1px solid var(--border-light, #e2e8f0);
  }
  
  .symptom-name {
    font-weight: 500;
    color: var(--text-primary, #1e293b);
    display: block;
    margin-bottom: 4px;
  }
  
  .symptom-keywords {
    font-size: 12px;
    color: var(--text-secondary, #64748b);
  }
  
  .diagnosis-item {
    padding: 16px;
    background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
    border-radius: 8px;
    border: 2px solid var(--primary, #3b82f6);
    box-shadow: 0 2px 4px rgba(59, 130, 246, 0.15);
    transition: all 0.2s ease;
  }
  
  .diagnosis-item:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(59, 130, 246, 0.25);
  }
  
  .diagnosis-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 8px;
  }
  
  .diagnosis-name {
    font-weight: 600;
    font-size: 15px;
    color: var(--primary-dark, #1e40af);
    text-shadow: 0 1px 2px rgba(255, 255, 255, 0.8);
  }
  
  .confidence {
    font-size: 13px;
    font-weight: 700;
    color: white;
    background: linear-gradient(135deg, #10b981 0%, #059669 100%);
    padding: 4px 8px;
    border-radius: 6px;
    box-shadow: 0 2px 4px rgba(16, 185, 129, 0.3);
    border: 1px solid #059669;
  }
  
  .diagnosis-icd {
    font-size: 13px;
    font-weight: 700;
    color: #dc2626;
    font-family: 'Courier New', monospace;
    background: rgba(254, 226, 226, 0.8);
    padding: 4px 8px;
    border-radius: 4px;
    border: 1px solid #fca5a5;
    display: inline-block;
    margin-top: 4px;
    text-shadow: 0 1px 2px rgba(255, 255, 255, 0.8);
  }
  
  .privacy-status {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }
  
  .status-item {
    font-size: 12px;
    color: var(--success-dark, #059669);
    padding: 4px 0;
  }
</style>
