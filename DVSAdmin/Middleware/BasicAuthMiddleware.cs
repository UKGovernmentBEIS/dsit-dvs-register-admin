﻿using System.Net.Http.Headers;
using System.Text;
using DVSAdmin.CommonUtility;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace DVSAdmin.Middleware
{
    
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BasicAuthMiddlewareConfiguration configuration;
        private readonly ILogger<BasicAuthMiddleware> logger;

        public BasicAuthMiddleware(RequestDelegate next, IOptions<BasicAuthMiddlewareConfiguration> options, ILogger<BasicAuthMiddleware> logger)
        {
            _next = next;
            configuration = options.Value;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {

                if (IsAuthorised(httpContext))
                {
                    await _next.Invoke(httpContext);
                }
                else
                {
                    SendUnauthorisedResponse(httpContext);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An unexpected error occurred: {ex}");
                logger.LogError($"Stacktrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.Write("Inner Exception");
                    Console.Write(String.Concat(ex.InnerException.StackTrace, ex.InnerException.Message));
                }
                // Redirect to error page 
                httpContext.Response.Redirect(Constants.ErrorPath);
            }
        }
        private bool IsAuthorised(HttpContext httpContext)
        {
            try
            {
                bool returnValue = false;
                bool hasAuthorizationHeader = httpContext.Request.Headers.ContainsKey("Authorization");
                if (hasAuthorizationHeader)
                {
                    var authHeader = AuthenticationHeaderValue.Parse(httpContext.Request.Headers["Authorization"]);
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                    var username = credentials[0];
                    var password = credentials[1];

                    returnValue = configuration.Username == username
                           && configuration.Password == password;
                    if (!returnValue)
                    {
                        Console.WriteLine("Basic Auth Details Entered Wrong ");
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                // Default to denying access if anything goes wrong
                Console.WriteLine("Basic Auth Details Entered Wrong " + ex.Message);
                return false;
            }
        }

        private static void SendUnauthorisedResponse(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            AddOrUpdateHeader(httpContext, "WWW-Authenticate", $"Basic realm=DVS-Register-Beta");
        }

        private static void AddOrUpdateHeader(HttpContext httpContext, string headerName, string headerValue)
        {
            if (httpContext.Response.Headers.ContainsKey(headerName))
            {
                httpContext.Response.Headers[headerName] = headerValue;
            }
            else
            {
                httpContext.Response.Headers.Add(headerName, headerValue);
            }
        }
    }


    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class BasicAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddlewareClassTemplate(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}

