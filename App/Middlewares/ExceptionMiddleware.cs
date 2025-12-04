using System.Net;
using System.Security;
using System.Text.Json;
using App.UTIL.Abstractions.DTO.Response;
using Microsoft.EntityFrameworkCore; // nếu dùng EF

namespace App.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            var traceId = context.Items["TraceId"]?.ToString();
            var requestId = context.Items["RequestId"]?.ToString();

            var (status, message) = ex switch
            {
                ArgumentException or ArgumentNullException => ((int)HttpStatusCode.BadRequest, "Invalid request"),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found"),
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
                SecurityException => ((int)HttpStatusCode.Forbidden, "Forbidden"),
                DbUpdateConcurrencyException => ((int)HttpStatusCode.Conflict, "Conflict"),
                _ => ((int)HttpStatusCode.InternalServerError, "Unexpected system error")
            };

            _logger.LogError(ex, "Unhandled exception. TraceId={TraceId} RequestId={RequestId}", traceId, requestId);

            var rsp = new BaseResponse();
            rsp.SetException(ex, message, status);
            rsp.ExtensionData["TraceId"] = traceId;   // nếu BaseResponse có field này
            rsp.ExtensionData["RequestId"] = requestId;

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(rsp);
        }
    }
}