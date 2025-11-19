using System.Diagnostics;
using PayTR.PosSelection.Shared.Models;

namespace PayTR.PosSelection.API.SeedWork.ExceptionHandling.Helper;

public static class ExceptionHandlingHelper
{
    public static void SetTraceId(this ApiResult<object> details, HttpContext httpContext)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        details.Error.TraceId = traceId;
    }

    public static void IncludeExceptionDetails(this ApiResult<object> details, Exception exception)
    {
        if (exception.InnerException != null)
        {
            details.Error.Extensions["exceptionDetails"] = exception.InnerException?.ToString();
        }
    }

    public static string GetInnerMostExceptionMessage(this Exception exception)
    {
        var innerException = exception.GetInnerMostException();
        return innerException?.Message;
    }

    public static Exception GetInnerMostException(this Exception exception)
    {
        while (exception.InnerException != null)
        {
            exception = exception.InnerException;
        }

        return exception;
    }
}