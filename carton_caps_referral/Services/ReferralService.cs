using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Repositories;

namespace carton_caps_referral.Services
{
    public sealed class ReferralService : IReferralService
    {
        /// <summary>
        /// Service responsible for retrieving referral summaries for users.
        /// </summary>
        private readonly IReferralRepository _referralSummary;

        /// <summary>
        /// Initializes a new instance of <see cref="ReferralService"/>.
        /// </summary>
        /// <param name="referralSummary">Repository for referral summaries.</param>
        public ReferralService(IReferralRepository referralSummary)
        {
            _referralSummary = referralSummary;
        }

        /// <summary>
        /// Retrieves a paged list of referral summaries for the specified user.
        /// </summary>
        /// <param name="userId">The id of the user whose referrals to fetch.</param>
        /// <param name="limit">Maximum number of items to return for the page.</param>
        /// <param name="cursor">Optional pagination cursor for the next page.</param>
        /// <returns>A <see cref="PagedResponse{ReferralSummary}"/> containing the items and next cursor.</returns>
        public async Task<PagedResponse<ReferralSummary>> GetReferralsByUserId(string userId, int limit, string? cursor)
        {
            var (items, nextCursor) = await _referralSummary.GetReferralSummariesByUserId(userId, limit, cursor);
            return new PagedResponse<ReferralSummary>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }
    }
}
