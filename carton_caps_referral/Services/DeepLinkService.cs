using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Repositories;

namespace carton_caps_referral.Services
{
    public sealed class DeepLinkService : IDeepLinkService
    {
        /// <summary>
        /// Service responsible for resolving vendor deep links into referral information
        /// and preparing user-facing resolution responses.
        /// </summary>
        private readonly IReferralLinkRepository _referralLink;
        private readonly IReferralRepository _referralSummary;

        /// <summary>
        /// Creates a new instance of <see cref="DeepLinkService"/>.
        /// </summary>
        /// <param name="referralLink">Repository for referral link entities.</param>
        /// <param name="referralSummary">Repository for referral summaries.</param>
        public DeepLinkService(IReferralLinkRepository referralLink, IReferralRepository referralSummary)
        {
            _referralLink = referralLink;
            _referralSummary = referralSummary;
        }

        /// <summary>
        /// Resolves a vendor token into a deep link response. Validates the token,
        /// checks expiration, creates/updates a referral summary, and returns
        /// a user-facing resolution payload.
        /// </summary>
        /// <param name="vendorToken">The vendor-provided token representing the deep link.</param>
        /// <returns>A <see cref="DeepLinkResolveResponseReferred"/> describing the resolved link.</returns>
        /// <exception cref="ApiValidationException">Thrown when the token is invalid, missing, expired, or not found.</exception>
        public async Task<DeepLinkResolveResponseReferred> ResolveAsync(string vendorToken)
        {
            if(string.IsNullOrWhiteSpace(vendorToken))
            {
                throw new ApiValidationException("Vendor token cannot be null or empty.", nameof(vendorToken));
            }

            var referral = await _referralLink.GetReferralByVendorToken(vendorToken);

            if (referral is null)
            {
                throw new ApiValidationException($"No referral found for vendor token: {vendorToken}", nameof(vendorToken));
            }

            if (DateTimeOffset.UtcNow > referral.ExpiresAt)
            {
                throw new ApiValidationException($"The referral link associated with the vendor token: {vendorToken} has expired.", nameof(vendorToken));
            }

            var destinationType = referral.DestinationType ?? DeepLinkDestinationType.AUTH_GATE_DEFAULT;

            await _referralSummary.UpsertReferralSummary(new ReferralSummary
            {
                ReferralId = Guid.NewGuid(),
                ReferrerUserId = referral.ReferrerUserId,
                DisplayName = string.Empty,
                Status = ReferralStatus.PENDING,                
                UpdatedAt = DateTimeOffset.UtcNow,
                ReferralLinkId = referral.Id
            });

            return CreateResolveResponse(referral, destinationType);
        }

        /// <summary>
        /// Builds the <see cref="DeepLinkResolveResponseReferred"/> from a referral entity
        /// and the determined destination type.
        /// </summary>
        /// <param name="referral">The referral link entity used to populate the response.</param>
        /// <param name="destinationType">The destination type for the deep link.</param>
        /// <returns>The populated <see cref="DeepLinkResolveResponseReferred"/>.</returns>
        private DeepLinkResolveResponseReferred CreateResolveResponse(ReferralLinkEntity referral, DeepLinkDestinationType destinationType)
        {
            return new DeepLinkResolveResponseReferred
            {
                IsReferred = true,
                ReferralCode = referral.ReferrerReferralCode,
                Destination = new Destination
                {
                    Type = destinationType,
                    Title = "You've been invited to join Carton Caps!",
                    Body = "Sing up today to start earning for your school and help support America's teachers",
                    PrimaryCta = "Sign up",
                    SecondaryCta = "Sing in"
                },
                PrefillInfo = new PrefillInfo
                {
                    RegistrationReferalCode = referral.ReferrerReferralCode
                },
                ReferralLinkId = referral.Id
            };
        }
    }
}
