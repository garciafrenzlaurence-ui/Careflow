using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CareFlow.Infrastructure.Persistence;
using CareFlow.Domain.Entities;

#nullable disable

namespace CareFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CareFlowDbContext))]
[Migration("202611190001_InitialCareFlowDatabase")]
public partial class InitialCareFlowDatabase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("DiagnosticServices", table => new
        {
            DiagnosticServiceId = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            ServiceName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
            DailyCapacity = table.Column<int>(type: "INTEGER", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_DiagnosticServices", x => x.DiagnosticServiceId));

        migrationBuilder.CreateTable("Patients", table => new
        {
            PatientId = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            PatientNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
            FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Patients", x => x.PatientId));

        migrationBuilder.CreateTable("Bookings", table => new
        {
            BookingId = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            PatientId = table.Column<int>(type: "INTEGER", nullable: false),
            BookingDate = table.Column<string>(type: "TEXT", nullable: false),
            Status = table.Column<int>(type: "INTEGER", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Bookings", x => x.BookingId);
            table.ForeignKey("FK_Bookings_Patients_PatientId", x => x.PatientId, "Patients", "PatientId", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateTable("BookingServices", table => new
        {
            BookingServiceId = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            BookingId = table.Column<int>(type: "INTEGER", nullable: false),
            DiagnosticServiceId = table.Column<int>(type: "INTEGER", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BookingServices", x => x.BookingServiceId);
            table.ForeignKey("FK_BookingServices_Bookings_BookingId", x => x.BookingId, "Bookings", "BookingId", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_BookingServices_DiagnosticServices_DiagnosticServiceId", x => x.DiagnosticServiceId, "DiagnosticServices", "DiagnosticServiceId", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.InsertData("Patients", new[] { "PatientId", "FullName", "PatientNumber" }, new object[] { 1, "Demo Patient", "P-0001" });
        migrationBuilder.InsertData("DiagnosticServices", new[] { "DiagnosticServiceId", "DailyCapacity", "ServiceName" }, new object[,]
        {
            { 1, 20, "Complete Blood Count (CBC)" },
            { 2, 20, "Urinalysis" },
            { 3, 20, "Chest X-Ray" },
            { 4, 20, "Blood Chemistry" },
            { 5, 20, "Electrocardiogram (ECG)" }
        });

        migrationBuilder.CreateIndex("IX_BookingServices_BookingId_DiagnosticServiceId", "BookingServices", new[] { "BookingId", "DiagnosticServiceId" }, unique: true);
        migrationBuilder.CreateIndex("IX_BookingServices_DiagnosticServiceId", "BookingServices", "DiagnosticServiceId");
        migrationBuilder.CreateIndex("IX_Bookings_BookingDate_Status", "Bookings", new[] { "BookingDate", "Status" });
        migrationBuilder.CreateIndex("IX_Bookings_PatientId", "Bookings", "PatientId");
        migrationBuilder.CreateIndex("IX_Patients_PatientNumber", "Patients", "PatientNumber", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("BookingServices");
        migrationBuilder.DropTable("Bookings");
        migrationBuilder.DropTable("DiagnosticServices");
        migrationBuilder.DropTable("Patients");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
