using CatalogAPI.Exceptions;

namespace CatalogAPI.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger)
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
            catch (NotFoundException ex)
            {
                context.Response.StatusCode =
                    StatusCodes.Status404NotFound;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        error = ex.Message
                    });
            }
            catch (ConflictException ex)
            {
                context.Response.StatusCode =
                    StatusCodes.Status409Conflict;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        error = ex.Message
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred.");

                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        error = "An unexpected error occurred."
                    });
            }
        }
    }

}
