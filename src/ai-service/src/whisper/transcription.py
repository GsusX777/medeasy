# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Whisper transcription module for MedEasy.
Provides medical audio transcription with Swiss German support.

[WMM] Implements Whisper Multi-Model
[SDH] Swiss German handling
[PB] Performance baseline for transcription (<3 sec latency)
"""

import os
import re
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, List, Optional, Union

import structlog
from faster_whisper import WhisperModel as FasterWhisperModel

from src.config import config

logger = structlog.get_logger()


@dataclass
class TranscriptionResult:
    """Result of a transcription operation."""
    text: str
    language: str
    segments: List[Dict[str, Any]]
    processing_time: float


class WhisperTranscriber:
    """
    Whisper transcription service for medical audio.
    Supports multiple model sizes and Swiss German detection.
    """

    def __init__(self):
        """Initialize the transcriber with the configured model."""
        self.model_name = config.whisper.model
        self.device = "cuda" if torch.cuda.is_available() and config.whisper.enable_cuda else "cpu"
        
        logger.info(
            "Initializing Whisper transcriber",
            model=self.model_name,
            device=self.device,
        )
        
        # Load the faster-whisper model [WMM]
        try:
            # faster-whisper uses different initialization
            device = "cuda" if self.device == "cuda" and config.whisper.enable_cuda else "cpu"
            compute_type = "float16" if device == "cuda" else "int8"
            
            self.model = FasterWhisperModel(
                self.model_name.value,
                device=device,
                compute_type=compute_type,
                download_root=str(config.whisper.local_models_path),
            )
            logger.info(
                "faster-whisper model loaded successfully",
                model=self.model_name,
                device=device,
                compute_type=compute_type,
            )
        except Exception as e:
            logger.error(
                "Failed to load faster-whisper model",
                model=self.model_name,
                error=str(e),
                exc_info=True,
            )
            raise RuntimeError(f"Failed to load faster-whisper model: {str(e)}")
        
        # Swiss German detection patterns
        self.swiss_german_patterns = [
            r'\b(nöd|hoi|grüezi|merci|velo|znacht|zmorge|zmittag)\b',
            r'\b(isch|het|hät|gsi|gsii|gseh)\b',
            r'\b(chind|chrank|chrankheit|chopfweh)\b',
            r'\b(spital|dokter|schwöschter)\b',
        ]
        self.swiss_german_regex = re.compile('|'.join(self.swiss_german_patterns), re.IGNORECASE)

    def transcribe(self, audio_data: bytes) -> TranscriptionResult:
        """
        Transcribe audio data to text.
        Language detection is handled automatically by the Whisper model.
        
        Args:
            audio_data: Raw audio data bytes
            
        Returns:
            TranscriptionResult with transcribed text and metadata
            
        [PB] Performance baseline: <3 sec latency target
        """
        start_time = time.time()
        
        # Save audio data to a temporary file
        temp_dir = Path("./temp")
        temp_dir.mkdir(exist_ok=True)
        temp_file = temp_dir / f"audio_{int(time.time())}.wav"
        
        try:
            with open(temp_file, "wb") as f:
                f.write(audio_data)
            
            # Transcribe with Whisper - automatic language detection
            # Language detection is handled automatically by the Whisper model [PSF][Bugfix]
            transcribe_options = {
                "task": "transcribe",
                "verbose": False,
            }
            
            result = self.model.transcribe(str(temp_file), **transcribe_options)
            
            # Create result object
            processing_time = time.time() - start_time
            transcription_result = TranscriptionResult(
                text=result["text"],
                language=result.get("language", language),
                segments=result.get("segments", []),
                processing_time=processing_time,
            )
            
            # Log performance metrics
            logger.info(
                "Transcription completed",
                audio_length_seconds=result.get("duration", 0),
                processing_time=processing_time,
                language=transcription_result.language,
                performance_target_met=processing_time < 3.0,  # [PB] <3 sec latency
            )
            
            return transcription_result
            
        except Exception as e:
            logger.error(
                "Transcription failed",
                error=str(e),
                exc_info=True,
            )
            raise
        finally:
            # Clean up temporary file
            if temp_file.exists():
                temp_file.unlink()

    def detect_swiss_german(self, text: str) -> bool:
        """
        [SDH] Detect if text contains Swiss German dialect.
        
        Args:
            text: The text to analyze
            
        Returns:
            True if Swiss German is detected, False otherwise
        """
        if not config.whisper.enable_swiss_german_detection:
            return False
        
        # Check for Swiss German patterns
        match = self.swiss_german_regex.search(text)
        if match:
            logger.info(
                "Swiss German detected",
                match=match.group(),
                position=match.span(),
            )
            return True
        
        return False

    def get_available_models(self) -> List[str]:
        """Get list of available Whisper models."""
        return ["tiny", "small", "medium"]
