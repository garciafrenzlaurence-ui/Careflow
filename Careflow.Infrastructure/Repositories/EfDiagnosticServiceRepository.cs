using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Enums;
using CareFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public sealed class EfDiagnosticServiceRepository(
    IDbContextFactory<CareFlowDbContext> contextFactory) : IDiagnosticServiceRepository
{
    public async Task<IReadOnlyList<ServiceAvailabilityDto>> GetAvailabilityAsync(
        DateOnly bookingDate, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var services = await db.DiagnosticServices.AsNoTracking()
            .OrderBy(x => x.DiagnosticServiceId).ToListAsync(cancellationToken);

        var bookedCounts = await db.BookingServices.AsNoTracking()
            .Where(x => x.Booking.BookingDate == bookingDate &&
                        x.Booking.Status == BookingStatus.Confirmed)
            .GroupBy(x => x.DiagnosticServiceId)
            .Select(group => new { ServiceId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.ServiceId, x => x.Count, cancellationToken);

        return services.Select(service => new ServiceAvailabilityDto
        {
            DiagnosticServiceId = service.DiagnosticServiceId,
            ServiceName = service.ServiceName,
            DailyCapacity = service.DailyCapacity,
            BookedCount = bookedCounts.GetValueOrDefault(service.DiagnosticServiceId)
        }).ToList();
    }
}
