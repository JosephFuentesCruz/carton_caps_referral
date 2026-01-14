using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.Domain;
using System.Net;
using System.Text.Json;

namespace carton_caps_referral.Middlewares
{
    public class ErrorHandlerMiddleware : IMiddleware
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

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

        private static ErrorResponseMapHandler MapErrorResponse(Exception ex)
        {
            switch (ex)
            {
                default:
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Code = "internal_server_error",
                        Message = "An unexpected error occurred.",
                        Details = null,
                        RetryAfter = null
                    };
                case (ApiValidationException apiValidationException):
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Code = "VALIDATION_FAILED",
                        Message = apiValidationException.Message,
                        Details = apiValidationException.Details,
                        RetryAfter = null
                    };
                case (ApiRateLimitException apiRateLimitException):
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.TooManyRequests,
                        Code = "RATE_LIMITED",
                        Message = apiRateLimitException.Message,
                        Details = apiRateLimitException.Details,
                        RetryAfter = apiRateLimitException.RetryAfter
                    };
                case (ApiVendorNotFoundException vendorNotFoundException):
                    return new ErrorResponseMapHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Code = "VENDOR_NOT_FOUND",
                        Message = vendorNotFoundException.Message,
                        Details = vendorNotFoundException.Details,
                        RetryAfter = null
                    };
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
