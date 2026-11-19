# CareFlow Verification Checklist

Do not mark a scenario verified based only on a successful build or a screenshot. Confirm the runtime result and persisted SQLite data.

| Scenario | Verification procedure | Expected result | Status |
|---|---|---|---|
| AC-01 | Select a date with existing bookings and inspect each service's remaining capacity. | Remaining capacity equals daily capacity minus confirmed booking-service rows for that date. | Not yet runtime-verified |
| AC-02 | Use Patient ID 1, a current/future date, and two available services. | One booking row and both booking-service rows are committed. | Not yet runtime-verified |
| AC-03 | Try Patient ID 9999, no services, four services, a past date, and a full service. | Each invalid request is rejected and no booking rows are added. | Not yet runtime-verified |
| AC-04 | Display availability, then use a second test session or database edit to consume the last available unit before submitting. | Confirmation rechecks capacity and rejects the stale request. | Not yet runtime-verified |
| AC-05 | Use a controlled database failure during persistence (or inspect the transaction boundary and rollback path). | No booking or booking-service partial records remain. | Not yet runtime-verified |
| AC-06 | Create a booking, close the application, reopen it, and refresh the list. | Booking and associated patient/services remain visible. | Not yet runtime-verified |

## Additional checks
- Verify the first-run migration creates all four tables and inserts fictional seed records.
- Verify the application does not display success when initialization or persistence fails.
- Verify that no real patient data or credentials are included.
