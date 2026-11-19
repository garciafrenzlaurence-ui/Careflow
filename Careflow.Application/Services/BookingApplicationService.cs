using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;

namespace CareFlow.Application.Services;

public sealed class BookingApplicationService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDiagnosticServiceRepository _diagnosticRepository;
    private readonly IBookingRepository _bookingRepository;

    public BookingApplicationService(
        IPatientRepository patientRepository,
        IDiagnosticServiceRepository diagnosticRepository,
        IBookingRepository bookingRepository)
    {
        _patientRepository = patientRepository;
        _diagnosticRepository = diagnosticRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingResultDto> CreateBookingAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return BookingResultDto.Failed("Booking information is required.");
        if (request.PatientId <= 0)
            return BookingResultDto.Failed("Please enter a valid patient ID.");
        if (request.BookingDate < DateOnly.FromDateTime(DateTime.Today))
            return BookingResultDto.Failed("The booking date cannot be earlier than today.");
        if (request.DiagnosticServiceIds is null || request.DiagnosticServiceIds.Count == 0)
            return BookingResultDto.Failed("Please select at least one diagnostic service.");
        if (request.DiagnosticServiceIds.Count > 3)
            return BookingResultDto.Failed("A maximum of three diagnostic services is allowed.");
        if (request.DiagnosticServiceIds.Any(id => id <= 0))
            return BookingResultDto.Failed("Invalid diagnostic service selection.");
        if (request.DiagnosticServiceIds.Distinct().Count() != request.DiagnosticServiceIds.Count)
            return BookingResultDto.Failed("Duplicate diagnostic services are not allowed.");

        bool patientExists = await _patientRepository.ExistsAsync(request.PatientId, cancellationToken);
        if (!patientExists)
            return BookingResultDto.Failed("The selected patient does not exist.");

        // This check provides early feedback. The repository repeats the check
        // inside its transaction immediately before saving the booking.
        var availability = await _diagnosticRepository.GetAvailabilityAsync(
            request.BookingDate, cancellationToken);

        foreach (int serviceId in request.DiagnosticServiceIds)
        {
            var service = availability.FirstOrDefault(x => x.DiagnosticServiceId == serviceId);
            if (service is null)
                return BookingResultDto.Failed($"Diagnostic service {serviceId} was not found.");
            if (service.RemainingCapacity <= 0)
                return BookingResultDto.Failed($"{service.ServiceName} has reached its daily capacity.");
        }

        var booking = new Booking
        {
            PatientId = request.PatientId,
            BookingDate = request.BookingDate,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now,
            BookingServices = request.DiagnosticServiceIds.Select(id => new BookingService
            {
                DiagnosticServiceId = id
            }).ToList()
        };

        return await _bookingRepository.CreateConfirmedAsync(booking, cancellationToken);
    }

    public Task<IReadOnlyList<BookingListItemDto>> GetConfirmedBookingsAsync(
        CancellationToken cancellationToken = default) =>
        _bookingRepository.GetConfirmedBookingsAsync(cancellationToken);
}
