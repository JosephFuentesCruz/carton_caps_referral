using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.DeepLinks;
using carton_caps_referral.Contracts.DTOs;
using System.Collections.ObjectModel;

namespace carton_caps_referral.Services
{
    public interface IReferralLinkService
    {
        /// <summary>
        /// Creates a new referral link for the given referrer.
        /// </summary>
        /// <param name="referrerUserId">The unique identifier of the user creating the referral.</param>
        /// <param name="referrerReferralCode">The referral code associated with the referrer.</param>
        /// <param name="channel">The channel through which the referral will be shared.</param>
        /// <returns>
        /// A <see cref="ReferralLinkResponse"/> wrapped in a <see cref="Task"/>,
        /// containing the created referral link details.
        /// </returns>
        Task<ReferralLinkResponse> CreateReferralLinkAsync(string referrerUserId, string referrerReferralCode, ShareChannel channel);

        /// <summary>
        /// Retrieves a referral link entity by its identifier.
        /// </summary>
        /// <param name="referralId">The identifier of the referral to retrieve.</param>
        /// <returns>
        /// A <see cref="ReferralLinkEntity"/> wrapped in a <see cref="Task"/>, or <c>null</c>
        /// if no referral with the specified id exists.
        /// </returns>
        Task<ReferralLinkEntity?> GetReferralByIdAsync(Guid referralId);

        /// <summary>
        /// Retrieves a paged list of referral links for a specific user.
        /// </summary>
        /// <param name="userId">The user id whose referral links should be returned.</param>
        /// <param name="limit">The maximum number of items to return in the page.</param>
        /// <param name="cursor">An optional pagination cursor for fetching the next page.</param>
        /// <returns>
        /// A <see cref="PagedResponse{T}"/> containing a page of <see cref="ReferralLinkEntity"/>
        /// items wrapped in a <see cref="Task"/>.
        /// </returns>
        Task<PagedResponse<ReferralLinkEntity>> GetReferralsByUserId(string userId, int limit, string? cursor);
    }
}
