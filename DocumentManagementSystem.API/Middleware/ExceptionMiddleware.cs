using DocumentManagementSystem.Core.Exceptions;
using System.Net;
using System.Text.Json;
namespace DocumentManagementSystem.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Something went wrong: {ex.Message}");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var message = "Internal Server Error";
        var details = exception.Message;

        switch (exception)
        {
            case NotFoundException _:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                message = "Not Found";
                break;
            case UnauthorizedAccessException _:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                message = "Unauthorized";
                break;
            case ApplicationException _:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                message = "Bad Request";
                break;
            case ArgumentException _:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                message = "Invalid Argument";
                break;
        }

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = message,
            Details = details,
            StackTrace = context.Response.StatusCode == (int)HttpStatusCode.InternalServerError ? exception.StackTrace : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}