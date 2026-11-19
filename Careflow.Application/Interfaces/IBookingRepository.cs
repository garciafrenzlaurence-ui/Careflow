using CareFlow.Application.DTOs;
using CareFlow.Domain.Entities;

namespace CareFlow.Application.Interfaces;

public interface IBookingRepository
{
    Task<BookingResultDto> CreateConfirmedAsync(
        Booking booking,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingListItemDto>> GetConfirmedBookingsAsync(
        CancellationToken cancellationToken = default);
}
