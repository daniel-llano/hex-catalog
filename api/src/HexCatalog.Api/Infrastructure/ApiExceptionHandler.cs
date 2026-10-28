using FluentValidation;
using HexCatalog.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HexCatalog.Api.Infrastructure;

/// <summary>Turns application exceptions into RFC 9457 problem responses.</summary>
public sealed class ApiExceptionHandler(IProblemDetailsService problems, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException ve => new ValidationProblemDetails(
                ve.Errors.GroupBy(e => ToCamel(e.PropertyName))
                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more fields are invalid."
            },
            NotFoundException => Problem(StatusCodes.Status404NotFound, "Not found", exception.Message),
            UnauthorizedAccessException => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            ConflictException => Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message),
            DbUpdateConcurrencyException => Problem(StatusCodes.Status409Conflict, "Conflict",
                "Someone else changed this record. Reload it and try again."),
            BusinessRuleException => Problem(StatusCodes.Status422UnprocessableEntity, "Request can't be completed", exception.Message),
            _ => Problem(StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
        };

        if (problem.Status >= 500)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", http.Request.Method, http.Request.Path);

        http.Response.StatusCode = problem.Status!.Value;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private static string ToCamel(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
