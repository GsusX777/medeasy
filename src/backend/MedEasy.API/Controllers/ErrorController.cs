// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Controller für zentrale Fehlerbehandlung [ECP][NSF]
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/error")]
        public IActionResult Error()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            
            if (exception != null)
            {
                // Vollständiger Kontext wird für Diagnose protokolliert [ECP]
                _logger.LogError(exception, "Unbehandelte Ausnahme: {Message} bei {Path}", 
                    exception.Message, HttpContext.Request.Path);
            }
            
            // Sichere Fehlerbehandlung ohne Preisgabe sensibler Informationen [ZTS]
            return Problem(
                title: "Ein interner Serverfehler ist aufgetreten.",
                statusCode: 500,
                instance: HttpContext.Request.Path,
                detail: null // Keine Details in der Produktion [ZTS]
            );
        }
    }
}
