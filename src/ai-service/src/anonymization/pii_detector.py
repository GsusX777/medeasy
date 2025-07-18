# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
PII detection and anonymization for MedEasy.
Provides detection and anonymization of personally identifiable information.

[AIU] Anonymization is MANDATORY and cannot be disabled
[ARQ] Anonymization Review Queue implementation
[DSC] Swiss data protection compliance
"""

import re
import time
import uuid
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Set, Tuple

import structlog
from presidio_analyzer import AnalyzerEngine, RecognizerRegistry
from presidio_analyzer.nlp_engine import NlpEngineProvider
from presidio_anonymizer import AnonymizerEngine
from presidio_anonymizer.entities import OperatorConfig

from src.anonymization.rules import SwissMedicalEntityRecognizer
from src.config import config

logger = structlog.get_logger()


@dataclass
class Entity:
    """Detected PII entity."""
    entity_id: str
    entity_type: str
    text: str
    start: int
    end: int
    score: float
    anonymized_text: Optional[str] = None


@dataclass
class AnonymizationResult:
    """Result of anonymization operation."""
    original_text: str
    anonymized_text: str
    entities: List[Entity]
    processing_time: float


class ReviewQueue:
    """
    [ARQ] Anonymization Review Queue implementation.
    Stores entities that need manual review due to low confidence.
    """
    
    def __init__(self, max_size: int = 100):
        """Initialize the review queue."""
        self.queue: Dict[str, Entity] = {}
        self.max_size = max_size
    
    def add(self, entity: Entity) -> bool:
        """
        Add an entity to the review queue.
        
        Args:
            entity: Entity to add
            
        Returns:
            True if added, False if queue is full
        """
        if len(self.queue) >= self.max_size:
            logger.warning(
                "Review queue is full",
                queue_size=len(self.queue),
                max_size=self.max_size,
            )
            return False
        
        self.queue[entity.entity_id] = entity
        return True
    
    def get(self, entity_id: str) -> Optional[Entity]:
        """Get an entity from the review queue."""
        return self.queue.get(entity_id)
    
    def remove(self, entity_id: str) -> bool:
        """Remove an entity from the review queue."""
        if entity_id in self.queue:
            del self.queue[entity_id]
            return True
        return False
    
    def get_all(self) -> List[Entity]:
        """Get all entities in the review queue."""
        return list(self.queue.values())
    
    def size(self) -> int:
        """Get the current size of the review queue."""
        return len(self.queue)


class PIIDetector:
    """
    PII detection and anonymization service.
    
    [AIU] Anonymization is MANDATORY and cannot be disabled
    [DSC] Swiss data protection compliance
    """
    
    def __init__(self):
        """Initialize the PII detector."""
        # [AIU] Ensure anonymization is enabled - cannot be disabled
        if not config.anonymization.enabled:
            raise ValueError("[AIU] Anonymization cannot be disabled")
        
        # Initialize NLP engine with German language model
        logger.info("Initializing NLP engine with German language model")
        nlp_engine_provider = NlpEngineProvider(nlp_configuration={"models": [{"lang_code": "de", "model_name": "de_core_news_lg"}]})
        nlp_engine = nlp_engine_provider.create_engine()
        
        # Initialize recognizer registry
        registry = RecognizerRegistry()
        registry.load_predefined_recognizers(languages=["de"])
        
        # Add Swiss-specific recognizers
        swiss_recognizer = SwissMedicalEntityRecognizer()
        registry.add_recognizer(swiss_recognizer)
        
        # Initialize analyzer and anonymizer engines
        self.analyzer = AnalyzerEngine(
            nlp_engine=nlp_engine,
            registry=registry,
        )
        self.anonymizer = AnonymizerEngine()
        
        # Initialize review queue
        self.review_queue = ReviewQueue(max_size=config.anonymization.review_queue_size)
        
        # Medical terms whitelist (terms that should not be anonymized)
        self.medical_whitelist: Set[str] = set([
            "diabetes", "hypertonie", "asthma", "migräne", "arthritis",
            "depression", "angststörung", "herzinsuffizienz", "copd",
            "parkinson", "alzheimer", "multiple sklerose", "epilepsie",
        ])
        
        logger.info(
            "PII detector initialized",
            confidence_threshold=config.anonymization.confidence_threshold,
            review_queue_size=config.anonymization.review_queue_size,
        )
    
    def detect_entities(self, text: str) -> List[Entity]:
        """
        Detect PII entities in text.
        
        Args:
            text: Text to analyze
            
        Returns:
            List of detected entities
        """
        # Analyze text with Presidio
        analyzer_results = self.analyzer.analyze(
            text=text,
            language="de",
            entities=None,  # Detect all entity types
            allow_list=self.medical_whitelist,
        )
        
        # Convert to our Entity model
        entities = []
        for result in analyzer_results:
            entity_id = str(uuid.uuid4())
            entity = Entity(
                entity_id=entity_id,
                entity_type=result.entity_type,
                text=text[result.start:result.end],
                start=result.start,
                end=result.end,
                score=result.score,
            )
            entities.append(entity)
            
            # Add to review queue if confidence is below threshold
            if entity.score < config.anonymization.confidence_threshold:
                self.review_queue.add(entity)
        
        return entities
    
    def anonymize_text(self, text: str, entities: List[Entity]) -> str:
        """
        Anonymize text based on detected entities.
        
        Args:
            text: Original text
            entities: Detected entities
            
        Returns:
            Anonymized text
        """
        # Convert entities to Presidio format
        analyzer_results = []
        for entity in entities:
            analyzer_results.append({
                "entity_type": entity.entity_type,
                "start": entity.start,
                "end": entity.end,
                "score": entity.score,
            })
        
        # Anonymize with Presidio
        anonymizer_results = self.anonymizer.anonymize(
            text=text,
            analyzer_results=analyzer_results,
            operators={
                "PERSON": OperatorConfig("replace", {"new_value": "[PERSON]"}),
                "LOCATION": OperatorConfig("replace", {"new_value": "[ORT]"}),
                "DATE_TIME": OperatorConfig("replace", {"new_value": "[DATUM]"}),
                "PHONE_NUMBER": OperatorConfig("replace", {"new_value": "[TELEFON]"}),
                "EMAIL_ADDRESS": OperatorConfig("replace", {"new_value": "[EMAIL]"}),
                "MEDICAL_LICENSE": OperatorConfig("replace", {"new_value": "[ARZT-ID]"}),
                "SWISS_AHV": OperatorConfig("replace", {"new_value": "[AHV-NUMMER]"}),
                "SWISS_INSURANCE": OperatorConfig("replace", {"new_value": "[VERSICHERUNG]"}),
                "MEDICAL_RECORD": OperatorConfig("replace", {"new_value": "[PATIENTENAKTE]"}),
                "DEFAULT": OperatorConfig("replace", {"new_value": "[VERTRAULICH]"}),
            },
        )
        
        # Update entities with anonymized text
        anonymized_text = anonymizer_results.text
        for i, entity in enumerate(entities):
            if i < len(anonymizer_results.items):
                entity.anonymized_text = anonymizer_results.items[i].replace_text
        
        return anonymized_text
    
    def detect_and_anonymize(self, text: str) -> Tuple[str, List[Entity]]:
        """
        Detect and anonymize PII in text.
        
        Args:
            text: Text to anonymize
            
        Returns:
            Tuple of (anonymized text, detected entities)
        """
        start_time = time.time()
        
        # [AIU] Anonymization is mandatory - cannot be disabled
        if not config.anonymization.enabled:
            raise ValueError("[AIU] Anonymization cannot be disabled")
        
        # Detect entities
        entities = self.detect_entities(text)
        
        # Anonymize text
        anonymized_text = self.anonymize_text(text, entities)
        
        processing_time = time.time() - start_time
        logger.info(
            "Text anonymized",
            text_length=len(text),
            entity_count=len(entities),
            processing_time=processing_time,
            review_queue_size=self.review_queue.size(),
        )
        
        return anonymized_text, entities
    
    def update_entity_decision(
        self, entity_id: str, approved: bool, replacement_text: Optional[str] = None
    ) -> Optional[Entity]:
        """
        Update an entity based on review decision.
        
        Args:
            entity_id: ID of the entity to update
            approved: Whether the anonymization is approved
            replacement_text: Optional replacement text
            
        Returns:
            Updated entity or None if not found
        """
        entity = self.review_queue.get(entity_id)
        if not entity:
            logger.warning(
                "Entity not found in review queue",
                entity_id=entity_id,
            )
            return None
        
        if approved:
            # Update anonymized text if provided
            if replacement_text:
                entity.anonymized_text = replacement_text
        
        # Remove from review queue
        self.review_queue.remove(entity_id)
        
        logger.info(
            "Entity decision updated",
            entity_id=entity_id,
            entity_type=entity.entity_type,
            approved=approved,
            review_queue_size=self.review_queue.size(),
        )
        
        return entity
    
    def get_review_queue_size(self) -> int:
        """Get the current size of the review queue."""
        return self.review_queue.size()
    
    def get_review_queue(self) -> List[Entity]:
        """Get all entities in the review queue."""
        return self.review_queue.get_all()
