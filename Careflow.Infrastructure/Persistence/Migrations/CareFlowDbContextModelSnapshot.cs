using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace CareFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CareFlowDbContext))]
partial class CareFlowDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        modelBuilder.Entity<Patient>(b =>
        {
            b.ToTable("Patients"); b.HasKey(x => x.PatientId);
            b.Property(x => x.PatientId).ValueGeneratedOnAdd();
            b.Property(x => x.PatientNumber).HasMaxLength(30).IsRequired();
            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            b.HasIndex(x => x.PatientNumber).IsUnique();
            b.HasData(new Patient { PatientId = 1, PatientNumber = "P-0001", FullName = "Demo Patient" });
        });
        modelBuilder.Entity<DiagnosticService>(b =>
        {
            b.ToTable("DiagnosticServices"); b.HasKey(x => x.DiagnosticServiceId);
            b.Property(x => x.DiagnosticServiceId).ValueGeneratedOnAdd();
            b.Property(x => x.ServiceName).HasMaxLength(120).IsRequired();
            b.Property(x => x.DailyCapacity).IsRequired();
            b.HasData(
                new DiagnosticService { DiagnosticServiceId = 1, ServiceName = "Complete Blood Count (CBC)", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 2, ServiceName = "Urinalysis", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 3, ServiceName = "Chest X-Ray", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 4, ServiceName = "Blood Chemistry", DailyCapacity = 20 },
                new DiagnosticService { DiagnosticServiceId = 5, ServiceName = "Electrocardiogram (ECG)", DailyCapacity = 20 });
        });
        modelBuilder.Entity<Booking>(b =>
        {
            b.ToTable("Bookings"); b.HasKey(x => x.BookingId);
            b.Property(x => x.BookingId).ValueGeneratedOnAdd();
            b.Property(x => x.BookingDate).HasColumnType("TEXT");
            b.Property(x => x.CreatedAt).HasColumnType("TEXT");
            b.Property(x => x.Status).HasConversion<int>();
            b.HasIndex(x => new { x.BookingDate, x.Status });
            b.HasOne(x => x.Patient).WithMany(x => x.Bookings)
                .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<BookingService>(b =>
        {
            b.ToTable("BookingServices"); b.HasKey(x => x.BookingServiceId);
            b.Property(x => x.BookingServiceId).ValueGeneratedOnAdd();
            b.HasIndex(x => new { x.BookingId, x.DiagnosticServiceId }).IsUnique();
            b.HasIndex(x => x.DiagnosticServiceId);
            b.HasOne(x => x.Booking).WithMany(x => x.BookingServices)
                .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.DiagnosticService).WithMany(x => x.BookingServices)
                .HasForeignKey(x => x.DiagnosticServiceId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
