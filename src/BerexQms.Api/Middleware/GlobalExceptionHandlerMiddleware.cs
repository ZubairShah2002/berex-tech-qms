using System.Net;
using System.Text.Json;
using BerexQms.Application.Exceptions;
using BerexQms.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Validation Failed",
                "One or more validation errors occurred.",
                ve.Errors),

            NotFoundException nfe => (
                HttpStatusCode.NotFound,
                "Not Found",
                nfe.Message,
                (IReadOnlyDictionary<string, string[]>?)null),

            ForbiddenAccessException => (
                HttpStatusCode.Forbidden,
                "Forbidden",
                "You do not have permission to perform this action.",
                (IReadOnlyDictionary<string, string[]>?)null),

            ConflictException ce => (
                HttpStatusCode.Conflict,
                "Conflict",
                ce.Message,
                (IReadOnlyDictionary<string, string[]>?)null),

            BusinessRuleException bre => (
                HttpStatusCode.UnprocessableEntity,
                "Business Rule Violation",
                bre.Message,
                (IReadOnlyDictionary<string, string[]>?)null),

            DomainException de => (
                HttpStatusCode.UnprocessableEntity,
                "Domain Error",
                de.Message,
                (IReadOnlyDictionary<string, string[]>?)null),

            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.",
                (IReadOnlyDictionary<string, string[]>?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception ({StatusCode}): {Message}",
                (int)statusCode, exception.Message);
        }

        var correlationId = context.Items["CorrelationId"]?.ToString();

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
        };

        if (correlationId is not null)
            problemDetails.Extensions["correlationId"] = correlationId;

        if (errors is not null)
            problemDetails.Extensions["errors"] = errors;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }
}
