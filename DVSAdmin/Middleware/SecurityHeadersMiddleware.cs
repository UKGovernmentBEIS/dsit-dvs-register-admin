using System.Net.Http.Headers;
using System.Text;
using DVSAdmin.CommonUtility;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace DVSAdmin.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;//Storing the reference to next middleware in pipeline
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                // Added security headers
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "-1";

                //CSP for inline scripts
                context.Response.Headers["Content-Security-Policy"] =
                "script-src 'unsafe-inline' 'self' https:; " +
                "connect-src 'self'; " +
                "img-src 'self'; " +
                "style-src 'self'; " +
                "base-uri 'self'; " +
                "font-src 'self'; " +
                "form-action 'self';";
            }
            // Calling the next middleware in the pipeline
            await _next(context);
        }
    }
}