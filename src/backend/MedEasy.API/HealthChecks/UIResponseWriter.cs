// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MedEasy.API.HealthChecks
{
    /// <summary>
    /// Schreibt Health-Check-Ergebnisse im JSON-Format [MPR]
    /// </summary>
    public static class UIResponseWriter
    {
        /// <summary>
        /// Schreibt Health-Check-Ergebnisse im JSON-Format
        /// </summary>
        public static Task WriteHealthCheckUIResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                timestamp = DateTime.UtcNow,
                entries = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds,
                    description = e.Value.Description,
                    data = e.Value.Data
                })
            };
            
            return context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
