// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using MedEasy.Domain.Entities;

namespace MedEasy.Application.Interfaces;

/// <summary>
/// Repository interface für AudioRecord-Entitäten [SP][EIV][ATV]
/// Verwaltet verschlüsselte Audio-Aufnahmen für Benchmark-Tests
/// </summary>
public interface IAudioRecordRepository
{
    /// <summary>
    /// Fügt einen neuen AudioRecord hinzu [SP][EIV]
    /// </summary>
    /// <param name="audioRecord">Audio-Record mit verschlüsselten Daten</param>
    /// <returns>Gespeicherter AudioRecord</returns>
    Task<AudioRecord> AddAsync(AudioRecord audioRecord);

    /// <summary>
    /// Ruft einen AudioRecord anhand der ID ab [SP][EIV]
    /// </summary>
    /// <param name="id">AudioRecord-ID</param>
    /// <returns>AudioRecord oder null</returns>
    Task<AudioRecord?> GetByIdAsync(string id);

    /// <summary>
    /// Ruft alle AudioRecords für einen Benchmark ab [WMM]
    /// </summary>
    /// <param name="benchmarkId">Benchmark-ID</param>
    /// <returns>Liste der AudioRecords</returns>
    Task<IEnumerable<AudioRecord>> GetByBenchmarkIdAsync(string benchmarkId);

    /// <summary>
    /// Ruft AudioRecords nach RecordingType ab [WMM]
    /// </summary>
    /// <param name="recordingType">Art der Aufnahme</param>
    /// <returns>Liste der AudioRecords</returns>
    Task<IEnumerable<AudioRecord>> GetByRecordingTypeAsync(string recordingType);

    /// <summary>
    /// Ruft alle AudioRecords ab [ATV]
    /// </summary>
    /// <returns>Liste aller AudioRecords</returns>
    Task<IEnumerable<AudioRecord>> GetAllAsync();

    /// <summary>
    /// Aktualisiert einen AudioRecord [ATV]
    /// </summary>
    /// <param name="audioRecord">Zu aktualisierender AudioRecord</param>
    /// <returns>Aktualisierter AudioRecord</returns>
    Task<AudioRecord> UpdateAsync(AudioRecord audioRecord);

    /// <summary>
    /// Löscht einen AudioRecord (Soft Delete) [ATV]
    /// </summary>
    /// <param name="id">AudioRecord-ID</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Ruft AudioRecords ab, die eine manuelle Überprüfung benötigen [ARQ]
    /// </summary>
    /// <returns>Liste der AudioRecords mit NeedsReview = true</returns>
    Task<IEnumerable<AudioRecord>> GetRecordsNeedingReviewAsync();
}
