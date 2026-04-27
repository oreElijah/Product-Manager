using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Exceptions
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger; 

       public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
       {
            _next = next;
            _logger = logger;
       }

       public async Task InvokeAsync(HttpContext context)
        {
            try{
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                var statuscode = ex switch
                {
                    KeyNotFoundException => 404,
                    UnauthorizedAccessException => 401,
                    _ => 500
                };
                
                context.Response.StatusCode = statuscode;
                context.Response.ContentType = "application/json";

                var response = new { 
                    message = ex.Message,
                    statusCode = statuscode
                     };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}