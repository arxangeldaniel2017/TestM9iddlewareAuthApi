namespace TestM9iddlewareAuthApi.Middleware
{
    public class PermissionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionsMiddleware> _logger;

        public PermissionsMiddleware(RequestDelegate next, ILogger<PermissionsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1 - if the request is not authenticated, nothing to do
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                // not authorized
                await _next(context);
                return;
            }

            // authorized
            await _next(context);
        }
    }
}