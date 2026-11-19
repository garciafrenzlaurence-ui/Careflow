
namespace CareFlow.Domain.Entities;

public class DiagnosticService
{
    public int DiagnosticServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public int DailyCapacity { get; set; }

    public ICollection<BookingService> BookingServices { get; set; }
        = new List<BookingService>();
}

