namespace CareFlow.Application.DTOs;

public sealed class BookingListItemDto
{
    public int BookingId { get; init; }
    public int PatientId { get; init; }
    public string PatientNumber { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public DateOnly BookingDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Services { get; init; } = string.Empty;
}
