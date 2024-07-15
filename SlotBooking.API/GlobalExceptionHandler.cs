using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SlotBooking.Domain.Exceptions;

namespace SlotBooking.API;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        ProblemDetails problemDetails;
        switch (exception)
        {
            case ArgumentNullException argumentNullException:
                problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Detail = argumentNullException.Message
                };
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case ValidationException validationException:
                problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Detail = validationException.Errors.Select(error => error.ErrorMessage)
                        .Aggregate((current, next) => $"{current}; {next}")
                };
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case FailedToBookAvailableSlotException failedToBookAvailableSlotException:
                problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Detail = failedToBookAvailableSlotException.Message
                };
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case SlotNotAvailableException slotNotAvailableException:
                problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Detail = slotNotAvailableException.Message
                };
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            default:
                problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server Error",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    Detail = exception.Message
                };
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}