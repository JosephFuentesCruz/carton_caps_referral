
namespace carton_caps_referral.Middlewares
{
    public sealed class TraceIdHeaderMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.OnStarting(() =>
            {
                var traceId = context.TraceIdentifier;
                context.Response.Headers["X-Trace-Id"] = traceId;
                return Task.CompletedTask;
            });

            await next(context);
        }
    }
}
