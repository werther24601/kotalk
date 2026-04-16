using System.Text.Json;
using PhysOn.Application.Exceptions;
using PhysOn.Contracts.Common;

namespace PhysOn.Api.Infrastructure;

public static class ApplicationExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UsePhysOnExceptionHandling(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (AppException exception)
            {
                context.Response.StatusCode = (int)exception.StatusCode;
                context.Response.ContentType = "application/json";
                var payload = new ApiErrorEnvelope(new ApiError(
                    exception.Code,
                    exception.Message,
                    exception.Retryable,
                    exception.FieldErrors));
                await JsonSerializer.SerializeAsync(context.Response.Body, payload);
            }
        });
}
