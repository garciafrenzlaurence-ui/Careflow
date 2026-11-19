# CareFlow – Diagnostic Booking Management

CareFlow is a C# .NET 10 desktop application built with Avalonia UI, MVVM, Entity Framework Core 10, and SQLite. It supports service availability, multi-service booking, transactional confirmation, booking retrieval, and persistent storage.

## Requirements
- .NET 10 SDK
- Windows, Linux, or macOS desktop environment supported by Avalonia

## Run
1. Open `CareFlow.sln` in Visual Studio or open the `CareFlow.Desktop` project.
2. Restore NuGet packages.
3. Build and run `CareFlow.Desktop`.
4. On first launch, CareFlow creates the local application-data folder, applies the included EF Core migration, and inserts fictional seed data.

The SQLite database is stored at:
- Windows: `%LOCALAPPDATA%\CareFlow\careflow.db`
- Linux: `~/.local/share/CareFlow/careflow.db`
- macOS: `~/Library/Application Support/CareFlow/careflow.db`

## Sample data
- Patient ID: `1`
- Patient number: `P-0001`
- Patient name: `Demo Patient`
- Five diagnostic services, each with a daily capacity of 20.

All patient information is fictional.

## Migrations
The initial migration is included in `Careflow.Infrastructure/Persistence/Migrations` and is applied automatically at startup using `Database.MigrateAsync()`.

To add future migrations from the solution directory, install the EF tool if needed (`dotnet tool install --global dotnet-ef --version 10.0.0`) and run:

```bash
dotnet ef migrations add MigrationName   --project Careflow.Infrastructure   --startup-project CareFlow.Desktop   --output-dir Persistence/Migrations
```

## Business rules
- Existing patient required.
- One to three distinct diagnostic services per booking.
- Booking date cannot be earlier than the local date.
- Each booking-service link consumes one daily capacity unit.
- Capacity is rechecked inside a serializable database transaction immediately before saving.
- Booking and its service links are saved together or rolled back.
- Confirmed bookings persist and can be retrieved after restart.

## Database design
See `docs/relational-design.md`.

## Verification
See `docs/verification-checklist.md`. Scenarios must be marked verified only after testing the running application and persisted SQLite data.

## AI assistance acknowledgment
AI-assisted development support was used to help review requirements, draft and revise implementation code, and prepare supporting documentation. The student team remains responsible for reviewing, understanding, testing, and validating the submitted work.
