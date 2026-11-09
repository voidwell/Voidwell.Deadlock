using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Voidwell.Deadlock.Analytics;

namespace Voidwell.Deadlock.Api.Filters;

/// <summary>
/// Turns an invalid analytics query into a 400 response that says what was wrong.
/// </summary>
public class AnalyticsRequestExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AnalyticsRequestException exception)
        {
            return;
        }

        context.Result = new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid analytics query",
            Detail = exception.Message
        });
        context.ExceptionHandled = true;
    }
}
