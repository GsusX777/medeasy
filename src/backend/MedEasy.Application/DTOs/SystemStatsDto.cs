// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System;

namespace MedEasy.Application.DTOs
{
    /// <summary>
    /// DTO für System-Statistiken im Admin-Dashboard [PSF][ATV]
    /// Übersicht über Systemleistung und Benchmark-Historie
    /// </summary>
    public class SystemStatsDto
    {
        /// <summary>
        /// Aktueller System-Gesundheitsstatus [PSF]
        /// </summary>
        public string SystemHealth { get; set; } = string.Empty;
        
        /// <summary>
        /// System-Laufzeit in Millisekunden [ATV]
        /// </summary>
        public long Uptime { get; set; }
        
        /// <summary>
        /// Zeitpunkt des letzten Benchmarks [WMM]
        /// </summary>
        public DateTime LastBenchmark { get; set; }
        
        /// <summary>
        /// Gesamtanzahl durchgeführter Benchmarks [WMM]
        /// </summary>
        public int TotalBenchmarks { get; set; }
        
        /// <summary>
        /// Durchschnittliche System-Performance (0-100%) [PSF]
        /// </summary>
        public double AveragePerformance { get; set; }
        
        /// <summary>
        /// Anzahl CPU-Kerne [PSF]
        /// </summary>
        public int CpuCores { get; set; }
        
        /// <summary>
        /// Gesamt-RAM in GB [PSF]
        /// </summary>
        public double TotalRamGb { get; set; }
        
        /// <summary>
        /// Betriebssystem-Information [ATV]
        /// </summary>
        public string OperatingSystem { get; set; } = string.Empty;
        
        /// <summary>
        /// Gesamtanzahl Log-Einträge [ATV]
        /// </summary>
        public int TotalLogs { get; set; }
        
        /// <summary>
        /// Anzahl aktive Sessions [ATV]
        /// </summary>
        public int ActiveSessions { get; set; }
        
        /// <summary>
        /// System-Laufzeit in Millisekunden [ATV]
        /// </summary>
        public long UptimeMs { get; set; }
        
        /// <summary>
        /// Durchschnittliche Benchmark-Zeit in Millisekunden [WMM]
        /// </summary>
        public double AverageBenchmarkTimeMs { get; set; }
    }
}
