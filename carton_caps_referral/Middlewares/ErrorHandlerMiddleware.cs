using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.Domain;
using System.Net;
using System.Text.Json;

namespace carton_caps_referral.Middlewares
{
    /// <summary>
    /// Middleware that catches unhandled exceptions and converts them into
    /// standardized JSON error responses with proper HTTP status codes.
    /// </summary>
    public class ErrorHandlerMiddleware : IMiddleware
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Invoked per-request. Calls the next middleware and catches any exceptions
        /// to transform them into an <see cref="ErrorResponse"/> payload.
        /// </summary>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {

                await next(context);
            }
            catch (Exception ex)
            {

                string traceId = context.TraceIdentifier;


                ErrorResponseMapHandler errorResponse = MapErrorResponse(ex);


                context.Response.ContentType = "application/json";
                context.Response.StatusCode = errorResponse.StatusCode;

                if (errorResponse.RetryAfter.HasValue)
                {
                    context.Response.Headers["Retry-After"] = errorResponse.RetryAfter.Value.ToString();
                }

                var errorBody = new ErrorResponse
                {
                    Error = new ErrorBody
                    {
                        Code = errorResponse.Code,
                        Message = errorResponse.Message,
                        Details = errorResponse.Details,
                        TraceId = traceId
                    }
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorBody, _jsonOptions));
            }
        }

        /// <summary>
        /// Translates known API exceptions into <see cref="ErrorResponseMapHandler"/>,
        /// which contains an HTTP status, error code, message, optional details and retry information.
        /// </summary>
        private static ErrorResponseMapHandler MapErrorResponse(Exception ex)
        {
            switch (ex)
            {
                // Unexpected exceptions map to 500 Internal Server Error with a generic message.
                default:
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Code = "internal_server_error",
                        Message = "An unexpected error occurred.",
                        Details = null,
                        RetryAfter = null
                    };

                // Validation failures return 400 with details provided by the exception.
                case (ApiValidationException apiValidationException):
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Code = "VALIDATION_FAILED",
                        Message = apiValidationException.Message,
                        Details = apiValidationException.Details,
                        RetryAfter = null
                    };

                // Rate limit exceptions map to 429 Too Many Requests and include Retry-After seconds.
                case (ApiRateLimitException apiRateLimitException):
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.TooManyRequests,
                        Code = "RATE_LIMITED",
                        Message = apiRateLimitException.Message,
                        Details = apiRateLimitException.Details,
                        RetryAfter = apiRateLimitException.RetryAfterSeconds
                    };

                // Vendor not found maps to 404 Not Found.
                case (ApiVendorNotFoundException vendorNotFoundException):
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Code = "VENDOR_NOT_FOUND",
                        Message = vendorNotFoundException.Message,
                        Details = vendorNotFoundException.Details,
                        RetryAfter = null
                    };

                // Token issues map to 422 Unprocessable Entity with details.
                case (ApiTokenInvalidOrExpiredException tokenInvalidOrExpiredException):
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.UnprocessableEntity, // 422 is common
                        Code = "TOKEN_INVALID_OR_EXPIRED",
                        Message = tokenInvalidOrExpiredException.Message,
                        Details = tokenInvalidOrExpiredException.Details,
                        RetryAfter = null
                    };
            }
        }
    }
}
