# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Swiss German dialect detector for MedEasy.

[SDH] Swiss German dialect handling
[MFD] Swiss medical terminology
[ATV] Audit logging for all operations
"""

import time
from typing import Dict, List, Optional, Tuple, NamedTuple
import re
import structlog

from src.config import config

logger = structlog.get_logger()

class DialectMarker(NamedTuple):
    """Represents a Swiss German dialect marker in text."""
    text: str
    start: int
    end: int
    standard_german: str
    confidence: float


class SwissGermanDetector:
    """
    Swiss German dialect detector.
    
    [SDH] Detects Swiss German dialect in text and provides warnings
    [MFD] Handles Swiss medical terminology differences
    """
    
    def __init__(self):
        """Initialize the Swiss German dialect detector."""
        # Load Swiss German patterns and medical terminology
        self._load_patterns()
        self._load_medical_terminology()
        
        logger.info(
            "Swiss German detector initialized",
            pattern_count=len(self.patterns),
            medical_term_count=len(self.medical_terms),
        )
    
    def _load_patterns(self):
        """Load Swiss German dialect patterns."""
        # In a real implementation, this would load from a file or database
        # For now, we'll use a simple dictionary of common Swiss German words/patterns
        self.patterns = {
            # Common Swiss German words with their Standard German equivalents
            "grüezi": {"standard": "guten tag", "confidence": 0.95},
            "merci": {"standard": "danke", "confidence": 0.8},
            "hoi": {"standard": "hallo", "confidence": 0.9},
            "velo": {"standard": "fahrrad", "confidence": 0.85},
            "spital": {"standard": "krankenhaus", "confidence": 0.9},
            "tschüss": {"standard": "auf wiedersehen", "confidence": 0.8},
            "znüni": {"standard": "zwischenmahlzeit", "confidence": 0.95},
            "rüebli": {"standard": "karotte", "confidence": 0.9},
            "bünzli": {"standard": "spießer", "confidence": 0.85},
            "chuchichäschtli": {"standard": "küchenschrank", "confidence": 0.99},
            "gummiband": {"standard": "gummiband", "confidence": 0.7},
            "parat": {"standard": "bereit", "confidence": 0.8},
            "parkieren": {"standard": "parken", "confidence": 0.85},
            "posten": {"standard": "einkaufen", "confidence": 0.8},
            "pressant": {"standard": "dringend", "confidence": 0.85},
            "beige": {"standard": "beige", "confidence": 0.7},
            "glace": {"standard": "eis", "confidence": 0.9},
            "portemonnaie": {"standard": "geldbeutel", "confidence": 0.85},
            "schoggi": {"standard": "schokolade", "confidence": 0.9},
        }
    
    def _load_medical_terminology(self):
        """Load Swiss medical terminology differences."""
        # In a real implementation, this would load from a file or database
        # For now, we'll use a simple dictionary of Swiss medical terms
        self.medical_terms = {
            # Swiss German medical terms with their Standard German equivalents
            "spital": "krankenhaus",
            "doktor": "arzt",
            "medikament": "arzneimittel",
            "apotheke": "apotheke",
            "krankenkasse": "krankenversicherung",
            "sprechstunde": "sprechstunde",
            "röntgen": "röntgen",
            "fieber": "fieber",
            "grippe": "grippe",
            "allergie": "allergie",
            "blutdruck": "blutdruck",
            "blutzucker": "blutzucker",
            "herz": "herz",
            "lunge": "lunge",
            "niere": "niere",
            "leber": "leber",
            "magen": "magen",
            "darm": "darm",
            "kopfweh": "kopfschmerzen",
            "rückenweh": "rückenschmerzen",
            "halsweh": "halsschmerzen",
        }
    
    def detect(self, text: str, include_details: bool = False) -> Tuple[bool, float, Optional[List[DialectMarker]]]:
        """
        Detect Swiss German dialect in text.
        
        Args:
            text: The text to analyze
            include_details: Whether to include detailed dialect markers
            
        Returns:
            Tuple containing:
            - is_swiss_german: Whether Swiss German was detected
            - confidence_score: Confidence score (0.0-1.0)
            - dialect_markers: List of dialect markers if include_details is True, None otherwise
        """
        start_time = time.time()
        
        # Initialize variables
        dialect_markers = [] if include_details else None
        total_confidence = 0.0
        match_count = 0
        
        # Check for Swiss German patterns
        for pattern, info in self.patterns.items():
            # Use word boundary to match whole words
            for match in re.finditer(r'\b' + re.escape(pattern) + r'\b', text.lower()):
                match_count += 1
                total_confidence += info["confidence"]
                
                if include_details:
                    dialect_markers.append(
                        DialectMarker(
                            text=match.group(),
                            start=match.start(),
                            end=match.end(),
                            standard_german=info["standard"],
                            confidence=info["confidence"],
                        )
                    )
        
        # Calculate overall confidence
        confidence_score = total_confidence / max(match_count, 1)
        
        # Determine if text is Swiss German based on match count and confidence
        is_swiss_german = match_count >= config.swiss_german.min_matches and confidence_score >= config.swiss_german.min_confidence
        
        logger.info(
            "Swiss German detection completed",
            is_swiss_german=is_swiss_german,
            confidence_score=confidence_score,
            match_count=match_count,
            processing_time_seconds=time.time() - start_time,
        )
        
        return is_swiss_german, confidence_score, dialect_markers
    
    def extract_medical_terms(self, text: str) -> List[str]:
        """
        Extract Swiss medical terminology from text.
        
        [MFD] Swiss medical terminology
        
        Args:
            text: The text to analyze
            
        Returns:
            List of Swiss medical terms found in the text
        """
        found_terms = []
        
        # Check for Swiss medical terms
        for term in self.medical_terms.keys():
            # Use word boundary to match whole words
            if re.search(r'\b' + re.escape(term) + r'\b', text.lower()):
                found_terms.append(term)
        
        logger.info(
            "Swiss medical terms extraction completed",
            term_count=len(found_terms),
        )
        
        return found_terms
