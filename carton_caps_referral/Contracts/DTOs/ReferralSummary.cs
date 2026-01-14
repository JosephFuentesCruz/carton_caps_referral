using carton_caps_referral.Contracts.Common;

namespace carton_caps_referral.Contracts.DTOs
{
    public sealed record ReferralSummary
    {
        public required Guid ReferralId { get; init; }
        public required string ReferrerUserId { get; init; }
        public required string DisplayName { get; init; }
        public required ReferralStatus Status { get; init; }
        public required DateTimeOffset UpdatedAt { get; init; }
        public Guid? ReferralLinkId { get; init; }
    }
}
