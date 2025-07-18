// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MedEasy.API.Middleware
{
    /// <summary>
    /// Middleware zur zentralen Fehlerbehandlung [ECP][NSF]
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Vollständiger Kontext wird für Diagnose protokolliert [ECP]
                _logger.LogError(ex, "Unbehandelte Ausnahme: {Message} bei {Path}", 
                    ex.Message, context.Request.Path);
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            // Sichere Fehlerbehandlung ohne Preisgabe sensibler Informationen [ZTS]
            var statusCode = HttpStatusCode.InternalServerError;
            var errorMessage = "Ein interner Serverfehler ist aufgetreten.";
            
            // Spezifische Fehlertypen unterscheiden
            if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                errorMessage = "Nicht autorisiert.";
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                errorMessage = "Die angeforderte Ressource wurde nicht gefunden.";
            }
            else if (exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                errorMessage = "Ungültige Operation.";
            }
            
            // HTTP-Status-Code setzen
            context.Response.StatusCode = (int)statusCode;
            
            // Fehler-Response erstellen (ohne Stack-Trace in Produktion) [ZTS]
            var result = JsonSerializer.Serialize(new 
            { 
                error = errorMessage,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.ToString(),
                correlationId = context.TraceIdentifier // Für Fehler-Tracking
            });
            
            return context.Response.WriteAsync(result);
        }
    }
}
