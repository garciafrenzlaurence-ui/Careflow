# CareFlow Relational Design

```mermaid
erDiagram
    PATIENTS ||--o{ BOOKINGS : has
    BOOKINGS ||--|{ BOOKING_SERVICES : contains
    DIAGNOSTIC_SERVICES ||--o{ BOOKING_SERVICES : selected_in

    PATIENTS {
        int PatientId PK
        string PatientNumber UK
        string FullName
    }
    BOOKINGS {
        int BookingId PK
        int PatientId FK
        date BookingDate
        int Status
        datetime CreatedAt
    }
    DIAGNOSTIC_SERVICES {
        int DiagnosticServiceId PK
        string ServiceName
        int DailyCapacity
    }
    BOOKING_SERVICES {
        int BookingServiceId PK
        int BookingId FK
        int DiagnosticServiceId FK
    }
```

## Relationships and constraints
- One patient can have many bookings; each booking references exactly one patient.
- One booking has one to three booking-service rows.
- Each booking-service row references one diagnostic service.
- A unique index on `(BookingId, DiagnosticServiceId)` prevents a service from appearing twice in the same booking.
- Foreign keys prevent bookings from referencing missing patients or services.
- Availability counts confirmed booking-service rows for the selected booking date.
- Daily capacity is checked again inside the booking transaction before records are committed.
