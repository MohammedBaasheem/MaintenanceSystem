using FluentValidation;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceSystem.API.Middleware;

public class ExceptionHandlingMiddleware
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
        catch (ValidationException ex)
        {
            var validationErrors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = "One or more validation errors occurred.",
                Instance = context.Request.Path.Value
            };
            problemDetails.Extensions["validationErrors"] = validationErrors;

            await WriteProblemDetailsAsync(context, problemDetails);
        }
        catch (InvalidStateTransitionException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid State Transition",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (BusinessRuleViolationException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Business Rule Violation",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (DomainException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Domain Error",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (NotFoundException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (UnauthorizedException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (ConflictException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (LockedException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status423Locked,
                Title = "Locked",
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Detail = ex.Message,
                Instance = context.Request.Path.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing request {Path}", context.Request.Path);

            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path.Value
            });
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, ProblemDetails problemDetails)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
    }
}



