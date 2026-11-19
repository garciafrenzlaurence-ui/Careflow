using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;

namespace CareFlow.Application.Services;

public sealed class AvailabilityApplicationService(IDiagnosticServiceRepository repository)
    : IAvailabilityService
{
    public Task<IReadOnlyList<ServiceAvailabilityDto>> GetAvailabilityAsync(
        DateOnly bookingDate, CancellationToken cancellationToken = default)
    {
        if (bookingDate < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("The selected date cannot be earlier than today.");
        return repository.GetAvailabilityAsync(bookingDate, cancellationToken);
    }
}
