using System.Text.Json;
using App.UTIL.Abstractions.DTO.Response;

namespace App.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var rsp = new BaseResponse();
            rsp.SetException(ex, "Unexpected system error", 500);
            context.Response.StatusCode = rsp.Status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(rsp);
        }
    }
}
