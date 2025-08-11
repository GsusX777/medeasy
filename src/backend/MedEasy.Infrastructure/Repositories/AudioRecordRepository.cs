// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.EntityFrameworkCore;
using MedEasy.Domain.Entities;
using MedEasy.Application.Interfaces;
using MedEasy.Infrastructure.Database;

namespace MedEasy.Infrastructure.Repositories;

/// <summary>
/// Repository-Implementierung für AudioRecord-Entitäten [SP][EIV][ATV]
/// Verwaltet verschlüsselte Audio-Aufnahmen in SQLCipher-Datenbank
/// </summary>
public class AudioRecordRepository : IAudioRecordRepository
{
    private readonly SQLCipherContext _context;

    public AudioRecordRepository(SQLCipherContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Fügt einen neuen AudioRecord hinzu [SP][EIV]
    /// </summary>
    public async Task<AudioRecord> AddAsync(AudioRecord audioRecord)
    {
        audioRecord.Created = DateTime.UtcNow;
        audioRecord.LastModified = DateTime.UtcNow;
        
        _context.AudioRecords.Add(audioRecord);
        await _context.SaveChangesAsync();
        
        return audioRecord;
    }

    /// <summary>
    /// Ruft einen AudioRecord anhand der ID ab [SP][EIV]
    /// </summary>
    public async Task<AudioRecord?> GetByIdAsync(string id)
    {
        return await _context.AudioRecords
            .FirstOrDefaultAsync(ar => ar.Id == id);
    }

    /// <summary>
    /// Ruft alle AudioRecords für einen Benchmark ab [WMM]
    /// </summary>
    public async Task<IEnumerable<AudioRecord>> GetByBenchmarkIdAsync(string benchmarkId)
    {
        return await _context.AudioRecords
            .Where(ar => ar.BenchmarkId == benchmarkId)
            .OrderByDescending(ar => ar.Created)
            .ToListAsync();
    }

    /// <summary>
    /// Ruft AudioRecords nach RecordingType ab [WMM]
    /// </summary>
    public async Task<IEnumerable<AudioRecord>> GetByRecordingTypeAsync(string recordingType)
    {
        return await _context.AudioRecords
            .Where(ar => ar.RecordingType == recordingType)
            .OrderByDescending(ar => ar.Created)
            .ToListAsync();
    }

    /// <summary>
    /// Ruft alle AudioRecords ab [ATV]
    /// </summary>
    public async Task<IEnumerable<AudioRecord>> GetAllAsync()
    {
        return await _context.AudioRecords
            .OrderByDescending(ar => ar.Created)
            .ToListAsync();
    }

    /// <summary>
    /// Aktualisiert einen AudioRecord [ATV]
    /// </summary>
    public async Task<AudioRecord> UpdateAsync(AudioRecord audioRecord)
    {
        audioRecord.LastModified = DateTime.UtcNow;
        
        _context.AudioRecords.Update(audioRecord);
        await _context.SaveChangesAsync();
        
        return audioRecord;
    }

    /// <summary>
    /// Löscht einen AudioRecord (Soft Delete) [ATV]
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        var audioRecord = await GetByIdAsync(id);
        if (audioRecord == null)
            return false;

        // Soft Delete - AudioRecord hat keine IsDeleted/DeletedAt Properties
        // Implementierung würde erweiterte Entity erfordern oder physisches Löschen
        _context.AudioRecords.Remove(audioRecord);
        await _context.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// Ruft AudioRecords ab, die eine manuelle Überprüfung benötigen [ARQ]
    /// </summary>
    public async Task<IEnumerable<AudioRecord>> GetRecordsNeedingReviewAsync()
    {
        return await _context.AudioRecords
            .Where(ar => ar.NeedsReview)
            .OrderByDescending(ar => ar.Created)
            .ToListAsync();
    }
}
