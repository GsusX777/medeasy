# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Base AI provider interface for MedEasy.
Defines the common interface for all AI providers and implements provider selection.

[PK] Provider chain implementation
[CT] Cloud transparency
[NDW] Never diagnose without warning
"""

import abc
from enum import Enum
from typing import Any, Dict, List, Optional, Union

import structlog

from src.config import ProviderType, config

logger = structlog.get_logger()


class AnalysisType(str, Enum):
    """Types of text analysis supported by providers."""
    SUMMARIZE = "summarize"
    EXTRACT_SYMPTOMS = "extract_symptoms"
    SUGGEST_DIAGNOSIS = "suggest_diagnosis"
    EXTRACT_MEDICATIONS = "extract_medications"
    EXTRACT_PROCEDURES = "extract_procedures"
    GENERAL_QUERY = "general_query"


class BaseProvider(abc.ABC):
    """
    Abstract base class for AI providers.
    All providers must implement this interface.
    """
    
    def __init__(self, provider_type: ProviderType):
        """
        Initialize the provider.
        
        Args:
            provider_type: Type of this provider
        """
        self.provider_type = provider_type
        self.is_cloud_based = provider_type != ProviderType.LOCAL
        
        # [CT] Log whether this is cloud or local processing
        logger.info(
            "Initializing AI provider",
            provider=provider_type,
            is_cloud_based=self.is_cloud_based,
        )
    
    @abc.abstractmethod
    def analyze_text(
        self, text: str, analysis_type: str, options: Optional[Dict[str, Any]] = None
    ) -> str:
        """
        Analyze medical text.
        
        Args:
            text: Text to analyze
            analysis_type: Type of analysis to perform
            options: Additional options for the analysis
            
        Returns:
            Analysis result as text
        """
        pass
    
    def get_disclaimer(self, analysis_type: str) -> str:
        """
        [NDW] Get appropriate disclaimer for the analysis type.
        Never diagnose without warning.
        
        Args:
            analysis_type: Type of analysis
            
        Returns:
            Disclaimer text
        """
        if analysis_type == AnalysisType.SUGGEST_DIAGNOSIS:
            return (
                "WICHTIGER HINWEIS: Diese KI-generierten Diagnosevorschläge dienen nur als "
                "Unterstützung und ersetzen nicht die ärztliche Beurteilung. Alle Vorschläge "
                "müssen von medizinischem Fachpersonal überprüft und bestätigt werden."
            )
        elif analysis_type == AnalysisType.EXTRACT_MEDICATIONS:
            return (
                "HINWEIS: Diese Medikamentenliste wurde automatisch extrahiert und muss "
                "von medizinischem Fachpersonal überprüft werden."
            )
        else:
            return (
                "HINWEIS: Diese Informationen wurden automatisch generiert und dienen "
                "nur als Unterstützung für medizinisches Fachpersonal."
            )
    
    def is_available(self) -> bool:
        """
        Check if the provider is available.
        
        Returns:
            True if available, False otherwise
        """
        try:
            # Simple availability check
            self.analyze_text(
                text="Test",
                analysis_type=AnalysisType.GENERAL_QUERY,
                options={"max_tokens": 5},
            )
            return True
        except Exception as e:
            logger.warning(
                "Provider availability check failed",
                provider=self.provider_type,
                error=str(e),
            )
            return False


def get_provider(provider_type: Union[str, ProviderType]) -> BaseProvider:
    """
    Get an instance of the specified provider.
    
    Args:
        provider_type: Type of provider to get
        
    Returns:
        Provider instance
        
    Raises:
        ValueError: If provider type is invalid or provider is not available
    """
    # Convert string to enum if needed
    if isinstance(provider_type, str):
        try:
            provider_type = ProviderType(provider_type)
        except ValueError:
            raise ValueError(f"Invalid provider type: {provider_type}")
    
    # Import and create the appropriate provider
    if provider_type == ProviderType.OPENAI:
        from src.providers.openai_client import OpenAIProvider
        return OpenAIProvider()
    elif provider_type == ProviderType.ANTHROPIC:
        # Dynamic import to avoid dependency issues if not installed
        try:
            from src.providers.anthropic_client import AnthropicProvider
            return AnthropicProvider()
        except (ImportError, ModuleNotFoundError):
            logger.error("Anthropic provider requested but not available")
            raise ValueError("Anthropic provider not available")
    elif provider_type == ProviderType.GOOGLE:
        # Dynamic import to avoid dependency issues if not installed
        try:
            from src.providers.google_client import GoogleProvider
            return GoogleProvider()
        except (ImportError, ModuleNotFoundError):
            logger.error("Google provider requested but not available")
            raise ValueError("Google provider not available")
    elif provider_type == ProviderType.LOCAL:
        # Dynamic import to avoid dependency issues if not installed
        try:
            from src.providers.local_client import LocalProvider
            return LocalProvider()
        except (ImportError, ModuleNotFoundError):
            logger.error("Local provider requested but not available")
            raise ValueError("Local provider not available")
    else:
        raise ValueError(f"Unknown provider type: {provider_type}")


def get_available_providers() -> List[ProviderType]:
    """
    Get list of available providers.
    
    Returns:
        List of available provider types
    """
    available_providers = []
    
    # Check OpenAI
    if config.provider.openai_api_key:
        try:
            from src.providers.openai_client import OpenAIProvider
            provider = OpenAIProvider()
            if provider.is_available():
                available_providers.append(ProviderType.OPENAI)
        except Exception:
            pass
    
    # Check Anthropic
    if config.provider.anthropic_api_key:
        try:
            from src.providers.anthropic_client import AnthropicProvider
            provider = AnthropicProvider()
            if provider.is_available():
                available_providers.append(ProviderType.ANTHROPIC)
        except Exception:
            pass
    
    # Check Google
    if config.provider.google_api_key:
        try:
            from src.providers.google_client import GoogleProvider
            provider = GoogleProvider()
            if provider.is_available():
                available_providers.append(ProviderType.GOOGLE)
        except Exception:
            pass
    
    # Check Local
    try:
        from src.providers.local_client import LocalProvider
        provider = LocalProvider()
        if provider.is_available():
            available_providers.append(ProviderType.LOCAL)
    except Exception:
        pass
    
    return available_providers
