using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.DTOs;

namespace carton_caps_referral.Repositories
{
    public interface IReferralLinkRepository
    {
        /// <summary>
        /// Persists a new <see cref="ReferralLinkEntity"/>.
        /// </summary>
        /// <param name="referralLink">The entity to persist.</param>
        /// <returns>The persisted <see cref="ReferralLinkEntity"/>.</returns>
        Task<ReferralLinkEntity> CreateReferralLink(ReferralLinkEntity referralLink);

        /// <summary>
        /// Retrieves a page of referral links for a user.
        /// </summary>
        /// <param name="userId">User identifier to filter by.</param>
        /// <param name="limit">Maximum items to return.</param>
        /// <param name="cursor">Optional pagination cursor.</param>
        /// <returns>A tuple of items and an optional next cursor.</returns>
        Task<(IReadOnlyList<ReferralLinkEntity> Items, string? NextCursor)> GetReferralsByUserId(string userId, int limit, string? cursor);

        /// <summary>
        /// Gets a referral link by its id.
        /// </summary>
        /// <param name="referralId">Referral identifier.</param>
        /// <returns>The matching <see cref="ReferralLinkEntity"/>, or <c>null</c> if not found.</returns>
        Task<ReferralLinkEntity?> GetReferralById(Guid referralId);

        /// <summary>
        /// Finds a referral link by the vendor token.
        /// </summary>
        /// <param name="vendorToken">The vendor token associated with the referral.</param>
        /// <returns>The matching <see cref="ReferralLinkEntity"/>, or <c>null</c> if not found.</returns>
        Task<ReferralLinkEntity?> GetReferralByVendorToken(string vendorToken);
    }
}
