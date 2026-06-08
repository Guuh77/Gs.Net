using AgroGuard.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AgroGuard.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = exception switch
        {
            ValidationException validationException => CreateValidationProblem(context, validationException),
            NotFoundException => CreateProblem(context, StatusCodes.Status404NotFound, "Resource not found", exception.Message),
            ConflictException => CreateProblem(context, StatusCodes.Status409Conflict, "Conflict", exception.Message),
            ForbiddenException => CreateProblem(context, StatusCodes.Status403Forbidden, "Forbidden", exception.Message),
            ArgumentException => CreateProblem(context, StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
            _ => CreateProblem(context, StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred.")
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
        }

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static ProblemDetails CreateProblem(HttpContext context, int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
    }

    private static ValidationProblemDetails CreateValidationProblem(HttpContext context, ValidationException exception)
    {
        return new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation error",
            Detail = exception.Message,
            Instance = context.Request.Path
        };
    }
}
