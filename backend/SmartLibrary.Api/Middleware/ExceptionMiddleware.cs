using System.Net;
using System.Text.Json;

namespace SmartLibrary.Api.Middleware;

// Custom middleware sits in the ASP.NET Core request pipeline and wraps
// every request. Here, it catches exceptions thrown anywhere further down
// the pipeline (controllers, services, repositories) and converts them into
// consistent JSON error responses instead of leaking raw stack traces to
// the frontend. It's registered once in Program.cs with app.UseMiddleware<ExceptionMiddleware>().
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // ArgumentException / InvalidOperationException are used throughout
        // the Services layer for expected validation failures (bad input,
        // "already checked in", etc.) - these map to 400 Bad Request.
        // Anything else is unexpected and maps to 500.
        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new
        {
            success = false,
            message = statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred. Please try again."
                : exception.Message
        });

        return context.Response.WriteAsync(response);
    }
}
