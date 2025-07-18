<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [ZTS] Zero Tolerance Security - Sicherheits-Badges für Status-Anzeigen
  [SP] SQLCipher Pflicht - Verschlüsselungs-Status
  [ATV] Audit-Trail Vollständig - Audit-Status
-->
<script lang="ts">
  // Props
  export let type: 'encryption' | 'audit' | 'anonymization' | 'cloud' | 'swiss' = 'encryption';
  export let active: boolean = false;
  export let size: 'small' | 'medium' | 'large' = 'medium';
  export let tooltip: string = '';
  
  // Badge-Konfiguration basierend auf Typ
  function getBadgeConfig(badgeType: typeof type) {
    switch (badgeType) {
      case 'encryption':
        return {
          icon: '🔐',
          label: 'Verschlüsselt',
          color: 'green',
          description: 'SQLCipher AES-256 Verschlüsselung aktiv'
        };
      case 'audit':
        return {
          icon: '📋',
          label: 'Audit',
          color: 'blue',
          description: 'Audit-Trail wird vollständig aufgezeichnet'
        };
      case 'anonymization':
        return {
          icon: '🔒',
          label: 'Anonymisiert',
          color: 'purple',
          description: 'Automatische Anonymisierung aktiv'
        };
      case 'cloud':
        return {
          icon: '☁️',
          label: 'Cloud',
          color: 'orange',
          description: 'Cloud-Verarbeitung mit Einwilligung'
        };
      case 'swiss':
        return {
          icon: '🇨🇭',
          label: 'CH-Konform',
          color: 'red',
          description: 'Schweizer Datenschutz-Konformität'
        };
      default:
        return {
          icon: '🔐',
          label: 'Sicherheit',
          color: 'gray',
          description: 'Sicherheitsfeature'
        };
    }
  }
  
  $: config = getBadgeConfig(type);
  $: displayTooltip = tooltip || config.description;
</script>

<div 
  class="security-badge {config.color} {size}" 
  class:active
  class:inactive={!active}
  title={displayTooltip}
>
  <span class="badge-icon">{config.icon}</span>
  <span class="badge-label">{config.label}</span>
  {#if active}
    <span class="status-indicator active"></span>
  {:else}
    <span class="status-indicator inactive"></span>
  {/if}
</div>

<style>
  .security-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.375rem;
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.75rem;
    font-weight: 500;
    transition: all 0.3s ease;
    cursor: default;
    position: relative;
    border: 1px solid transparent;
  }
  
  /* Größen */
  .security-badge.small {
    padding: 0.125rem 0.5rem;
    font-size: 0.625rem;
    gap: 0.25rem;
  }
  
  .security-badge.medium {
    padding: 0.25rem 0.75rem;
    font-size: 0.75rem;
    gap: 0.375rem;
  }
  
  .security-badge.large {
    padding: 0.375rem 1rem;
    font-size: 0.875rem;
    gap: 0.5rem;
  }
  
  /* Farben - Aktiv */
  .security-badge.green.active {
    background: rgba(34, 197, 94, 0.15);
    border-color: rgba(34, 197, 94, 0.3);
    color: rgb(34, 197, 94);
  }
  
  .security-badge.blue.active {
    background: rgba(59, 130, 246, 0.15);
    border-color: rgba(59, 130, 246, 0.3);
    color: rgb(59, 130, 246);
  }
  
  .security-badge.purple.active {
    background: rgba(147, 51, 234, 0.15);
    border-color: rgba(147, 51, 234, 0.3);
    color: rgb(147, 51, 234);
  }
  
  .security-badge.orange.active {
    background: rgba(251, 146, 60, 0.15);
    border-color: rgba(251, 146, 60, 0.3);
    color: rgb(251, 146, 60);
  }
  
  .security-badge.red.active {
    background: rgba(239, 68, 68, 0.15);
    border-color: rgba(239, 68, 68, 0.3);
    color: rgb(239, 68, 68);
  }
  
  /* Farben - Inaktiv */
  .security-badge.inactive {
    background: rgba(107, 114, 128, 0.1);
    border-color: rgba(107, 114, 128, 0.2);
    color: rgb(107, 114, 128);
    opacity: 0.7;
  }
  
  .badge-icon {
    font-size: 1em;
  }
  
  .badge-label {
    font-weight: 500;
    white-space: nowrap;
  }
  
  .status-indicator {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    flex-shrink: 0;
  }
  
  .status-indicator.active {
    background: currentColor;
    animation: pulse 2s infinite;
  }
  
  .status-indicator.inactive {
    background: rgba(107, 114, 128, 0.5);
  }
  
  @keyframes pulse {
    0%, 100% {
      opacity: 1;
    }
    50% {
      opacity: 0.5;
    }
  }
  
  /* Hover-Effekte */
  .security-badge:hover {
    transform: translateY(-1px);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  /* Responsive Design */
  @media (max-width: 768px) {
    .security-badge {
      padding: 0.125rem 0.5rem;
      font-size: 0.625rem;
      gap: 0.25rem;
    }
    
    .badge-label {
      display: none;
    }
    
    .badge-icon {
      font-size: 1.2em;
    }
  }
</style>
