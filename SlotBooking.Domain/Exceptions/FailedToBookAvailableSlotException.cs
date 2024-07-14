namespace SlotBooking.Domain.Exceptions;

public class FailedToBookAvailableSlotException : Exception
{
    public FailedToBookAvailableSlotException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}