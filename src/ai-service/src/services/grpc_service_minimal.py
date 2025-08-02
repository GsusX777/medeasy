# „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

"""
Minimal gRPC Service Implementation for MedEasy AI Service Testing.

[MLB] Multi-Language Bridge: .NET ↔ Python via gRPC
[WMM] Whisper Multi-Model support with benchmarking (minimal)
"""

import asyncio
import logging
import time
from typing import List

import grpc
import structlog

# Import generated protobuf modules (minimal schema)
import medeasy_minimal_pb2 as medeasy_pb2
import medeasy_minimal_pb2_grpc as medeasy_pb2_grpc

from src.config import config
from src.services.whisper_service import WhisperService

logger = structlog.get_logger()


class MedEasyServiceImpl(medeasy_pb2_grpc.MedEasyServiceServicer):
    """
    Minimal gRPC Service Implementation for MedEasy AI Service.
    
    [MLB] Provides .NET ↔ Python bridge via gRPC
    [WMM] Supports GetAvailableModels and GetHardwareInfo only
    """
    
    def __init__(self):
        """Initialize the minimal gRPC service."""
        # Activate WhisperService for real functionality
        self.whisper_service = WhisperService()
        logger.info("MedEasyServiceImpl (minimal) initialized with WhisperService")
    
    async def GetAvailableModels(
        self, 
        request: medeasy_pb2.GetAvailableModelsRequest, 
        context: grpc.aio.ServicerContext
    ) -> medeasy_pb2.GetAvailableModelsResponse:
        """Get available Whisper models (minimal implementation)."""
        logger.info(f"🎯 RECEIVED GetAvailableModels request: {request.request_id}")
        try:
            logger.info(
                "Getting available models",
                request_id=request.request_id
            )
            
            # Get real detailed models from WhisperService
            models = await self.whisper_service.get_available_models()
            
            # Convert ModelInfo objects to protobuf format
            model_infos = []
            for model in models:
                model_infos.append(medeasy_pb2.WhisperModelInfo(
                    name=model.name,
                    is_available=model.is_available,
                    is_downloaded=model.is_downloaded,
                    size_mb=model.size_mb,
                    supported_languages=model.supported_languages,
                    description=model.description,
                    estimated_speed_factor=model.estimated_speed_factor,
                    recommended_ram_gb=model.recommended_ram_gb
                ))
            
            response = medeasy_pb2.GetAvailableModelsResponse(
                request_id=request.request_id,
                models=model_infos,
                recommended_model=config.whisper.model
            )
            
            logger.info(f"✅ RETURNING {len(model_infos)} models for request {request.request_id}")
            logger.info(f"🎉 SUCCESS: GetAvailableModels completed for {request.request_id}")
            
            return response
            
        except Exception as e:
            logger.error("Failed to get available models", error=str(e))
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Failed to get models: {str(e)}")
            return medeasy_pb2.GetAvailableModelsResponse()
    
    async def GetHardwareInfo(
        self, 
        request: medeasy_pb2.GetHardwareInfoRequest, 
        context: grpc.aio.ServicerContext
    ) -> medeasy_pb2.GetHardwareInfoResponse:
        """
        Get hardware information for model selection.
        
        [WMM] Provides hardware specs for optimal model selection
        """
        try:
            logger.info(
                "Getting hardware info",
                request_id=request.request_id
            )
            
            # Get real hardware info from WhisperService
            hardware_info = await self.whisper_service.get_hardware_info()
            
            response = medeasy_pb2.GetHardwareInfoResponse(
                request_id=request.request_id,
                cuda_available=hardware_info.gpu_available,
                ram_gb=hardware_info.memory_gb,
                cpu_info=medeasy_pb2.CpuInfo(
                    physical_cores=hardware_info.cpu_cores,
                    logical_cores=hardware_info.cpu_cores
                )
            )
            
            logger.info(
                "Hardware info retrieved successfully",
                request_id=request.request_id,
                cuda_available=hardware_info.gpu_available,
                ram_gb=hardware_info.memory_gb
            )
            
            return response
            
        except Exception as e:
            logger.error("Failed to get hardware info", error=str(e))
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Failed to get hardware info: {str(e)}")
            return medeasy_pb2.GetHardwareInfoResponse()
    
    async def Transcribe(
        self, 
        request: medeasy_pb2.TranscriptionRequest, 
        context: grpc.aio.ServicerContext
    ) -> medeasy_pb2.TranscriptionResponse:
        """Transcribe audio to text with anonymization."""
        logger.info(f"🎤 RECEIVED Transcribe request: {request.request_id}")
        try:
            logger.info(
                "Starting audio transcription",
                request_id=request.request_id,
                audio_size=len(request.audio_data),
                model=request.whisper_model or "small",
                language=request.language or "auto"
            )
            
            # Transcribe using WhisperService
            result = await self.whisper_service.transcribe(
                audio_data=request.audio_data,
                model=request.whisper_model or "small",
                language=request.language or "auto"
            )
            
            # Create response (minimal anonymization for now)
            response = medeasy_pb2.TranscriptionResponse(
                request_id=request.request_id,
                text=result.text,  # TODO: Add anonymization
                original_text=result.text,
                language_code=result.language,
                processing_time_seconds=result.processing_time,
                cloud_processed=False  # Always local processing
            )
            
            logger.info(
                "Transcription completed successfully",
                request_id=request.request_id,
                text_length=len(result.text),
                processing_time=result.processing_time,
                model=result.model
            )
            
            return response
            
        except Exception as e:
            logger.error("Failed to transcribe audio", error=str(e), request_id=request.request_id)
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Failed to transcribe audio: {str(e)}")
            return medeasy_pb2.TranscriptionResponse()
    
    async def BenchmarkModels(
        self, 
        request: medeasy_pb2.BenchmarkModelsRequest, 
        context: grpc.aio.ServicerContext
    ) -> medeasy_pb2.BenchmarkModelsResponse:
        """Benchmark multiple Whisper models with test audio."""
        logger.info(f"📊 RECEIVED BenchmarkModels request: {request.request_id}")
        try:
            # DEBUG: Log received models
            logger.info(f"🔧 DEBUG: request.models_to_test = {list(request.models_to_test)}")
            logger.info(f"🔧 DEBUG: models_to_test type = {type(request.models_to_test)}")
            logger.info(f"🔧 DEBUG: models_to_test length = {len(request.models_to_test)}")
            
            # Extract audio filename from request
            audio_filename = request.audio_filename or "unknown.m4a"
            
            logger.info(
                "Starting model benchmarking",
                request_id=request.request_id,
                audio_size=len(request.test_audio_data),
                audio_filename=audio_filename,
                models_count=len(request.models_to_test),
                iterations=request.iterations
            )
            
            # Benchmark using WhisperService with filename
            results = await self.whisper_service.benchmark_models(
                audio_data=request.test_audio_data,
                models=list(request.models_to_test),
                audio_filename=audio_filename
            )
            
            # Convert results to protobuf format
            benchmark_results = []
            for result in results:
                benchmark_results.append(medeasy_pb2.ModelBenchmarkResult(
                    model_name=result.model,
                    processing_time_seconds=result.processing_time,
                    confidence_score=result.confidence,
                    transcribed_text=result.text,  # [NEW] Real transcribed text for comparison
                    success=True,
                    error_message="",
                    # Extended fields for full protobuf compatibility
                    average_processing_time_ms=int(result.processing_time * 1000),
                    min_processing_time_ms=int(result.processing_time * 1000),
                    max_processing_time_ms=int(result.processing_time * 1000),
                    average_accuracy=result.accuracy_score,
                    average_confidence=result.confidence,
                    cuda_used=False,  # Not tracked yet
                    average_cpu_usage=float(result.cpu_usage),  # ✅ Ensure double type for protobuf
                    average_gpu_usage=0.0,  # Not tracked yet
                    average_ram_usage_mb=int(result.memory_usage),
                    average_vram_usage_mb=0,  # Not tracked yet
                    performance_score=int(result.performance_score),  # ✅ Cast to int32 for protobuf!
                    accuracy_score=int(result.accuracy_score)
                ))
            
            # Create response
            response = medeasy_pb2.BenchmarkModelsResponse(
                request_id=request.request_id,
                benchmark_id=f"bench_{int(time.time())}",
                total_benchmark_time_ms=int(sum(r.processing_time for r in results) * 1000),
                results=benchmark_results
            )
            
            logger.info(
                "Benchmarking completed successfully",
                request_id=request.request_id,
                models_tested=len(benchmark_results),
                total_time_ms=response.total_benchmark_time_ms
            )
            
            return response
            
        except Exception as e:
            logger.error("Failed to benchmark models", error=str(e), request_id=request.request_id)
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(f"Failed to benchmark models: {str(e)}")
            return medeasy_pb2.BenchmarkModelsResponse()
