
namespace CareFlow.Domain.Entities;

public class BookingService
{
    public int BookingServiceId { get; set; }

    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public int DiagnosticServiceId { get; set; }

    public DiagnosticService DiagnosticService { get; set; }
        = null!;
}
