# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Swiss-specific anonymization rules for MedEasy.
Implements custom recognizers for Swiss healthcare data.

[SF] Swiss formats (insurance numbers, etc.)
[DSC] Swiss data protection compliance
[MFD] Medical terminology DE-CH
"""

import re
from typing import List, Optional, Pattern

import structlog
from presidio_analyzer import EntityRecognizer, RecognizerResult

logger = structlog.get_logger()


class SwissMedicalEntityRecognizer(EntityRecognizer):
    """
    Custom entity recognizer for Swiss medical data.
    Detects Swiss-specific formats like AHV numbers, insurance numbers, etc.
    
    [SF] Implements Swiss formats detection
    """
    
    def __init__(self):
        """Initialize the Swiss medical entity recognizer."""
        supported_entities = [
            "SWISS_AHV",  # Swiss social security number
            "SWISS_INSURANCE",  # Swiss insurance number
            "MEDICAL_LICENSE",  # Swiss medical license number
            "MEDICAL_RECORD",  # Medical record number
            "HOSPITAL_PATIENT_ID",  # Hospital patient ID
        ]
        
        # Call parent init
        super().__init__(
            supported_entities=supported_entities,
            name="SwissMedicalEntityRecognizer",
            supported_language="de",
        )
        
        # Compile regex patterns
        self.patterns = {
            "SWISS_AHV": re.compile(r'756\.\d{4}\.\d{4}\.\d{2}'),  # Format: 756.XXXX.XXXX.XX
            "SWISS_INSURANCE": re.compile(r'\d{3}\.\d{4}\.\d{4}\.\d{2}'),  # Format: XXX.XXXX.XXXX.XX
            "MEDICAL_LICENSE": re.compile(r'[A-Z]{2}-\d{6}'),  # Format: XX-XXXXXX (e.g., FMH license)
            "MEDICAL_RECORD": re.compile(r'MR-\d{8}'),  # Format: MR-XXXXXXXX
            "HOSPITAL_PATIENT_ID": re.compile(r'P-\d{6}-\d{2}'),  # Format: P-XXXXXX-XX
        }
        
        # Swiss medical terminology mapping (German to Swiss German)
        # [MFD] Medical terminology DE-CH
        self.medical_terms_mapping = {
            "Krankenhaus": "Spital",
            "Arzt": "Doktor",
            "Arzneimittel": "Medikament",
            "Krankenschwester": "Pflegefachperson",
            "Krankenpfleger": "Pflegefachperson",
            "Notaufnahme": "Notfall",
            "Termin": "Konsultation",
            "Rezept": "Verschreibung",
            "Überweisung": "Zuweisung",
        }
        
        logger.info(
            "Swiss Medical Entity Recognizer initialized",
            supported_entities=supported_entities,
        )
    
    def analyze(self, text: str, entities: List[str], nlp_artifacts=None) -> List[RecognizerResult]:
        """
        Analyze text for Swiss-specific medical entities.
        
        Args:
            text: Text to analyze
            entities: List of entities to look for
            nlp_artifacts: NLP artifacts (not used)
            
        Returns:
            List of recognition results
        """
        results = []
        
        # Check each entity type
        for entity_type in entities:
            if entity_type in self.supported_entities:
                pattern = self.patterns.get(entity_type)
                if pattern:
                    # Find all matches
                    for match in pattern.finditer(text):
                        start, end = match.span()
                        results.append(
                            RecognizerResult(
                                entity_type=entity_type,
                                start=start,
                                end=end,
                                score=0.85,  # High confidence for regex matches
                            )
                        )
        
        return results
    
    def validate_result(self, pattern_text: str) -> Optional[bool]:
        """
        Validate if the pattern matches a specific format.
        
        Args:
            pattern_text: Text to validate
            
        Returns:
            True if valid, False if invalid, None if unknown
        """
        # Validate Swiss AHV number (social security)
        if re.match(self.patterns["SWISS_AHV"], pattern_text):
            # Check if it starts with 756 (Swiss prefix)
            if pattern_text.startswith("756."):
                # Additional validation could be added here (checksum, etc.)
                return True
            return False
        
        # Validate Swiss insurance number
        if re.match(self.patterns["SWISS_INSURANCE"], pattern_text):
            # Additional validation could be added here
            return True
        
        # For other patterns, just return None (unknown)
        return None


class SwissGermanDetector:
    """
    Detector for Swiss German dialect in medical text.
    Used to provide warnings about potential transcription inaccuracies.
    
    [SDH] Swiss German handling
    """
    
    def __init__(self):
        """Initialize the Swiss German detector."""
        # Common Swiss German words and patterns
        self.swiss_german_patterns: List[Pattern] = [
            re.compile(r'\b(nöd|hoi|grüezi|merci|velo|znacht|zmorge|zmittag)\b', re.IGNORECASE),
            re.compile(r'\b(isch|het|hät|gsi|gsii|gseh)\b', re.IGNORECASE),
            re.compile(r'\b(chind|chrank|chrankheit|chopfweh)\b', re.IGNORECASE),
            re.compile(r'\b(spital|dokter|schwöschter)\b', re.IGNORECASE),
        ]
        
        # Swiss German medical terms
        self.swiss_german_medical_terms: List[str] = [
            "Chopfweh",  # Kopfschmerzen (headache)
            "Buchweh",   # Bauchschmerzen (stomach ache)
            "Fieber",    # Fieber (fever)
            "Schnupfe",  # Schnupfen (cold)
            "Hueschte",  # Husten (cough)
            "Schwindel", # Schwindel (dizziness)
            "Übelkeit",  # Übelkeit (nausea)
            "Glieder",   # Gliederschmerzen (body aches)
        ]
    
    def detect(self, text: str) -> bool:
        """
        Detect if text contains Swiss German dialect.
        
        Args:
            text: Text to analyze
            
        Returns:
            True if Swiss German is detected, False otherwise
        """
        # Check for Swiss German patterns
        for pattern in self.swiss_german_patterns:
            if pattern.search(text):
                return True
        
        # Check for Swiss German medical terms
        for term in self.swiss_german_medical_terms:
            if term.lower() in text.lower():
                return True
        
        return False
    
    def get_confidence(self, text: str) -> float:
        """
        Get confidence score for Swiss German detection.
        
        Args:
            text: Text to analyze
            
        Returns:
            Confidence score (0.0-1.0)
        """
        # Count matches
        match_count = 0
        total_patterns = len(self.swiss_german_patterns) + len(self.swiss_german_medical_terms)
        
        # Check patterns
        for pattern in self.swiss_german_patterns:
            if pattern.search(text):
                match_count += 1
        
        # Check terms
        for term in self.swiss_german_medical_terms:
            if term.lower() in text.lower():
                match_count += 1
        
        # Calculate confidence
        return min(1.0, match_count / max(1, total_patterns))
