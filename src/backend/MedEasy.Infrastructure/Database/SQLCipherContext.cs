// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using MedEasy.Domain.Entities;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Infrastructure.Database
{
    /// <summary>
    /// DbContext für die verschlüsselte SQLCipher-Datenbank [SP]
    /// </summary>
    public class SQLCipherContext : DbContext
    {
        /// <summary>
        /// Patienten in der Datenbank
        /// </summary>
        public DbSet<Patient> Patients { get; set; }

        /// <summary>
        /// Sessions/Konsultationen in der Datenbank [SK]
        /// </summary>
        public DbSet<Session> Sessions { get; set; }

        /// <summary>
        /// Transkripte in der Datenbank
        /// </summary>
        public DbSet<Transcript> Transcripts { get; set; }

        /// <summary>
        /// Audit-Log für alle Datenbankoperationen [ATV]
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Anonymisierungs-Review-Queue [ARQ]
        /// </summary>
        public DbSet<AnonymizationReviewItem> AnonymizationReviewQueue { get; set; }

        /// <summary>
        /// Aktueller Benutzer für Audit-Trail
        /// </summary>
        private readonly string _currentUser;

        /// <summary>
        /// SQLCipher Konfiguration [SP]
        /// </summary>
        private readonly SQLCipherConfiguration _sqlCipherConfig;

        /// <summary>
        /// Konstruktor für SQLCipherContext
        /// </summary>
        public SQLCipherContext(DbContextOptions<SQLCipherContext> options, string currentUser = "System")
            : base(options)
        {
            _currentUser = currentUser;
            _sqlCipherConfig = options.FindExtension<SQLCipherConfiguration>() ?? new SQLCipherConfiguration();
        }

        /// <summary>
        /// Konfiguriert die Datenbankverbindung und Verschlüsselung [SP]
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Hier keine Connection String hardcoden! [NEA]
                // Connection String wird über DI injiziert
                throw new InvalidOperationException("SQLCipherContext muss mit einem Connection String konfiguriert werden");
            }

            // Stelle sicher, dass SQLCipher korrekt initialisiert wird
            optionsBuilder.ReplaceService<ISqliteConnectionFactory, SQLCipherConnectionFactory>();
        }

        /// <summary>
        /// Konfiguriert die Entity-Modelle
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Patient-Entity konfigurieren [EIV]
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                // Verschlüsselte Felder als erforderlich markieren [SP]
                entity.Property(e => e.EncryptedName).IsRequired();
                entity.Property(e => e.EncryptedGender).IsRequired();
                entity.Property(e => e.EncryptedInsuranceProvider).IsRequired();
                
                // Versicherungsnummer-Hash für Suche indizieren
                entity.Property(e => e.InsuranceNumberHash).IsRequired();
                entity.HasIndex(e => e.InsuranceNumberHash);
                
                // Beziehung zu Sessions konfigurieren
                entity.HasMany(e => e.Sessions)
                      .WithOne(e => e.Patient)
                      .HasForeignKey(e => e.PatientId)
                      .OnDelete(DeleteBehavior.Restrict); // Keine Kaskadenlöschung für Patientendaten [PSF]
            });
            
            // Session-Entity konfigurieren [SK]
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                // Zeit-Felder für Frontend-Kompatibilität [SF]
                entity.Property(e => e.SessionDate)
                      .HasColumnName("session_date")
                      .IsRequired();
                      
                entity.Property(e => e.StartTime)
                      .HasColumnName("start_time")
                      .IsRequired(false); // Optional
                      
                entity.Property(e => e.EndTime)
                      .HasColumnName("end_time")
                      .IsRequired(false); // Optional
                
                // Verschlüsselte Felder als erforderlich markieren [SP]
                entity.Property(e => e.EncryptedReason).IsRequired(false); // Optional für Frontend-Kompatibilität
                entity.Property(e => e.EncryptedNotes).IsRequired(false); // Notizen können leer sein
                
                // Beziehung zu Patient konfigurieren
                entity.HasOne(e => e.Patient)
                      .WithMany(e => e.Sessions)
                      .HasForeignKey(e => e.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Beziehung zu Transcripts konfigurieren
                entity.HasMany(e => e.Transcripts)
                      .WithOne(e => e.Session)
                      .HasForeignKey(e => e.SessionId)
                      .OnDelete(DeleteBehavior.Cascade); // Transkripte werden mit Session gelöscht
                
                // Status als String speichern
                entity.Property(e => e.Status)
                      .HasConversion<string>();
                
                // Audit-Felder als erforderlich markieren [ATV]
                entity.Property(e => e.Created).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.LastModified).IsRequired();
                entity.Property(e => e.LastModifiedBy).IsRequired();
            });
            
            // Transcript-Entity konfigurieren
            modelBuilder.Entity<Transcript>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                // Verschlüsselte Felder als erforderlich markieren [SP]
                entity.Property(e => e.EncryptedOriginalText).IsRequired();
                entity.Property(e => e.EncryptedAnonymizedText).IsRequired(); // Anonymisierung ist PFLICHT [AIU]
                
                // Beziehung zu Session konfigurieren
                entity.HasOne(e => e.Session)
                      .WithMany(e => e.Transcripts)
                      .HasForeignKey(e => e.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Audit-Felder als erforderlich markieren [ATV]
                entity.Property(e => e.Created).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.LastModified).IsRequired();
                entity.Property(e => e.LastModifiedBy).IsRequired();
                
                // Index für Anonymisierungs-Review-Queue [ARQ]
                entity.HasIndex(e => e.RequiresAnonymizationReview);
            });
            
            // AuditLog-Entity konfigurieren [ATV]
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                // Erforderliche Felder
                entity.Property(e => e.EntityName).IsRequired();
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                
                // Indizes für schnelle Suche im Audit-Log
                entity.HasIndex(e => e.EntityName);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
            });
            
            // AnonymizationReviewItem-Entity konfigurieren [ARQ]
            modelBuilder.Entity<AnonymizationReviewItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                // Erforderliche Felder
                entity.Property(e => e.TranscriptId).IsRequired();
                entity.Property(e => e.DetectedPII).IsRequired();
                entity.Property(e => e.AnonymizationConfidence).IsRequired();
                entity.Property(e => e.ReviewReason).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().IsRequired();
                entity.Property(e => e.Created).IsRequired();
                
                // Optionale Felder
                entity.Property(e => e.ReviewedAt).IsRequired(false);
                entity.Property(e => e.ReviewedBy).IsRequired(false);
                entity.Property(e => e.ReviewerNotes).IsRequired(false);
                
                // Beziehung zum Transkript konfigurieren
                entity.HasOne(e => e.Transcript)
                      .WithMany()
                      .HasForeignKey(e => e.TranscriptId)
                      .OnDelete(DeleteBehavior.Cascade); // Review-Items werden mit Transkript gelöscht
                
                // Indizes für schnelle Suche
                entity.HasIndex(e => e.TranscriptId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Created);
                entity.HasIndex(e => e.AnonymizationConfidence); // Für Batch-Verarbeitung nach Konfidenz
            });
        }

        /// <summary>
        /// Überschreibt SaveChanges, um Audit-Trail zu implementieren [ATV]
        /// </summary>
        public override int SaveChanges()
        {
            AddAuditTrail();
            return base.SaveChanges();
        }

        /// <summary>
        /// Überschreibt SaveChangesAsync, um Audit-Trail zu implementieren [ATV]
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddAuditTrail();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Überschreibt SaveChanges, um Audit-Trail zu implementieren [ATV]
        /// </summary>
        public override int SaveChanges()
        {
            AddAuditTrail();
            return base.SaveChanges();
        }

        /// <summary>
        /// Überschreibt SaveChangesAsync, um Audit-Trail zu implementieren [ATV]
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddAuditTrail();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Fügt Audit-Trail für alle Änderungen hinzu [ATV]
        /// </summary>
        private void AddAuditTrail()
        {
            var entries = ChangeTracker.Entries();
            var timestamp = DateTime.UtcNow;
            
            foreach (var entry in entries)
            {
                // Überspringe AuditLog-Einträge selbst, um Endlosschleife zu vermeiden
                if (entry.Entity is AuditLog)
                    continue;

                var entityName = entry.Entity.GetType().Name;
                var entityId = GetEntityId(entry);
                
                // Prüfe, ob es sich um eine Entity mit Audit-Feldern handelt
                UpdateAuditFields(entry, timestamp);
                
                // Erfasse detaillierte Änderungen für den Audit-Trail
                var changedProperties = GetChangedProperties(entry);

                switch (entry.State)
                {
                    case EntityState.Added:
                        AuditLogs.Add(new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            EntityName = entityName,
                            EntityId = entityId,
                            Action = "INSERT",
                            Changes = changedProperties.Count > 0 ? 
                                     $"Neue Entity erstellt mit {changedProperties.Count} Feldern" : 
                                     "Neue Entity erstellt",
                            ContainsSensitiveData = ContainsSensitiveData(entry),
                            Timestamp = timestamp,
                            UserId = _currentUser
                        });
                        break;
                    case EntityState.Modified:
                        // Nur Änderungen protokollieren, wenn tatsächlich etwas geändert wurde
                        if (changedProperties.Count > 0)
                        {
                            AuditLogs.Add(new AuditLog
                            {
                                Id = Guid.NewGuid(),
                                EntityName = entityName,
                                EntityId = entityId,
                                Action = "UPDATE",
                                Changes = $"{changedProperties.Count} Felder geändert: {string.Join(", ", changedProperties)}",
                                ContainsSensitiveData = ContainsSensitiveData(entry),
                                Timestamp = timestamp,
                                UserId = _currentUser
                            });
                        }
                        break;
                    case EntityState.Deleted:
                        AuditLogs.Add(new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            EntityName = entityName,
                            EntityId = entityId,
                            Action = "DELETE",
                            Changes = "Entity gelöscht",
                            ContainsSensitiveData = ContainsSensitiveData(entry),
                            Timestamp = timestamp,
                            UserId = _currentUser
                        });
                        break;
                }
            }
        }
        
        /// <summary>
        /// Aktualisiert die Audit-Felder einer Entity [ATV]
        /// </summary>
        private void UpdateAuditFields(EntityEntry entry, DateTime timestamp)
        {
            if (entry.State == EntityState.Added)
            {
                // Created und CreatedBy für neue Entities setzen
                if (entry.Entity is IHasAuditInfo auditableEntity)
                {
                    auditableEntity.Created = timestamp;
                    auditableEntity.CreatedBy = _currentUser;
                    auditableEntity.LastModified = timestamp;
                    auditableEntity.LastModifiedBy = _currentUser;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // LastModified und LastModifiedBy für geänderte Entities aktualisieren
                if (entry.Entity is IHasAuditInfo auditableEntity)
                {
                    auditableEntity.LastModified = timestamp;
                    auditableEntity.LastModifiedBy = _currentUser;
                }
            }
        }
        
        /// <summary>
        /// Ermittelt die ID einer Entity [ATV]
        /// </summary>
        private string GetEntityId(EntityEntry entry)
        {
            try
            {
                var primaryKey = entry.Metadata.FindPrimaryKey();
                if (primaryKey != null)
                {
                    var keyName = primaryKey.Properties[0].Name;
                    var keyValue = entry.Property(keyName).CurrentValue;
                    return keyValue?.ToString() ?? "<null>";
                }
            }
            catch
            {
                // Fehler beim Ermitteln der ID abfangen
            }
            
            return "<unknown>";
        }
        
        /// <summary>
        /// Ermittelt die geänderten Eigenschaften einer Entity [ATV]
        /// </summary>
        private List<string> GetChangedProperties(EntityEntry entry)
        {
            var changedProperties = new List<string>();
            
            if (entry.State == EntityState.Added)
            {
                // Bei neuen Entities alle nicht-null Properties als geändert betrachten
                foreach (var property in entry.Properties)
                {
                    if (property.CurrentValue != null && 
                        !property.Metadata.Name.EndsWith("Id") && // IDs ausschließen
                        !IsAuditField(property.Metadata.Name)) // Audit-Felder ausschließen
                    {
                        changedProperties.Add(property.Metadata.Name);
                    }
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // Bei geänderten Entities nur tatsächlich geänderte Properties erfassen
                foreach (var property in entry.Properties)
                {
                    if (property.IsModified && !IsAuditField(property.Metadata.Name))
                    {
                        changedProperties.Add(property.Metadata.Name);
                    }
                }
            }
            
            return changedProperties;
        }
        
        /// <summary>
        /// Prüft, ob ein Feldname ein Audit-Feld ist [ATV]
        /// </summary>
        private bool IsAuditField(string propertyName)
        {
            return propertyName == "Created" || 
                   propertyName == "CreatedBy" || 
                   propertyName == "LastModified" || 
                   propertyName == "LastModifiedBy";
        }
        
        /// <summary>
        /// Prüft, ob eine Entity sensible Daten enthält [SP][EIV]
        /// </summary>
        private bool ContainsSensitiveData(EntityEntry entry)
        {
            // Alle Entities mit "Encrypted" im Feldnamen enthalten sensible Daten
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name.Contains("Encrypted"))
                {
                    return true;
                }
            }
            
            // Bestimmte Entity-Typen immer als sensibel markieren
            var entityType = entry.Entity.GetType().Name;
            return entityType == "Patient" || 
                   entityType == "Transcript" || 
                   entityType == "Session";
        }
    }

    /// <summary>
    /// Repräsentiert einen Eintrag im Audit-Log [ATV]
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Eindeutige ID des Audit-Log-Eintrags
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name der Entität, die geändert wurde
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// ID der geänderten Entität
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Durchgeführte Aktion (INSERT, UPDATE, DELETE)
        /// </summary>
        public string Action { get; set; }
        
        /// <summary>
        /// Beschreibung der Änderungen (welche Felder wurden geändert)
        /// </summary>
        public string Changes { get; set; }
        
        /// <summary>
        /// Flag, ob die Änderung sensible Daten betrifft [SP][EIV]
        /// </summary>
        public bool ContainsSensitiveData { get; set; }

        /// <summary>
        /// Zeitstempel der Änderung
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// ID des Benutzers, der die Änderung durchgeführt hat
        /// </summary>
        public string UserId { get; set; }
    }

    /// <summary>
    /// Repräsentiert einen Eintrag in der Anonymisierungs-Review-Queue [ARQ]
    /// </summary>
    public class AnonymizationReviewItem
    {
        /// <summary>
        /// Eindeutige ID des Review-Items
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Referenz zum Transkript
        /// </summary>
        public Guid TranscriptId { get; set; }

        /// <summary>
        /// Navigationseigenschaft zum Transkript
        /// </summary>
        public Transcript Transcript { get; set; }
        
        /// <summary>
        /// Erkannte personenbezogene Daten (PII) [AIU]
        /// </summary>
        public string DetectedPII { get; set; }
        
        /// <summary>
        /// Konfidenz der Anonymisierung in Prozent [ARQ]
        /// </summary>
        public double AnonymizationConfidence { get; set; }
        
        /// <summary>
        /// Grund für die Überprüfung [ARQ]
        /// </summary>
        public string ReviewReason { get; set; }

        /// <summary>
        /// Zeitstempel der Erstellung
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Status des Review-Items
        /// </summary>
        public ReviewStatus Status { get; set; }

        /// <summary>
        /// Zeitstempel der Überprüfung
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Benutzer, der die Überprüfung durchgeführt hat
        /// </summary>
        public string ReviewedBy { get; set; }
        
        /// <summary>
        /// Anmerkungen des Reviewers
        /// </summary>
        public string ReviewerNotes { get; set; }
    }

    /// <summary>
    /// Status eines Review-Items [ARQ]
    /// </summary>
    public enum ReviewStatus
    {
        /// <summary>
        /// Wartet auf Überprüfung
        /// </summary>
        Pending,
        
        /// <summary>
        /// In Bearbeitung durch einen Reviewer
        /// </summary>
        InReview,

        /// <summary>
        /// Überprüft und genehmigt
        /// </summary>
        Approved,

        /// <summary>
        /// Überprüft und abgelehnt (erfordert Neu-Anonymisierung)
        /// </summary>
        Rejected,
        
        /// <summary>
        /// Zur Whitelist hinzugefügt (medizinischer Fachbegriff)
        /// </summary>
        Whitelisted,
        
        /// <summary>
        /// Automatisch genehmigt (hohe Konfidenz)
        /// </summary>
        AutoApproved
    }
}
