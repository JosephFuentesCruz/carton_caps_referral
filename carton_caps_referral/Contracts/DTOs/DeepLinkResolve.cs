using carton_caps_referral.Contracts.Common;
using System.Text.Json.Serialization;

namespace carton_caps_referral.Contracts.DTOs
{
    public sealed record DeepLinkResolveRequest
    {
        [JsonPropertyName("token")]
        public required string Token { get; init; }
    }

    public sealed record Destination
    {
        [JsonPropertyName("type")]
        public required DeepLinkDestinationType Type { get; init; }
        [JsonPropertyName("title")]
        public string? Title { get; init; }
        [JsonPropertyName("body")]
        public string? Body { get; init; }
        [JsonPropertyName("primaryCta")]
        public string? PrimaryCta { get; init; }
        [JsonPropertyName("secondaryCta")]
        public string? SecondaryCta { get; init; }
    }

    public sealed record DeepLinkResolveResponseReferred
    {
        [JsonPropertyName("isReferred")]
        public required bool IsReferred { get; init; }
        [JsonPropertyName("referralCode")]
        public required string ReferralCode { get; init; }
        [JsonPropertyName("destination")]
        public required Destination Destination { get; init; }
        [JsonPropertyName("prefillInfo")]
        public required PrefillInfo PrefillInfo { get; init; }
        [JsonPropertyName("referralLinkId")]
        public required Guid ReferralLinkId { get; init; }
    }

    public sealed record PrefillInfo
    {
        [JsonPropertyName("registrationReferalCode")]
        public required string RegistrationReferalCode { get; init; }
    }
}