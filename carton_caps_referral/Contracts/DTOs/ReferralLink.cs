using carton_caps_referral.Contracts.Common;
using System.Text.Json.Serialization;

namespace carton_caps_referral.Contracts.DeepLinks
{
    public sealed class SharePayload
    {
        [JsonPropertyName("channel")]
        public required ShareChannel Channel { get; init; }
        [JsonPropertyName("subject")]
        public string? Subject { get; init; }
        [JsonPropertyName("message")]
        public required string Message { get; init; }
    }

    public sealed class ReferralLinkResponse
    {
        [JsonPropertyName("referralLinkId")]
        public required Guid ReferralLinkId { get; init; }
        [JsonPropertyName("shareUrl")]
        public required string ShareUrl { get; init; }
        [JsonPropertyName("expiresAt")]
        public DateTimeOffset? ExpiresAt { get; init; } = DateTimeOffset.UtcNow.AddDays(1);
        [JsonPropertyName("sharePayload")]
        public required SharePayload SharePayload { get; init; }
    }
}
