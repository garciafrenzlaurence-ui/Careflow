using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Persistence;

public sealed class CareFlowDbContext(DbContextOptions<CareFlowDbContext> options)
    : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DiagnosticService> DiagnosticServices => Set<DiagnosticService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasKey(x => x.PatientId);
            entity.Property(x => x.PatientNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.PatientNumber).IsUnique();
            entity.HasData(new Patient { PatientId = 1, PatientNumber = "P-0001", FullName = "Demo Patient" });
        });

        modelBuilder.Entity<DiagnosticService>(entity =>
        {
            entity.ToTable("DiagnosticServices");
            entity.HasKey(x => x.DiagnosticServiceId);
            entity.Property(x => x.ServiceName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.DailyCapacity).IsRequired();
            entity.HasData(
                new DiagnosticService { DiagnosticServiceId = 1, ServiceName = "Complete Blood Count (CBC)", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 2, ServiceName = "Urinalysis", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 3, ServiceName = "Chest X-Ray", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 4, ServiceName = "Blood Chemistry", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 5, ServiceName = "Electrocardiogram (ECG)", DailyCapacity = 20 });
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(x => x.BookingId);
            entity.Property(x => x.BookingDate).HasColumnType("TEXT");
            entity.Property(x => x.CreatedAt).HasColumnType("TEXT");
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasOne(x => x.Patient).WithMany(x => x.Bookings)
                .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.BookingDate, x.Status });
        });

        modelBuilder.Entity<BookingService>(entity =>
        {
            entity.ToTable("BookingServices");
            entity.HasKey(x => x.BookingServiceId);
            entity.HasOne(x => x.Booking).WithMany(x => x.BookingServices)
                .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.DiagnosticService).WithMany(x => x.BookingServices)
                .HasForeignKey(x => x.DiagnosticServiceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.BookingId, x.DiagnosticServiceId }).IsUnique();
            entity.HasIndex(x => x.DiagnosticServiceId);
        });
    }
}
