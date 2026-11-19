namespace CareFlow.Application.DTOs;

public class CreateBookingRequest
{
    public int PatientId { get; set; }

    public DateOnly BookingDate { get; set; }

    public List<int> DiagnosticServiceIds { get; set; }
        = new();
}
