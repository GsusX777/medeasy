# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
OpenAI provider implementation for MedEasy.
Provides integration with OpenAI's API for text analysis.

[PK] Primary provider in the provider chain
[CT] Cloud transparency
[NDW] Never diagnose without warning
[NEA] Never use real API keys in code
"""

import os
import time
from typing import Any, Dict, Optional

import openai
import structlog

from src.config import ProviderType, config
from src.providers.base_provider import AnalysisType, BaseProvider

logger = structlog.get_logger()


class OpenAIProvider(BaseProvider):
    """
    OpenAI provider implementation.
    Integrates with OpenAI's API for text analysis.
    """
    
    def __init__(self):
        """Initialize the OpenAI provider."""
        super().__init__(ProviderType.OPENAI)
        
        # [NEA] Get API key from environment, never hardcode
        api_key = config.provider.openai_api_key
        if not api_key:
            raise ValueError("OpenAI API key not configured")
        
        # Initialize client
        self.client = openai.OpenAI(api_key=api_key)
        
        # Model configuration
        self.models = {
            "default": "gpt-4o",
            "fast": "gpt-3.5-turbo",
            "vision": "gpt-4-vision-preview",
        }
        
        logger.info(
            "OpenAI provider initialized",
            default_model=self.models["default"],
        )
    
    def analyze_text(
        self, text: str, analysis_type: str, options: Optional[Dict[str, Any]] = None
    ) -> str:
        """
        Analyze medical text using OpenAI.
        
        Args:
            text: Text to analyze
            analysis_type: Type of analysis to perform
            options: Additional options for the analysis
            
        Returns:
            Analysis result as text
        """
        start_time = time.time()
        options = options or {}
        
        # Select model
        model = options.get("model", self.models["default"])
        
        # Set up system prompt based on analysis type
        system_prompt = self._get_system_prompt(analysis_type)
        
        # Add Swiss German handling if needed
        if options.get("is_swiss_german", False):
            system_prompt += "\nDer Text enthält möglicherweise Schweizerdeutsch. Berücksichtige dies bei der Analyse."
        
        # [NDW] Add disclaimer for diagnosis
        disclaimer = self.get_disclaimer(analysis_type)
        
        try:
            # Call OpenAI API
            response = self.client.chat.completions.create(
                model=model,
                messages=[
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": text}
                ],
                temperature=options.get("temperature", 0.1),  # Low temperature for medical analysis
                max_tokens=options.get("max_tokens", 1000),
                top_p=options.get("top_p", 0.95),
                frequency_penalty=options.get("frequency_penalty", 0.0),
                presence_penalty=options.get("presence_penalty", 0.0),
            )
            
            # Extract result
            result = response.choices[0].message.content
            
            # Add disclaimer to result
            result = f"{result}\n\n{disclaimer}"
            
            # Log performance
            processing_time = time.time() - start_time
            logger.info(
                "OpenAI analysis completed",
                analysis_type=analysis_type,
                model=model,
                processing_time=processing_time,
                token_usage=response.usage.total_tokens if hasattr(response, 'usage') else None,
            )
            
            return result
            
        except Exception as e:
            logger.error(
                "OpenAI analysis failed",
                analysis_type=analysis_type,
                model=model,
                error=str(e),
                exc_info=True,
            )
            raise
    
    def _get_system_prompt(self, analysis_type: str) -> str:
        """
        Get system prompt for the specified analysis type.
        
        Args:
            analysis_type: Type of analysis
            
        Returns:
            System prompt
        """
        # Base medical context
        base_prompt = (
            "Du bist ein medizinischer KI-Assistent für Schweizer Ärzte. "
            "Verwende medizinische Fachbegriffe und halte dich an Schweizer medizinische Standards. "
            "Deine Antworten müssen präzise, evidenzbasiert und auf Hochdeutsch sein. "
            "Formatiere deine Antworten klar und strukturiert."
        )
        
        # Analysis-specific prompts
        if analysis_type == AnalysisType.SUMMARIZE:
            return base_prompt + (
                "\n\nFasse den medizinischen Text zusammen. "
                "Strukturiere die Zusammenfassung in: "
                "1. Hauptbeschwerden "
                "2. Relevante Anamnese "
                "3. Wichtige Befunde "
                "4. Empfohlene Maßnahmen"
            )
        elif analysis_type == AnalysisType.EXTRACT_SYMPTOMS:
            return base_prompt + (
                "\n\nExtrahiere alle Symptome aus dem Text. "
                "Gib für jedes Symptom an: "
                "- Beschreibung "
                "- Dauer (falls erwähnt) "
                "- Schweregrad (falls erwähnt) "
                "- Zusammenhang mit anderen Symptomen (falls erwähnt)"
            )
        elif analysis_type == AnalysisType.SUGGEST_DIAGNOSIS:
            return base_prompt + (
                "\n\nSchlage mögliche Diagnosen basierend auf den beschriebenen Symptomen vor. "
                "WICHTIG: Beginne mit einem deutlichen Hinweis, dass dies nur Vorschläge sind und "
                "eine ärztliche Beurteilung erforderlich ist. "
                "Für jede mögliche Diagnose gib an: "
                "- Name der Diagnose "
                "- Passende Symptome aus dem Text "
                "- Fehlende typische Symptome "
                "- Empfohlene weitere Untersuchungen"
            )
        elif analysis_type == AnalysisType.EXTRACT_MEDICATIONS:
            return base_prompt + (
                "\n\nExtrahiere alle Medikamente aus dem Text. "
                "Für jedes Medikament gib an: "
                "- Name (Wirkstoff und Handelsname, falls erwähnt) "
                "- Dosierung (falls erwähnt) "
                "- Anwendungszeitraum (falls erwähnt) "
                "- Zweck/Indikation (falls erwähnt)"
            )
        elif analysis_type == AnalysisType.EXTRACT_PROCEDURES:
            return base_prompt + (
                "\n\nExtrahiere alle medizinischen Verfahren und Untersuchungen aus dem Text. "
                "Für jedes Verfahren gib an: "
                "- Name des Verfahrens "
                "- Zweck/Indikation (falls erwähnt) "
                "- Ergebnisse (falls erwähnt) "
                "- Zeitpunkt (falls erwähnt)"
            )
        else:  # GENERAL_QUERY
            return base_prompt + (
                "\n\nBeantworte die medizinische Anfrage basierend auf dem Text. "
                "Halte dich strikt an die Informationen im Text und vermeide Spekulationen. "
                "Wenn Informationen fehlen, weise darauf hin."
            )
