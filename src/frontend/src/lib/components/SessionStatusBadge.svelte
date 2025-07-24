<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

<!-- 
  [ZTS] Zero Tolerance Security - Status-Anzeige für Session-Zustände
  [SF] Schweizer Formate - Deutsche Beschriftungen
-->
<script lang="ts">
  export let status: 'Scheduled' | 'InProgress' | 'Completed' | 'Cancelled';
  
  // Status-Konfiguration [SF]
  const statusConfig = {
    Scheduled: {
      label: 'Geplant',
      icon: '📅',
      color: 'blue'
    },
    InProgress: {
      label: 'Laufend',
      icon: '🔴',
      color: 'green'
    },
    Completed: {
      label: 'Abgeschlossen',
      icon: '✅',
      color: 'gray'
    },
    Cancelled: {
      label: 'Abgebrochen',
      icon: '❌',
      color: 'red'
    }
  };
  
  $: config = statusConfig[status];
</script>

<span class="status-badge" class:blue={config.color === 'blue'} class:green={config.color === 'green'} class:gray={config.color === 'gray'} class:red={config.color === 'red'}>
  <span class="status-icon">{config.icon}</span>
  <span class="status-label">{config.label}</span>
</span>

<style>
  .status-badge {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    padding: 4px 8px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 500;
    white-space: nowrap;
  }
  
  .status-badge.blue {
    background-color: #dbeafe;
    color: #1e40af;
  }
  
  .status-badge.green {
    background-color: #dcfce7;
    color: #166534;
  }
  
  .status-badge.gray {
    background-color: #f3f4f6;
    color: #374151;
  }
  
  .status-badge.red {
    background-color: #fee2e2;
    color: #dc2626;
  }
  
  .status-icon {
    font-size: 10px;
  }
  
  .status-label {
    font-weight: 500;
  }
  
  /* Responsive Design [PSF] */
  @media (max-width: 768px) {
    .status-badge {
      font-size: 11px;
      padding: 3px 6px;
    }
    
    .status-label {
      display: none;
    }
    
    .status-icon {
      font-size: 12px;
    }
  }
</style>
