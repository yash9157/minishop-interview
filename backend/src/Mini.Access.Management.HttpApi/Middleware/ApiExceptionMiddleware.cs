using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
namespace Mini.Access.Management.HttpApi;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, exception.Message);
        }
        catch (ArgumentException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, exception.Message);
        }
        catch (System.Security.SecurityException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, exception.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "A concurrent request changed the same workflow record.");
            await WriteProblemAsync(
                context, StatusCodes.Status409Conflict,
                "This record was changed by another request. Refresh and try again.");
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "A database constraint rejected the request.");
            await WriteProblemAsync(context, 409, "The operation conflicts with existing data.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API error.");
            await WriteProblemAsync(context, 500, "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrases.GetReasonPhrase(status),
            Detail = detail,
            Instance = context.Request.Path
        });
    }
}
