using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.DTOs;

namespace carton_caps_referral.Services
{
    public interface IReferralService
    {
        /// <summary>
        /// Retrieves a paged list of referral summaries for a given user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose referrals to retrieve.</param>
        /// <param name="pageNumber">The page number to fetch (1-based).</param>
        /// <param name="nextCursor">An optional cursor used for paging to the next page.</param>
        /// <returns>A <see cref="PagedResponse{ReferralSummary}"/> wrapped in a <see cref="Task"/>.</returns>
        Task <PagedResponse<ReferralSummary>> GetReferralsByUserId(string userId, int pageNumber, string? nextCursor);
    }
}
