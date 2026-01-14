using System.Net;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Services;
using Microsoft.AspNetCore.Mvc;

namespace carton_caps_referral.Controllers
{
    [ApiController]
    [Route("v1/referrals")]
    public class Referral : Controller
    {
        private readonly IReferralService referralService;

        /// <summary>
        /// Creates a new <see cref="Referral"/> controller.
        /// </summary>
        /// <param name="referralService">Service providing referral summary operations.</param>
        public Referral(IReferralService referralService)
        {
            this.referralService = referralService;
        }

        /// <summary>
        /// Returns a paged list of referral summaries for the given user.
        /// </summary>
        /// <param name="userId">The user identifier to filter referrals.</param>
        /// <param name="limit">Maximum number of items to return.</param>
        /// <param name="nextCursor">Optional cursor to continue pagination.</param>
        /// <returns>Paged referral summaries.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(ReferralSummary), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetReferrals(
            [FromQuery] string userId,
            [FromQuery] int limit,
            [FromQuery] string? nextCursor)
        {
            var referrals = await referralService.GetReferralsByUserId(userId, limit, nextCursor);
            if (referrals == null || referrals.Items.Count() == 0)
            {
                throw new ApiNotFoundException($"No referrals found for user with id '{userId}'.", new { userId });
            }
            return Ok(referrals);
        }
    }
}
