namespace CareFlow.Application.DTOs;

public class BookingResultDto
{
    public bool Success { get; set; }

    public int? BookingId { get; set; }

    public string Message { get; set; } = string.Empty;

    public static BookingResultDto Successful(int bookingId)
    {
        return new BookingResultDto
        {
            Success = true,
            BookingId = bookingId,
            Message = "Booking confirmed successfully."
        };
    }

    public static BookingResultDto Failed(string message)
    {
        return new BookingResultDto
        {
            Success = false,
            Message = message
        };
    }
}
