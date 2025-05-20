using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RepTrackCommon.Exceptions;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace RepTrackWeb.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred. Please try again later.";
            var redirectUrl = string.Empty;

            // Custom exception handling
            switch (exception)
            {
                case NotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    message = notFoundEx.Message;
                    redirectUrl = "/Home/Index";
                    break;

                case AccessDeniedException accessDeniedEx:
                    statusCode = HttpStatusCode.Forbidden;
                    message = accessDeniedEx.Message;
                    redirectUrl = "/Home/Index";
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    break;

                case InvalidOperationException invalidOpEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = invalidOpEx.Message;
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            // If it's an AJAX request, return JSON response
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Status = (int)statusCode,
                    Message = message,
                    // Only show detailed error info in development
                    Detail = _env.IsDevelopment() ? exception.ToString() : null
                };

                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }
            else
            {
                // For non-AJAX requests, add error to TempData and redirect
                context.Response.ContentType = "text/html";

                // If it's a GET request, redirect to error page or homepage
                if (context.Request.Method == "GET")
                {
                    context.Response.Redirect(redirectUrl.Length > 0
                        ? $"{redirectUrl}?error={System.Web.HttpUtility.UrlEncode(message)}"
                        : $"/Home/Error?message={System.Web.HttpUtility.UrlEncode(message)}");
                }
                else
                {
                    // For POST requests, render an error view
                    await context.Response.WriteAsync($@"
                        <html>
                        <head>
                            <title>Error</title>
                            <link rel='stylesheet' href='/lib/bootstrap/dist/css/bootstrap.min.css' />
                        </head>
                        <body>
                            <div class='container mt-5'>
                                <div class='alert alert-danger'>
                                    <h4>Error</h4>
                                    <p>{message}</p>
                                    <a href='{(redirectUrl.Length > 0 ? redirectUrl : "/Home/Index")}' class='btn btn-primary'>Return to Home</a>
                                </div>
                            </div>
                        </body>
                        </html>");
                }
            }
        }
    }

    // Extension method for easy registration in Program.cs
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}