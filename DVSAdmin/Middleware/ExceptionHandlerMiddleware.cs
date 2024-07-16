using DVSAdmin.CommonUtility;

namespace DVSAdmin.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlerMiddleware> logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError($"An unexpected error occurred: {ex}");
                logger.LogError($"Stacktrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception");
                    Console.WriteLine(String.Concat(ex.InnerException.StackTrace, ex.InnerException.Message));
                }
                // Redirect to error page 
                context.Response.Redirect(Constants.ErrorPath);
            }
        }
    }
}
