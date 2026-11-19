
namespace CareFlow.Domain.Entities;

public class Patient
{
    public int PatientId { get; set; }

    public string PatientNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}
