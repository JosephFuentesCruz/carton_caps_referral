using System.Text.Json.Serialization;

namespace carton_caps_referral.Contracts.Common
{
    public sealed record ErrorResponse
    {
        [JsonPropertyName("error")]
        public required ErrorBody Error{ get; init; }
    }

    public sealed record ErrorBody
    {
        [JsonPropertyName("code")]
        public required string Code { get; init; } = string.Empty;
        [JsonPropertyName("message")]
        public required string Message { get; init; } = string.Empty;
        [JsonPropertyName("details")]
        public object? Details { get; init; }
        [JsonPropertyName("traceId")]
        public string? TraceId { get; init; }
    }
}
