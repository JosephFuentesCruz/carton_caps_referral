using carton_caps_referral.Contracts.DTOs;

namespace carton_caps_referral.Repositories
{
    public interface IReferralRepository
    {
        /// <summary>
        /// Inserts or updates a referral summary record.
        /// </summary>
        /// <param name="referralSummary">The referral summary to upsert.</param>
        /// <returns>The upserted <see cref="ReferralSummary"/>.</returns>
        Task<ReferralSummary> UpsertReferralSummary(ReferralSummary referralSummary);

        /// <summary>
        /// Retrieves referral summaries for the specified referred user.
        /// </summary>
        /// <param name="referredUserId">The id of the referred user to filter by.</param>
        /// <param name="limit">Maximum number of items to return.</param>
        /// <param name="cursor">Optional pagination cursor.</param>
        /// <returns>A tuple containing the items and an optional next cursor.</returns>
        Task<(IReadOnlyList<ReferralSummary> Items, string? NextCursor)> GetReferralSummariesByUserId(string referredUserId, int limit, string? cursor);
    }
}
