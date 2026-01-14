using System.Text.Json.Serialization;

namespace carton_caps_referral.Contracts.Common
{
    public sealed record ErrorResponseMapHandler
    {
        [JsonPropertyName("statusCode")]
        public required int StatusCode { get; init; }
        [JsonPropertyName("code")]
        public required string Code { get; init; }
        [JsonPropertyName("message")]
        public required string Message { get; init; }
        [JsonPropertyName("details")]
        public object? Details { get; init; }
        [JsonPropertyName("retryAfter")]
        public int? RetryAfter { get; init; }
    }
}
