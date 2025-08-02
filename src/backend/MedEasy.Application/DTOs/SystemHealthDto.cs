// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System;
using System.Collections.Generic;

namespace MedEasy.Application.DTOs
{
    /// <summary>
    /// DTO für System-Health-Status [PSF][ATV]
    /// Überwacht kritische Systemzustände für Patientensicherheit
    /// </summary>
    public class SystemHealthDto
    {
        /// <summary>
        /// Aktueller Gesundheitsstatus des Systems [PSF]
        /// </summary>
        public SystemHealthStatus Status { get; set; }
        
        /// <summary>
        /// Liste der erkannten Probleme [PSF]
        /// </summary>
        public List<string> Issues { get; set; } = new();
        
        /// <summary>
        /// System-Laufzeit in Millisekunden [ATV]
        /// </summary>
        public long UptimeMs { get; set; }
        
        /// <summary>
        /// Letzter System-Neustart [ATV]
        /// </summary>
        public DateTime LastRestart { get; set; }
        
        /// <summary>
        /// CPU-Temperatur (falls verfügbar) [PSF]
        /// </summary>
        public double CpuTemperature { get; set; }
        
        /// <summary>
        /// Speicherdruck (0.0 - 1.0) [PSF]
        /// </summary>
        public double MemoryPressure { get; set; }
    }
    
    /// <summary>
    /// System-Health-Status-Enumeration [PSF]
    /// </summary>
    public enum SystemHealthStatus
    {
        /// <summary>
        /// System läuft normal [PSF]
        /// </summary>
        Healthy,
        
        /// <summary>
        /// Warnung - Aufmerksamkeit erforderlich [PSF]
        /// </summary>
        Warning,
        
        /// <summary>
        /// Kritisch - Sofortige Maßnahmen erforderlich [PSF]
        /// </summary>
        Critical
    }
}
