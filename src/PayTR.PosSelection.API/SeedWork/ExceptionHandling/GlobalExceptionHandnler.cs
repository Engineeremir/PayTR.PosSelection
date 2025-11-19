using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using PayTR.PosSelection.API.SeedWork.ExceptionHandling.Helper;
using PayTR.PosSelection.Shared.Models;

namespace PayTR.PosSelection.API.SeedWork.ExceptionHandling
{
    public sealed class GlobalExceptionHandnler(IWebHostEnvironment environment) : IExceptionHandler
    {
        private IWebHostEnvironment Environment { get; } = environment;
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var problemDetails = new ApiResult<object>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Title = "General Exception",
                Data = null,
                Error = new ApiProblemDetails
                {
                    Detail = exception.GetInnerMostExceptionMessage(),
                    Type = httpContext.Request.GetDisplayUrl(),
                    Instance = httpContext.Request.Path,
                    ErrorType = (short)ExceptionType.UnhandledException
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
}
