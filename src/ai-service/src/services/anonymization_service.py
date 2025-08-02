# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Anonymization Service for MedEasy AI Service.

[AIU] Anonymization is mandatory and cannot be disabled
[SP] Secure processing of sensitive data
[ATV] Audit logging for all anonymization operations
[SF] Swiss-specific entity recognition
"""

import re
import time
from dataclasses import dataclass
from typing import List, Dict, Tuple

import structlog

from src.config import config

logger = structlog.get_logger()


@dataclass
class AnonymizedEntity:
    """Detected and anonymized entity."""
    original: str
    anonymized: str
    entity_type: str
    confidence: float
    start_pos: int
    end_pos: int


@dataclass
class AnonymizationResult:
    """Result of text anonymization."""
    text: str
    entities: List[AnonymizedEntity]
    status: str  # "completed", "review_required"
    confidence_score: float


class AnonymizationService:
    """
    Anonymization Service for medical text processing.
    
    [AIU] Mandatory anonymization - cannot be disabled
    [SF] Swiss-specific patterns and formats
    [SP] Secure processing of sensitive data
    """
    
    def __init__(self):
        """Initialize the anonymization service."""
        self.entity_counter = {}
        logger.info("AnonymizationService initialized")
    
    def _get_swiss_patterns(self) -> Dict[str, str]:
        """Get Swiss-specific regex patterns for entity detection."""
        return {
            # Swiss insurance number: XXX.XXXX.XXXX.XX
            "insurance_number": r"\b\d{3}\.\d{4}\.\d{4}\.\d{2}\b",
            
            # Swiss phone numbers
            "phone": r"(\+41|0041|0)\s?(\d{2})\s?(\d{3})\s?(\d{2})\s?(\d{2})",
            
            # Swiss postal codes
            "postal_code": r"\b[1-9]\d{3}\b",
            
            # Names (simplified pattern)
            "name": r"\b[A-ZÄÖÜ][a-zäöüß]+(?:\s+[A-ZÄÖÜ][a-zäöüß]+)*\b",
            
            # Dates (Swiss format DD.MM.YYYY)
            "date": r"\b\d{1,2}\.\d{1,2}\.\d{4}\b",
            
            # Email addresses
            "email": r"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
            
            # Medical record numbers
            "medical_id": r"\b(MR|PAT|ID)[-_]?\d{4,}\b",
        }
    
    def _detect_entities(self, text: str) -> List[Tuple[str, str, int, int, float]]:
        """
        Detect entities in text using Swiss-specific patterns.
        
        Returns: List of (original, entity_type, start, end, confidence)
        """
        entities = []
        patterns = self._get_swiss_patterns()
        
        for entity_type, pattern in patterns.items():
            matches = re.finditer(pattern, text, re.IGNORECASE)
            
            for match in matches:
                original = match.group()
                start_pos = match.start()
                end_pos = match.end()
                
                # Calculate confidence based on pattern specificity
                confidence = self._calculate_confidence(original, entity_type)
                
                entities.append((original, entity_type, start_pos, end_pos, confidence))
        
        # Sort by position to maintain text structure
        entities.sort(key=lambda x: x[2])
        
        return entities
    
    def _calculate_confidence(self, text: str, entity_type: str) -> float:
        """Calculate confidence score for detected entity."""
        if entity_type == "insurance_number":
            # Swiss insurance numbers have specific format
            return 0.95
        elif entity_type == "phone":
            # Phone numbers are quite specific
            return 0.90
        elif entity_type == "email":
            # Email format is very specific
            return 0.95
        elif entity_type == "date":
            # Date format is specific but could be false positive
            return 0.80
        elif entity_type == "postal_code":
            # Swiss postal codes are specific
            return 0.85
        elif entity_type == "medical_id":
            # Medical IDs are quite specific
            return 0.90
        elif entity_type == "name":
            # Names are harder to detect accurately
            return 0.60
        else:
            return 0.50
    
    def _anonymize_entity(self, original: str, entity_type: str) -> str:
        """Generate anonymized replacement for entity."""
        # Get or create counter for this entity type
        if entity_type not in self.entity_counter:
            self.entity_counter[entity_type] = 0
        
        self.entity_counter[entity_type] += 1
        counter = self.entity_counter[entity_type]
        
        # Generate anonymized replacement
        if entity_type == "insurance_number":
            return f"[VERSICHERUNG-{counter:03d}]"
        elif entity_type == "phone":
            return f"[TELEFON-{counter:03d}]"
        elif entity_type == "email":
            return f"[EMAIL-{counter:03d}]"
        elif entity_type == "date":
            return f"[DATUM-{counter:03d}]"
        elif entity_type == "postal_code":
            return f"[PLZ-{counter:03d}]"
        elif entity_type == "medical_id":
            return f"[PATIENT-ID-{counter:03d}]"
        elif entity_type == "name":
            return f"[NAME-{counter:03d}]"
        else:
            return f"[{entity_type.upper()}-{counter:03d}]"
    
    async def anonymize_text(
        self,
        text: str,
        confidence_threshold: float = 0.8
    ) -> AnonymizationResult:
        """
        Anonymize text by detecting and replacing sensitive entities.
        
        [AIU] Mandatory anonymization - cannot be disabled
        [SF] Uses Swiss-specific patterns
        [ATV] Logs all anonymization operations
        """
        request_id = f"anon_{int(time.time() * 1000)}"
        
        try:
            logger.info(
                "Starting text anonymization",
                request_id=request_id,
                text_length=len(text),
                confidence_threshold=confidence_threshold
            )
            
            # Detect entities
            detected_entities = self._detect_entities(text)
            
            # Process entities and build anonymized text
            anonymized_text = text
            anonymized_entities = []
            offset = 0  # Track position changes due to replacements
            
            for original, entity_type, start_pos, end_pos, confidence in detected_entities:
                # Adjust positions for previous replacements
                adjusted_start = start_pos + offset
                adjusted_end = end_pos + offset
                
                # Generate anonymized replacement
                anonymized = self._anonymize_entity(original, entity_type)
                
                # Replace in text
                anonymized_text = (
                    anonymized_text[:adjusted_start] + 
                    anonymized + 
                    anonymized_text[adjusted_end:]
                )
                
                # Update offset
                offset += len(anonymized) - len(original)
                
                # Store entity info
                anonymized_entities.append(AnonymizedEntity(
                    original=original,
                    anonymized=anonymized,
                    entity_type=entity_type,
                    confidence=confidence,
                    start_pos=start_pos,
                    end_pos=end_pos
                ))
            
            # Calculate overall confidence
            if anonymized_entities:
                avg_confidence = sum(e.confidence for e in anonymized_entities) / len(anonymized_entities)
            else:
                avg_confidence = 1.0  # No entities found = high confidence
            
            # Determine status
            low_confidence_entities = [e for e in anonymized_entities if e.confidence < confidence_threshold]
            status = "review_required" if low_confidence_entities else "completed"
            
            logger.info(
                "Text anonymization completed",
                request_id=request_id,
                entities_found=len(anonymized_entities),
                low_confidence_entities=len(low_confidence_entities),
                status=status,
                avg_confidence=avg_confidence
            )
            
            return AnonymizationResult(
                text=anonymized_text,
                entities=anonymized_entities,
                status=status,
                confidence_score=avg_confidence
            )
            
        except Exception as e:
            logger.error(
                "Anonymization failed",
                request_id=request_id,
                error=str(e)
            )
            raise
