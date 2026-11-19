namespace CareFlow.Application.DTOs;

public class ServiceAvailabilityDto
{
    public int DiagnosticServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public int DailyCapacity { get; set; }

    public int BookedCount { get; set; }

    public int RemainingCapacity =>
        Math.Max(0, DailyCapacity - BookedCount);

    public bool IsAvailable => RemainingCapacity > 0;
}
