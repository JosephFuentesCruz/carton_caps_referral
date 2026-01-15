namespace carton_caps_referral.Contracts.Domain
{
    public abstract class ApiExceptionHandler : Exception
    {
        public object? Details { get; init; }

        protected ApiExceptionHandler(string message, object? details = null) : base(message)
        {
            Details = details;
        }
    }

    public sealed class ApiValidationException : ApiExceptionHandler
    {
        public ApiValidationException(string message, object? details = null) : base(message, details)
        {
        }
    }

    public sealed class  ApiRateLimitException : ApiExceptionHandler
    {
        public int? RetryAfterSeconds { get; init; } = 60;

        public ApiRateLimitException(string message, object? details = null, int? retryAfter = null) : base(message, details)
        {
                RetryAfterSeconds = retryAfter;
        }
    }

    public sealed class ApiVendorNotFoundException : ApiExceptionHandler
    {
        public ApiVendorNotFoundException(string message, object? details = null) : base(message, details)
        {
        }
    }
    public sealed class ApiTokenInvalidOrExpiredException : ApiExceptionHandler
    {
        public ApiTokenInvalidOrExpiredException(string message, object? details = null) : base(message, details)
        {
        }
    }

    public sealed class ApiNotFoundException : ApiExceptionHandler
    {
        public ApiNotFoundException(string message, object? details = null) : base(message, details)
        {
        }
    }
}
