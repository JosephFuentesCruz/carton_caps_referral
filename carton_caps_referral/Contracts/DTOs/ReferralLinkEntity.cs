using carton_caps_referral.Contracts.Common;

namespace carton_caps_referral.Contracts.DTOs
{
    public sealed class ReferralLinkEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string ReferrerUserId { get; init; }
        public required string ReferrerReferralCode { get; init; }
        public required ShareChannel Channel { get; init; }
        public required string VendorToken { get; init; }
        public required string ShareUrl { get; init; }
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(1);
        public DeepLinkDestinationType? DestinationType { get; init; } = DeepLinkDestinationType.AUTH_GATE_REFERRAL;
    }
}
