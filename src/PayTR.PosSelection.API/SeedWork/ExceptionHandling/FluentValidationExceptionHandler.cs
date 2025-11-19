using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using PayTR.PosSelection.API.SeedWork.ExceptionHandling.Helper;
using PayTR.PosSelection.Shared.Models;

namespace PayTR.PosSelection.API.SeedWork.ExceptionHandling;

public sealed class FluentValidationExceptionHandler(IWebHostEnvironment environment) : IExceptionHandler
{
    private IWebHostEnvironment Environment { get; } = environment;
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException fluentValidationException)
        {
            return false;
        }

        var problemDetails = new ApiResult<object>
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Title = $"FluentValidationException: {fluentValidationException.Message}",
            Data = null,
            Error = new ApiProblemDetails
            {
                Detail = fluentValidationException.Message,
                Type = httpContext.Request.GetDisplayUrl(),
                Instance = httpContext.Request.Path,
                ErrorType = (short)ExceptionType.ValidationException,
                Extensions = GetErrors(fluentValidationException.Errors)
            }
        };

        problemDetails.SetTraceId(httpContext);

        if (Environment.IsDevelopment())
        {
            problemDetails.IncludeExceptionDetails(exception);
        }

        httpContext.Response.StatusCode = problemDetails.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private Dictionary<string, object?> GetErrors(IEnumerable<ValidationFailure> failures)
    {
        var detectedFailures = new Dictionary<string, object?>();
        var errors = new Dictionary<string, string[]>();
        var failuresGroups = failures.GroupBy(e => e.PropertyName, e => e.ErrorMessage);

        foreach (var failuresGroup in failuresGroups)
        {
            var propertyName = failuresGroup.Key;
            var propertyFailures = failuresGroup.ToArray();

            errors.Add(propertyName, propertyFailures);
        }

        detectedFailures.Add("errors", errors);
        return detectedFailures;
    }
}