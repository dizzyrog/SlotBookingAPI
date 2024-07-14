using System.Text.Json;

namespace SlotBooking.API;

/// <summary>
/// Middleware for handling application exceptions and converting them to responses
/// </summary>
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Constructor for initializing a <see cref="ErrorHandlerMiddleware"/> class instance
    /// </summary>
    /// <param name="next">Next middleware in request pipeline</param>
    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Middleware logic for handling exceptions and convert them to the error response
    /// </summary>
    /// <param name="context">Context of the current http request</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpRequestException exception)
        {
            context.Response.StatusCode = exception switch
            {
                // NotFoundException e => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            };

            context.Response.ContentType = "application/json";
            
            var response = JsonSerializer.Serialize(new
            {
                status = context.Response.StatusCode,
                message = exception.Message
            });

            await context.Response.WriteAsync(response);
        }
    }
}
