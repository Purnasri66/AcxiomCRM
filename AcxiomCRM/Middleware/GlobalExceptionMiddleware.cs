using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AcxiomCRM.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = context.Request.Path.StartsWithSegments("/api")
            ? "application/problem+json"
            : "text/html";

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Please contact support if the issue persists.",
                Instance = context.Request.Path
            };

            var json = JsonConvert.SerializeObject(problemDetails);
            await context.Response.WriteAsync(json);
        }
        else
        {
            context.Response.Redirect("/Home/Error");
        }
    }
}
