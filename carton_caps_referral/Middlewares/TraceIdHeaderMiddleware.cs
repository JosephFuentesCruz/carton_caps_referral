
namespace carton_caps_referral.Middlewares
{
    /// <summary>
    /// Middleware that ensures every HTTP response contains an <c>X-Trace-Id</c>
    /// header populated from the current <see cref="HttpContext.TraceIdentifier"/>.
    /// This is useful for correlating client and server logs and tracing requests
    /// across distributed systems.
    /// </summary>
    public sealed class TraceIdHeaderMiddleware : IMiddleware
    {
        /// <summary>
        /// Adds the trace identifier header to the response just before headers are sent,
        /// then invokes the next middleware in the pipeline.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="next">The next middleware delegate.</param>
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
