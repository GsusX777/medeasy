# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Unit tests for the Swiss German dialect detector.

[KP100] Critical path test coverage
[SDH] Swiss German dialect handling
[MFD] Swiss medical terminology
"""

import unittest
from unittest.mock import patch, MagicMock
import structlog
import pytest

from src.swiss.dialect_detector import SwissGermanDetector


class TestSwissGermanDetector(unittest.TestCase):
    """Test suite for the Swiss German dialect detector."""

    def setUp(self):
        """Set up test fixtures."""
        self.logger = structlog.get_logger()
        self.detector = SwissGermanDetector(self.logger)

    def test_initialization(self):
        """Test that the detector initializes correctly."""
        self.assertIsNotNone(self.detector)
        self.assertEqual(self.detector.logger, self.logger)
        self.assertIsNotNone(self.detector.swiss_german_patterns)
        self.assertIsNotNone(self.detector.swiss_medical_terms)

    def test_detect_swiss_german_positive(self):
        """Test Swiss German detection with Swiss German text."""
        text = "Grüezi, ich ha chli Chopfweh und Rückeschmerze. Mis Chind het au Fieber gha."
        result = self.detector.detect_swiss_german(text)
        
        self.assertTrue(result.is_swiss_german)
        self.assertGreaterEqual(result.confidence_score, 0.7)
        self.assertGreaterEqual(len(result.dialect_markers), 2)
        self.assertLessEqual(result.processing_time_seconds, 1.0)

    def test_detect_swiss_german_negative(self):
        """Test Swiss German detection with standard German text."""
        text = "Guten Tag, ich habe Kopfschmerzen und Rückenschmerzen. Mein Kind hatte auch Fieber."
        result = self.detector.detect_swiss_german(text)
        
        self.assertFalse(result.is_swiss_german)
        self.assertLess(result.confidence_score, 0.7)
        self.assertLessEqual(len(result.dialect_markers), 1)
        self.assertLessEqual(result.processing_time_seconds, 1.0)

    def test_detect_swiss_german_mixed(self):
        """Test Swiss German detection with mixed text."""
        text = "Guten Tag, ich ha chli Kopfschmerzen und Rückeweh. Mein Kind hatte Fieber."
        result = self.detector.detect_swiss_german(text)
        
        # Should detect some Swiss German but with lower confidence
        self.assertGreaterEqual(len(result.dialect_markers), 1)
        self.assertLessEqual(result.processing_time_seconds, 1.0)

    def test_extract_swiss_medical_terms(self):
        """Test extraction of Swiss medical terminology."""
        text = "Ich ha Chopfweh und Rückeschmerze. Ich bin im Spital gsi und der Doktor het mir Tablette verschriebe."
        terms = self.detector.extract_swiss_medical_terms(text)
        
        self.assertIn("Spital", terms)  # Hospital in Swiss German
        self.assertIn("Doktor", terms)  # Doctor in Swiss German

    def test_get_dialect_markers(self):
        """Test extraction of dialect markers."""
        text = "Grüezi, ich ha chli Chopfweh und Rückeschmerze."
        markers = self.detector.get_dialect_markers(text)
        
        self.assertGreaterEqual(len(markers), 2)
        self.assertTrue(any(marker.pattern == "ha" for marker in markers))
        self.assertTrue(any(marker.pattern == "chli" for marker in markers))

    def test_calculate_confidence_score(self):
        """Test confidence score calculation."""
        # Few markers should give low confidence
        few_markers = [
            MagicMock(pattern="ha", matches=1)
        ]
        low_confidence = self.detector.calculate_confidence_score(few_markers, "This is a long text with one Swiss German word ha.")
        self.assertLess(low_confidence, 0.5)
        
        # Many markers should give high confidence
        many_markers = [
            MagicMock(pattern="ha", matches=2),
            MagicMock(pattern="chli", matches=1),
            MagicMock(pattern="Grüezi", matches=1),
            MagicMock(pattern="isch", matches=2)
        ]
        high_confidence = self.detector.calculate_confidence_score(many_markers, "Grüezi, das isch en chli Test. Ich ha Chopfweh.")
        self.assertGreaterEqual(high_confidence, 0.7)

    def test_detect_with_empty_text(self):
        """Test detection with empty text."""
        result = self.detector.detect_swiss_german("")
        
        self.assertFalse(result.is_swiss_german)
        self.assertEqual(result.confidence_score, 0.0)
        self.assertEqual(len(result.dialect_markers), 0)
        self.assertEqual(len(result.swiss_medical_terms), 0)

    def test_detect_with_very_short_text(self):
        """Test detection with very short text."""
        result = self.detector.detect_swiss_german("ha")
        
        # Should not be confident with very short text
        self.assertLess(result.confidence_score, 0.7)

    @patch('src.swiss.dialect_detector.time.time')
    def test_processing_time_measurement(self, mock_time):
        """Test that processing time is measured correctly."""
        mock_time.side_effect = [0.0, 0.5]  # Start time, end time
        
        result = self.detector.detect_swiss_german("Grüezi, ich ha chli Chopfweh.")
        
        self.assertEqual(result.processing_time_seconds, 0.5)

    def test_detect_with_medical_context(self):
        """Test detection with medical context."""
        text = "Ich ha Chopfweh und bin zum Doktor im Spital gsi. Er het mir Tablette verschriebe."
        result = self.detector.detect_swiss_german(text)
        
        self.assertTrue(result.is_swiss_german)
        self.assertGreaterEqual(len(result.swiss_medical_terms), 2)
        self.assertIn("Spital", result.swiss_medical_terms)
        self.assertIn("Doktor", result.swiss_medical_terms)


if __name__ == '__main__':
    unittest.main()
