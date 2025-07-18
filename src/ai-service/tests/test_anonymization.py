# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Tests for the anonymization module.

[KP100] Anonymization requires 100% test coverage
[AIU] Tests verify that anonymization cannot be disabled
[TD] Uses synthetic test data only
"""

import pytest

from src.anonymization.pii_detector import PIIDetector
from src.anonymization.rules import SwissMedicalEntityRecognizer, SwissGermanDetector
from src.config import config


class TestPIIDetector:
    """Tests for the PII detector."""
    
    def setup_method(self):
        """Set up test environment."""
        self.detector = PIIDetector()
        
        # [TD] Synthetic test data
        self.test_texts = {
            "person": "Patient Max Muster wurde von Dr. Schmidt untersucht.",
            "contact": "Kontaktieren Sie mich unter max.muster@example.com oder 079 123 45 67.",
            "insurance": "Versicherungsnummer: 123.4567.8901.23",
            "ahv": "AHV-Nummer: 756.1234.5678.90",
            "medical": "Patient mit Diabetes mellitus Typ 2 und Hypertonie.",
            "mixed": (
                "Max Muster (geboren am 01.05.1980, AHV: 756.1234.5678.90) "
                "wurde im Kantonsspital Zürich behandelt. "
                "Kontakt: max.muster@example.com, Tel. 079 123 45 67."
            ),
        }
    
    def test_detect_entities(self):
        """Test entity detection."""
        # Test person detection
        entities = self.detector.detect_entities(self.test_texts["person"])
        assert len(entities) >= 2
        assert any(e.entity_type == "PERSON" and "Max Muster" in e.text for e in entities)
        assert any(e.entity_type == "PERSON" and "Schmidt" in e.text for e in entities)
        
        # Test contact information detection
        entities = self.detector.detect_entities(self.test_texts["contact"])
        assert len(entities) >= 2
        assert any(e.entity_type == "EMAIL_ADDRESS" for e in entities)
        assert any(e.entity_type == "PHONE_NUMBER" for e in entities)
        
        # Test insurance number detection
        entities = self.detector.detect_entities(self.test_texts["insurance"])
        assert len(entities) >= 1
        assert any(e.entity_type == "SWISS_INSURANCE" for e in entities)
        
        # Test AHV number detection
        entities = self.detector.detect_entities(self.test_texts["ahv"])
        assert len(entities) >= 1
        assert any(e.entity_type == "SWISS_AHV" for e in entities)
        
        # Test that medical terms are not detected as PII
        entities = self.detector.detect_entities(self.test_texts["medical"])
        assert not any(e.text.lower() == "diabetes" for e in entities)
        assert not any(e.text.lower() == "hypertonie" for e in entities)
    
    def test_anonymize_text(self):
        """Test text anonymization."""
        # Test anonymization of mixed text
        entities = self.detector.detect_entities(self.test_texts["mixed"])
        anonymized_text = self.detector.anonymize_text(self.test_texts["mixed"], entities)
        
        # Check that PII is anonymized
        assert "Max Muster" not in anonymized_text
        assert "756.1234.5678.90" not in anonymized_text
        assert "01.05.1980" not in anonymized_text
        assert "max.muster@example.com" not in anonymized_text
        assert "079 123 45 67" not in anonymized_text
        
        # Check that non-PII is preserved
        assert "Kantonsspital Zürich" in anonymized_text or "[ORT]" in anonymized_text
        assert "behandelt" in anonymized_text
        
        # Check replacement tokens
        assert "[PERSON]" in anonymized_text
        assert "[AHV-NUMMER]" in anonymized_text or "[VERTRAULICH]" in anonymized_text
        assert "[DATUM]" in anonymized_text
        assert "[EMAIL]" in anonymized_text
        assert "[TELEFON]" in anonymized_text
    
    def test_detect_and_anonymize(self):
        """Test combined detection and anonymization."""
        anonymized_text, entities = self.detector.detect_and_anonymize(self.test_texts["mixed"])
        
        # Check that PII is anonymized
        assert "Max Muster" not in anonymized_text
        assert "756.1234.5678.90" not in anonymized_text
        assert "max.muster@example.com" not in anonymized_text
        
        # Check that entities are returned
        assert len(entities) > 0
        assert all(hasattr(e, "entity_id") for e in entities)
        assert all(hasattr(e, "entity_type") for e in entities)
        assert all(hasattr(e, "start") for e in entities)
        assert all(hasattr(e, "end") for e in entities)
        assert all(hasattr(e, "score") for e in entities)
    
    def test_review_queue(self):
        """Test the review queue functionality."""
        # Add entities to review queue
        text = self.test_texts["mixed"]
        anonymized_text, entities = self.detector.detect_and_anonymize(text)
        
        # Get initial queue size
        initial_size = self.detector.get_review_queue_size()
        
        # Check that low confidence entities are in review queue
        low_confidence_entities = [e for e in entities if e.score < config.anonymization.confidence_threshold]
        assert len(low_confidence_entities) <= initial_size
        
        # Test updating an entity decision
        if low_confidence_entities:
            entity = low_confidence_entities[0]
            updated_entity = self.detector.update_entity_decision(
                entity_id=entity.entity_id,
                approved=True,
                replacement_text="[CUSTOM]",
            )
            
            # Check that entity was updated
            assert updated_entity is not None
            assert updated_entity.entity_id == entity.entity_id
            
            # Check that review queue size decreased
            assert self.detector.get_review_queue_size() == initial_size - 1
    
    def test_anonymization_cannot_be_disabled(self):
        """
        [AIU] Test that anonymization cannot be disabled.
        This test verifies that anonymization is mandatory.
        """
        # Attempt to disable anonymization (should raise an error)
        with pytest.raises(ValueError) as excinfo:
            # Access private attribute to simulate disabling anonymization
            # This is only for testing the safety feature
            self.detector._anonymization_enabled = False
            self.detector.detect_and_anonymize("Test")
        
        # Check that error message mentions that anonymization cannot be disabled
        assert "cannot be disabled" in str(excinfo.value).lower()


class TestSwissMedicalEntityRecognizer:
    """Tests for the Swiss medical entity recognizer."""
    
    def setup_method(self):
        """Set up test environment."""
        self.recognizer = SwissMedicalEntityRecognizer()
        
        # [TD] Synthetic test data
        self.test_texts = {
            "ahv": "AHV-Nummer: 756.1234.5678.90",
            "insurance": "Versicherungsnummer: 123.4567.8901.23",
            "medical_license": "Ärztliche Zulassung: CH-123456",
            "medical_record": "Patientenakte: MR-12345678",
            "hospital_id": "Patienten-ID: P-123456-78",
        }
    
    def test_analyze(self):
        """Test entity analysis."""
        # Test AHV number recognition
        results = self.recognizer.analyze(
            self.test_texts["ahv"],
            entities=["SWISS_AHV"],
            nlp_artifacts=None,
        )
        assert len(results) >= 1
        assert results[0].entity_type == "SWISS_AHV"
        
        # Test insurance number recognition
        results = self.recognizer.analyze(
            self.test_texts["insurance"],
            entities=["SWISS_INSURANCE"],
            nlp_artifacts=None,
        )
        assert len(results) >= 1
        assert results[0].entity_type == "SWISS_INSURANCE"
    
    def test_validate_result(self):
        """Test result validation."""
        # Test AHV validation
        assert self.recognizer.validate_result("756.1234.5678.90") is True
        assert self.recognizer.validate_result("123.1234.5678.90") is False
        
        # Test insurance validation
        assert self.recognizer.validate_result("123.4567.8901.23") is True


class TestSwissGermanDetector:
    """Tests for the Swiss German detector."""
    
    def setup_method(self):
        """Set up test environment."""
        self.detector = SwissGermanDetector()
        
        # [TD] Synthetic test data
        self.test_texts = {
            "standard_german": "Der Patient hat Kopfschmerzen und Fieber seit gestern.",
            "swiss_german": "Dr Patient hät Chopfweh und Fieber sit geschter.",
            "mixed": "Der Patient hat Chopfweh und Fieber seit gestern.",
        }
    
    def test_detect(self):
        """Test Swiss German detection."""
        # Test standard German
        assert not self.detector.detect(self.test_texts["standard_german"])
        
        # Test Swiss German
        assert self.detector.detect(self.test_texts["swiss_german"])
        
        # Test mixed text
        assert self.detector.detect(self.test_texts["mixed"])
    
    def test_get_confidence(self):
        """Test confidence scoring."""
        # Test standard German (should have low confidence)
        assert self.detector.get_confidence(self.test_texts["standard_german"]) < 0.3
        
        # Test Swiss German (should have high confidence)
        assert self.detector.get_confidence(self.test_texts["swiss_german"]) > 0.3
