// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using System;

namespace MedEasy.Application.DTOs
{
    /// <summary>
    /// DTO für System-Performance-Metriken [PSF][ZTS]
    /// </summary>
    public class SystemPerformanceDto
    {
        /// <summary>
        /// CPU-Auslastung in Prozent (0-100)
        /// </summary>
        public double CpuUsage { get; set; }

        /// <summary>
        /// RAM-Auslastung in Prozent (0-100)
        /// </summary>
        public double RamUsage { get; set; }

        /// <summary>
        /// GPU-Auslastung in Prozent (0-100), falls verfügbar
        /// </summary>
        public double? GpuUsage { get; set; }

        /// <summary>
        /// Ist GPU-Beschleunigung verfügbar
        /// </summary>
        public bool GpuAcceleration { get; set; }

        /// <summary>
        /// Disk I/O in MB/s
        /// </summary>
        public double DiskIo { get; set; }

        /// <summary>
        /// Netzwerk-Latenz zum Backend in ms
        /// </summary>
        public int NetworkLatency { get; set; }

        /// <summary>
        /// Zeitstempel der Messung
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gesamter verfügbarer RAM in MB
        /// </summary>
        public long TotalRamMb { get; set; }

        /// <summary>
        /// Verwendeter RAM in MB
        /// </summary>
        public long UsedRamMb { get; set; }

        /// <summary>
        /// GPU-Name, falls verfügbar
        /// </summary>
        public string? GpuName { get; set; }

        /// <summary>
        /// CPU-Name
        /// </summary>
        public string CpuName { get; set; } = string.Empty;

        /// <summary>
        /// Anzahl CPU-Kerne
        /// </summary>
        public int CpuCores { get; set; }
    }
}
