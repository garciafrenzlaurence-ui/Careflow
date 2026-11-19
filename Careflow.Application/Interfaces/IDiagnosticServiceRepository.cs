using CareFlow.Application.DTOs;

namespace CareFlow.Application.Interfaces;

public interface IDiagnosticServiceRepository
{
    Task<IReadOnlyList<ServiceAvailabilityDto>>
        GetAvailabilityAsync(
            DateOnly bookingDate,
            CancellationToken cancellationToken = default);
}
