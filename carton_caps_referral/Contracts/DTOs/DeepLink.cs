namespace carton_caps_referral.Contracts.DTOs
{
    public sealed record DeepLink
    {
        public required string Url { get; init; }
        public required string VendorToken { get; init; }
    }
}
