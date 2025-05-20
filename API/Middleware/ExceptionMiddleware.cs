using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        // prevents app from crashing or leaking any sensitive data
        // advantage is that it shows detailed error in development and genetic messages in production (displayed to the user)
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // awaits execution if there are no further exeptions present
                await next(context);
            }
            catch (Exception ex)
            {
                // log error message and declare its context type as json along with its response statusCode from the http server error
                logger.LogError(ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // if in development mode create a response object - includes stack trace if product don't
                var response = env.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace)
                    : new ApiException(context.Response.StatusCode, ex.Message, "Internal server error");

                // serialize the object into a json string
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);

            }
        }
    }
}
