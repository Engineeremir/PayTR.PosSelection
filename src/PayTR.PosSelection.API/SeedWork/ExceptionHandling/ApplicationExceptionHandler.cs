using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using PayTR.PosSelection.API.SeedWork.ExceptionHandling.Helper;
using PayTR.PosSelection.Shared.Models;
using PayTR.PosSelection.Shared.SeedWork;

namespace PayTR.PosSelection.API.SeedWork.ExceptionHandling;

public sealed class ApplicationExceptionHandler(IWebHostEnvironment environment) : IExceptionHandler
{
    private IWebHostEnvironment Environment { get; } = environment;
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ApplicationGeneralException applicationException)
        {
            return false;
        }

        var problemDetails = new ApiResult<object>
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Title = $"ApplicationGeneralException: {applicationException.Message}",
            Data = null,
            Error = new ApiProblemDetails
            {
                Detail = applicationException.Message,
                Type = httpContext.Request.GetDisplayUrl(),
                Instance = httpContext.Request.Path,
                ErrorType = (short)ExceptionType.ApplicationException
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
}