namespace CareFlow.Application.DTOs;

public sealed class PatientOptionDto
{
    public int PatientId { get; init; }
    public string PatientNumber { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string DisplayName => $"{PatientNumber} - {FullName}";
}
