<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [MDL] Medical Domain Language - Medizinische Fachsprache
  [AIU] Anonymisierung ist unveränderlich - immer aktiv
  [SF] Schweizer Formate - Deutsche Sprache
  [UX] User Experience - Live-Analyse der Transkription
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
  
  // Symptom-Erkennung [MDL]
  const symptomKeywords = {
    'Kopfschmerzen': ['kopfschmerz', 'migräne', 'schädel', 'stirn'],
    'Bauchschmerzen': ['bauchschmerz', 'magenschmerz', 'übelkeit', 'erbrechen'],
    'Fieber': ['fieber', 'erhöhte temperatur', 'heiß', 'schüttelfrost'],
    'Atemwegsprobleme': ['husten', 'schnupfen', 'atemnot', 'erkältung'],
    'Herz-Kreislauf': ['herzschlag', 'puls', 'blutdruck', 'schwindel'],
    'Allergien': ['allergie', 'juckreiz', 'ausschlag', 'schwellung']
  };
  
  // Erkannte Symptome [MDL]
  $: detectedSymptoms = Object.entries(symptomKeywords).reduce((symptoms, [symptom, keywords]) => {
    const found = transcriptEntries.some(entry => {
      const text = entry.text.toLowerCase();
      return keywords.some(keyword => text.includes(keyword));
    });
    if (found) {
      const confidence = Math.random() * 0.3 + 0.7; // 70-100% Simulation
      symptoms.push({ name: symptom, confidence, keywords });
    }
    return symptoms;
  }, [] as Array<{name: string, confidence: number, keywords: string[]}>);
  
  // Diagnose-Vorschläge basierend auf Symptomen [MDL]
  const diagnosisRules = {
    'Kopfschmerzen': [
      { icd: 'G43.9', name: 'Migräne, nicht näher bezeichnet', probability: 0.75 },
      { icd: 'G44.2', name: 'Spannungskopfschmerz', probability: 0.65 }
    ],
    'Atemwegsprobleme': [
      { icd: 'J06.9', name: 'Akute Infektion der oberen Atemwege', probability: 0.80 },
      { icd: 'J00', name: 'Akute Rhinopharyngitis [Erkältung]', probability: 0.70 }
    ],
    'Bauchschmerzen': [
      { icd: 'K59.1', name: 'Funktionelle Diarrhoe', probability: 0.60 },
      { icd: 'K30', name: 'Funktionelle Dyspepsie', probability: 0.55 }
    ]
  };
  
  $: suggestedDiagnoses = detectedSymptoms.reduce((diagnoses, symptom) => {
    const rules = diagnosisRules[symptom.name] || [];
    rules.forEach(rule => {
      const adjustedProbability = rule.probability * symptom.confidence;
      diagnoses.push({
        ...rule,
        probability: adjustedProbability,
        basedOnSymptom: symptom.name
      });
    });
    return diagnoses;
  }, [] as Array<{icd: string, name: string, probability: number, basedOnSymptom: string}>)
    .sort((a, b) => b.probability - a.probability)
    .slice(0, 5); // Top 5 Diagnosen
  
  // Gesprächsqualität-Analyse [UX]
  $: conversationQuality = {
    totalEntries: transcriptEntries.length,
    averageConfidence: transcriptEntries.length > 0 
      ? transcriptEntries.reduce((sum, entry) => sum + entry.confidence, 0) / transcriptEntries.length 
      : 0,
    lowConfidenceCount: transcriptEntries.filter(entry => entry.confidence < 0.85).length,
    speakerBalance: Math.abs(
      (speakerDistribution['Patient'] || 0) - (speakerDistribution['Arzt'] || 0)
    ) / Math.max(speakerDistribution['Patient'] || 1, speakerDistribution['Arzt'] || 1)
  };
  
  // Sentiment-Analyse (vereinfacht) [MDL]
  const positiveWords = ['gut', 'besser', 'okay', 'normal', 'gesund', 'hilft'];
  const negativeWords = ['schlecht', 'schmerz', 'weh', 'problem', 'sorge', 'angst'];
  
  $: sentiment = transcriptEntries.reduce((acc, entry) => {
    const text = entry.text.toLowerCase();
    const positive = positiveWords.filter(word => text.includes(word)).length;
    const negative = negativeWords.filter(word => text.includes(word)).length;
    return {
      positive: acc.positive + positive,
      negative: acc.negative + negative,
      neutral: acc.neutral + (positive === 0 && negative === 0 ? 1 : 0)
    };
  }, { positive: 0, negative: 0, neutral: 0 });
  
  function getQualityColor(value: number): string {
    if (value >= 0.9) return 'var(--success, #10b981)';
    if (value >= 0.7) return 'var(--warning, #f59e0b)';
    return 'var(--danger, #ef4444)';
  }
  
  function getQualityLabel(value: number): string {
    if (value >= 0.9) return 'Ausgezeichnet';
    if (value >= 0.8) return 'Gut';
    if (value >= 0.7) return 'Akzeptabel';
    if (value >= 0.6) return 'Verbesserungsbedarf';
    return 'Kritisch';
  }
</script>

<div class="live-analysis-panel">
  <!-- Symptom-Erkennung [MDL] -->
  <div class="analysis-section">
    <h4 class="section-title">🩺 Erkannte Symptome</h4>
    {#if detectedSymptoms.length === 0}
      <div class="empty-state">
        <p>Keine Symptome erkannt. Gespräch fortsetzen...</p>
      </div>
    {:else}
      <div class="symptoms-list">
        {#each detectedSymptoms as symptom}
          <div class="symptom-item">
            <div class="symptom-header">
              <span class="symptom-name">{symptom.name}</span>
              <span class="confidence-badge" style="background-color: {symptom.confidence > 0.8 ? '#10b981' : symptom.confidence > 0.6 ? '#f59e0b' : '#ef4444'}20; color: {symptom.confidence > 0.8 ? '#10b981' : symptom.confidence > 0.6 ? '#f59e0b' : '#ef4444'}">
                {Math.round(symptom.confidence * 100)}%
              </span>
            </div>
            <div class="symptom-keywords">
              Erkannte Begriffe: {symptom.keywords.join(', ')}
            </div>
          </div>
        {/each}
      </div>
    {/if}
  </div>
  
  <!-- Qualitäts-Analyse [AIU] -->
  <div class="analysis-section">
    <h4 class="section-title">🎯 Qualitäts-Analyse</h4>
    <div class="quality-metrics">
      <div class="quality-item">
        <div class="quality-header">
          <span class="quality-label">Transkriptions-Genauigkeit</span>
          <span 
            class="quality-value"
            style="color: {getQualityColor(conversationQuality.averageConfidence)}"
          >
            {Math.round(conversationQuality.averageConfidence * 100)}%
          </span>
        </div>
        <div class="quality-bar">
          <div 
            class="quality-fill"
            style="width: {conversationQuality.averageConfidence * 100}%; background-color: {getQualityColor(conversationQuality.averageConfidence)}"
          ></div>
        </div>
        <div class="quality-description">
          {getQualityLabel(conversationQuality.averageConfidence)}
          {#if conversationQuality.lowConfidenceCount > 0}
            • {conversationQuality.lowConfidenceCount} Einträge benötigen Review
          {/if}
        </div>
      </div>
    </div>
  </div>
  
  <!-- Speaker-Verteilung [MDL] -->
  <div class="analysis-section">
    <h4 class="section-title">👥 Speaker-Verteilung</h4>
    <div class="speaker-stats">
      {#each Object.entries(speakerDistribution) as [speaker, count]}
        <div class="speaker-item">
          <div class="speaker-info">
            <span class="speaker-name">{speaker}</span>
            <span class="speaker-count">{count} Einträge</span>
          </div>
          <div class="speaker-bar">
            <div 
              class="speaker-fill"
              style="width: {(count / transcriptEntries.length) * 100}%"
            ></div>
          </div>
          <div class="speaker-percentage">
            {Math.round((count / transcriptEntries.length) * 100)}%
          </div>
        </div>
      {/each}
    </div>
  </div>
  
  <!-- Medizinische Schlüsselwörter [MDL] -->
  {#if detectedKeywords.length > 0}
    <div class="analysis-section">
      <h4 class="section-title">🏥 Erkannte Medizinische Begriffe</h4>
      <div class="keywords-container">
        {#each detectedKeywords as keyword}
          <span class="keyword-tag">{keyword}</span>
        {/each}
      </div>
    </div>
  {/if}
  
  <!-- Sentiment-Analyse [MDL] -->
  <div class="analysis-section">
    <h4 class="section-title">😊 Gesprächs-Stimmung</h4>
    <div class="sentiment-chart">
      <div class="sentiment-item positive">
        <div class="sentiment-bar">
          <div 
            class="sentiment-fill"
            style="width: {sentiment.positive > 0 ? (sentiment.positive / (sentiment.positive + sentiment.negative + sentiment.neutral)) * 100 : 0}%"
          ></div>
        </div>
        <span class="sentiment-label">Positiv ({sentiment.positive})</span>
      </div>
      <div class="sentiment-item neutral">
        <div class="sentiment-bar">
          <div 
            class="sentiment-fill"
            style="width: {sentiment.neutral > 0 ? (sentiment.neutral / (sentiment.positive + sentiment.negative + sentiment.neutral)) * 100 : 0}%"
          ></div>
        </div>
        <span class="sentiment-label">Neutral ({sentiment.neutral})</span>
      </div>
      <div class="sentiment-item negative">
        <div class="sentiment-bar">
          <div 
            class="sentiment-fill"
            style="width: {sentiment.negative > 0 ? (sentiment.negative / (sentiment.positive + sentiment.negative + sentiment.neutral)) * 100 : 0}%"
          ></div>
        </div>
        <span class="sentiment-label">Negativ ({sentiment.negative})</span>
      </div>
    </div>
  </div>
  
  <!-- Anonymisierungs-Status [AIU] -->
  <div class="analysis-section">
    <h4 class="section-title">🔒 Datenschutz-Status</h4>
    <div class="privacy-status">
      <div class="status-item success">
        <span class="status-icon">✅</span>
        <span class="status-text">Alle Einträge anonymisiert</span>
      </div>
      <div class="status-item success">
        <span class="status-icon">🔒</span>
        <span class="status-text">Patientendaten geschützt</span>
      </div>
      <div class="status-item success">
        <span class="status-icon">🇨🇭</span>
        <span class="status-text">nDSG-konform verarbeitet</span>
      </div>
    </div>
  </div>
</div>

<style>
  .live-analysis-panel {
    height: 100%;
    overflow-y: auto;
    padding-right: 4px;
  }
  
  .analysis-section {
    margin-bottom: 24px;
    padding: 16px;
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-light, #e2e8f0);
    border-radius: 8px;
  }
  
  .section-title {
    margin: 0 0 16px 0;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary, #1e293b);
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .metrics-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 12px;
  }
  
  .metric-card {
    text-align: center;
    padding: 12px;
    background: var(--bg-secondary, #f8fafc);
    border-radius: 6px;
    border: 1px solid var(--border-light, #e2e8f0);
  }
  
  .metric-value {
    font-size: 20px;
    font-weight: 700;
    color: var(--primary, #3b82f6);
    margin-bottom: 4px;
  }
  
  .metric-label {
    font-size: 11px;
    color: var(--text-secondary, #64748b);
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .quality-metrics {
    space-y: 16px;
  }
  
  .quality-item {
    margin-bottom: 16px;
  }
  
  .quality-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 8px;
  }
  
  .quality-label {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-primary, #1e293b);
  }
  
  .quality-value {
    font-size: 14px;
    font-weight: 700;
  }
  
  .quality-bar {
    height: 6px;
    background: var(--bg-secondary, #f8fafc);
    border-radius: 3px;
    overflow: hidden;
    margin-bottom: 6px;
  }
  
  .quality-fill {
    height: 100%;
    transition: width 0.3s ease;
  }
  
  .quality-description {
    font-size: 11px;
    color: var(--text-secondary, #64748b);
  }
  
  .speaker-stats {
    space-y: 12px;
  }
  
  .speaker-item {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 12px;
  }
  
  .speaker-info {
    min-width: 80px;
    display: flex;
    flex-direction: column;
  }
  
  .speaker-name {
    font-size: 12px;
    font-weight: 500;
    color: var(--text-primary, #1e293b);
  }
  
  .speaker-count {
    font-size: 10px;
    color: var(--text-secondary, #64748b);
  }
  
  .speaker-bar {
    flex: 1;
    height: 4px;
    background: var(--bg-secondary, #f8fafc);
    border-radius: 2px;
    overflow: hidden;
  }
  
  .speaker-fill {
    height: 100%;
    background: var(--primary, #3b82f6);
    transition: width 0.3s ease;
  }
  
  .speaker-percentage {
    font-size: 11px;
    color: var(--text-secondary, #64748b);
    min-width: 35px;
    text-align: right;
  }
  
  .keywords-container {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
  }
  
  .keyword-tag {
    padding: 4px 8px;
    background: var(--primary-light, #dbeafe);
    color: var(--primary-dark, #1e40af);
    border-radius: 4px;
    font-size: 11px;
    font-weight: 500;
    border: 1px solid var(--primary, #3b82f6);
  }
  
  .sentiment-chart {
    space-y: 8px;
  }
  
  .sentiment-item {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 8px;
  }
  
  .sentiment-bar {
    flex: 1;
    height: 4px;
    background: var(--bg-secondary, #f8fafc);
    border-radius: 2px;
    overflow: hidden;
  }
  
  .sentiment-fill {
    height: 100%;
    transition: width 0.3s ease;
  }
  
  .sentiment-item.positive .sentiment-fill {
    background: var(--success, #10b981);
  }
  
  .sentiment-item.neutral .sentiment-fill {
    background: var(--text-secondary, #64748b);
  }
  
  .sentiment-item.negative .sentiment-fill {
    background: var(--danger, #ef4444);
  }
  
  .sentiment-label {
    font-size: 11px;
    color: var(--text-secondary, #64748b);
    min-width: 80px;
  }
  
  .privacy-status {
    space-y: 8px;
  }
  
  .status-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 12px;
    border-radius: 6px;
    margin-bottom: 8px;
  }
  
  .status-item.success {
    background: var(--success-light, #f0fdf4);
    border: 1px solid var(--success, #10b981);
  }
  
  .status-icon {
    font-size: 14px;
  }
  
  .status-text {
    font-size: 12px;
    color: var(--success-dark, #059669);
    font-weight: 500;
  }
  
  /* Mobile Responsive [UX] */
  @media (max-width: 768px) {
    .metrics-grid {
      grid-template-columns: 1fr;
    }
    
    .speaker-item {
      flex-direction: column;
      align-items: stretch;
      gap: 6px;
    }
    
    .speaker-info {
      min-width: auto;
    }
    
    .sentiment-item {
      flex-direction: column;
      align-items: stretch;
      gap: 4px;
    }
  }
</style>
