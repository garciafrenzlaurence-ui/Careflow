using System.Data;
using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public sealed class EfBookingRepository(IDbContextFactory<CareFlowDbContext> contextFactory)
    : IBookingRepository
{
    public async Task<BookingResultDto> CreateConfirmedAsync(
        Booking booking, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        try
        {
            bool patientExists = await db.Patients.AnyAsync(
                x => x.PatientId == booking.PatientId, cancellationToken);
            if (!patientExists)
                return await RollbackFailureAsync(transaction, "The selected patient does not exist.");

            var requestedIds = booking.BookingServices
                .Select(x => x.DiagnosticServiceId).ToList();

            if (requestedIds.Count is < 1 or > 3 || requestedIds.Distinct().Count() != requestedIds.Count)
                return await RollbackFailureAsync(transaction, "A booking must contain one to three different services.");

            var services = await db.DiagnosticServices
                .Where(x => requestedIds.Contains(x.DiagnosticServiceId))
                .ToListAsync(cancellationToken);

            if (services.Count != requestedIds.Count)
                return await RollbackFailureAsync(transaction, "One or more selected diagnostic services do not exist.");

            // Mandatory final availability check occurs inside the transaction,
            // immediately before any booking records are persisted.
            var counts = await db.BookingServices
                .Where(x => requestedIds.Contains(x.DiagnosticServiceId) &&
                            x.Booking.BookingDate == booking.BookingDate &&
                            x.Booking.Status == BookingStatus.Confirmed)
                .GroupBy(x => x.DiagnosticServiceId)
                .Select(group => new { ServiceId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.ServiceId, x => x.Count, cancellationToken);

            foreach (var service in services)
            {
                int booked = counts.GetValueOrDefault(service.DiagnosticServiceId);
                if (booked >= service.DailyCapacity)
                    return await RollbackFailureAsync(transaction,
                        $"{service.ServiceName} has reached its daily capacity. No booking was created.");
            }

            booking.Status = BookingStatus.Confirmed;
            booking.CreatedAt = DateTime.Now;
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return BookingResultDto.Successful(booking.BookingId);
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            return BookingResultDto.Failed(
                "The booking could not be saved. No booking changes were committed. Please try again.");
        }
    }

    public async Task<IReadOnlyList<BookingListItemDto>> GetConfirmedBookingsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var bookings = await db.Bookings.AsNoTracking()
            .Where(x => x.Status == BookingStatus.Confirmed)
            .Include(x => x.Patient)
            .Include(x => x.BookingServices).ThenInclude(x => x.DiagnosticService)
            .OrderByDescending(x => x.BookingDate).ThenByDescending(x => x.BookingId)
            .ToListAsync(cancellationToken);

        return bookings.Select(x => new BookingListItemDto
        {
            BookingId = x.BookingId,
            PatientId = x.PatientId,
            PatientNumber = x.Patient.PatientNumber,
            PatientName = x.Patient.FullName,
            BookingDate = x.BookingDate,
            Status = x.Status.ToString(),
            Services = string.Join(", ", x.BookingServices
                .OrderBy(s => s.DiagnosticServiceId)
                .Select(s => s.DiagnosticService.ServiceName))
        }).ToList();
    }

    private static async Task<BookingResultDto> RollbackFailureAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
        string message)
    {
        await transaction.RollbackAsync(CancellationToken.None);
        return BookingResultDto.Failed(message);
    }
}
