using CareFlow.Domain.Enums;

namespace CareFlow.Domain.Entities;

public class Booking
{
    public int BookingId { get; set; }

    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    public DateOnly BookingDate { get; set; }

    public BookingStatus Status { get; set; }
        = BookingStatus.Confirmed;

    public DateTime CreatedAt { get; set; }

    public ICollection<BookingService> BookingServices { get; set; }
        = new List<BookingService>();
}
