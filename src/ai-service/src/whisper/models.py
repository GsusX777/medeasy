# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Whisper model management for MedEasy.
Handles model selection, loading, and hardware optimization.

[WMM] Whisper Multi-Model implementation
[PB] Performance baseline for transcription
"""

import os
from dataclasses import dataclass
from enum import Enum
from pathlib import Path
from typing import Dict, List, Optional, Tuple

import structlog
import torch

from src.config import WhisperModel, config

logger = structlog.get_logger()


@dataclass
class ModelInfo:
    """Information about a Whisper model."""
    name: str
    size_mb: int
    languages: List[str]
    recommended_vram_gb: float
    recommended_ram_gb: float
    performance_score: int  # 1-10, higher is better
    accuracy_score: int  # 1-10, higher is better


class ModelSelector:
    """
    [WMM] Whisper model selector based on hardware capabilities.
    Automatically selects the best model for the available hardware.
    """

    # Model information
    MODEL_INFO: Dict[str, ModelInfo] = {
        "tiny": ModelInfo(
            name="tiny",
            size_mb=75,
            languages=["en", "de", "fr", "it"],
            recommended_vram_gb=0.5,
            recommended_ram_gb=2.0,
            performance_score=9,
            accuracy_score=3,
        ),
        "base": ModelInfo(
            name="base",
            size_mb=142,
            languages=["en", "de", "fr", "it"],
            recommended_vram_gb=1.0,
            recommended_ram_gb=4.0,
            performance_score=8,
            accuracy_score=5,
        ),
        "small": ModelInfo(
            name="small",
            size_mb=466,
            languages=["en", "de", "fr", "it", "multilingual"],
            recommended_vram_gb=2.0,
            recommended_ram_gb=8.0,
            performance_score=6,
            accuracy_score=7,
        ),
        "medium": ModelInfo(
            name="medium",
            size_mb=1500,
            languages=["en", "de", "fr", "it", "multilingual"],
            recommended_vram_gb=5.0,
            recommended_ram_gb=16.0,
            performance_score=4,
            accuracy_score=9,
        ),
    }

    def __init__(self):
        """Initialize the model selector."""
        self.available_models = self._get_available_models()
        self.system_info = self._get_system_info()
        
    def _get_available_models(self) -> List[str]:
        """Get list of available Whisper models."""
        return list(self.MODEL_INFO.keys())
    
    def _get_system_info(self) -> Dict[str, float]:
        """Get system hardware information."""
        # Get available VRAM if CUDA is available
        cuda_available = torch.cuda.is_available()
        vram_gb = 0.0
        if cuda_available:
            try:
                # Get VRAM in GB
                vram_bytes = torch.cuda.get_device_properties(0).total_memory
                vram_gb = vram_bytes / (1024 ** 3)
            except Exception as e:
                logger.warning(f"Failed to get CUDA device properties: {e}")
                cuda_available = False
        
        # Get available system RAM (approximate)
        ram_gb = 8.0  # Default assumption
        try:
            import psutil
            ram_bytes = psutil.virtual_memory().total
            ram_gb = ram_bytes / (1024 ** 3)
        except ImportError:
            logger.warning("psutil not available, using default RAM estimate")
        
        return {
            "cuda_available": cuda_available,
            "vram_gb": vram_gb,
            "ram_gb": ram_gb,
        }
    
    def select_best_model(self) -> str:
        """
        Select the best Whisper model based on hardware capabilities.
        
        Returns:
            Name of the best model to use
        """
        # If CUDA is available, select based on VRAM
        if self.system_info["cuda_available"]:
            vram_gb = self.system_info["vram_gb"]
            
            if vram_gb >= 5.0:
                return "medium"
            elif vram_gb >= 2.0:
                return "small"
            elif vram_gb >= 1.0:
                return "base"
            else:
                return "tiny"
        
        # If CUDA is not available, select based on RAM
        ram_gb = self.system_info["ram_gb"]
        
        if ram_gb >= 16.0:
            return "medium"
        elif ram_gb >= 8.0:
            return "small"
        elif ram_gb >= 4.0:
            return "base"
        else:
            return "tiny"
    
    def get_model_info(self, model_name: str) -> Optional[ModelInfo]:
        """Get information about a specific model."""
        return self.MODEL_INFO.get(model_name)
    
    def compare_models(self, model1: str, model2: str) -> Dict[str, Dict[str, int]]:
        """
        Compare two models based on performance and accuracy.
        
        Returns:
            Dictionary with comparison results
        """
        info1 = self.get_model_info(model1)
        info2 = self.get_model_info(model2)
        
        if not info1 or not info2:
            return {}
        
        return {
            "performance": {
                model1: info1.performance_score,
                model2: info2.performance_score,
            },
            "accuracy": {
                model1: info1.accuracy_score,
                model2: info2.accuracy_score,
            },
            "size_mb": {
                model1: info1.size_mb,
                model2: info2.size_mb,
            },
        }


class ModelManager:
    """
    Manages Whisper model download, caching, and loading.
    """
    
    def __init__(self):
        """Initialize the model manager."""
        self.model_selector = ModelSelector()
        self.models_dir = Path(config.whisper.local_models_path)
        self.models_dir.mkdir(exist_ok=True, parents=True)
        
    def download_model(self, model_name: str) -> bool:
        """
        Download a Whisper model if not already available.
        
        Args:
            model_name: Name of the model to download
            
        Returns:
            True if successful, False otherwise
        """
        try:
            # This will download the model if not already available
            import whisper
            whisper.load_model(
                model_name,
                download_root=str(self.models_dir),
                in_memory=False,
            )
            return True
        except Exception as e:
            logger.error(
                "Failed to download model",
                model=model_name,
                error=str(e),
                exc_info=True,
            )
            return False
    
    def get_downloaded_models(self) -> List[str]:
        """
        Get list of already downloaded models.
        
        Returns:
            List of model names that are downloaded
        """
        downloaded = []
        for model_name in self.model_selector.available_models:
            # Check if model files exist
            model_path = self.models_dir / f"{model_name}.pt"
            if model_path.exists():
                downloaded.append(model_name)
        return downloaded
    
    def get_model_path(self, model_name: str) -> Optional[Path]:
        """
        Get path to a downloaded model.
        
        Args:
            model_name: Name of the model
            
        Returns:
            Path to the model file, or None if not downloaded
        """
        model_path = self.models_dir / f"{model_name}.pt"
        return model_path if model_path.exists() else None
    
    def get_recommended_model(self) -> str:
        """
        Get the recommended model based on hardware and configuration.
        
        Returns:
            Name of the recommended model
        """
        # If a model is explicitly configured, use that
        if config.whisper.model:
            return config.whisper.model.value
        
        # Otherwise, select the best model based on hardware
        return self.model_selector.select_best_model()
