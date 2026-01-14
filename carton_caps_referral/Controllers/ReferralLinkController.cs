using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.DeepLinks;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace carton_caps_referral.Controllers
{
    [ApiController]
    [Route("v1/referral-links")]
    /// <summary>
    /// Controller that exposes referral-link related endpoints.
    /// The controller route base is <c>/v1/referral-links</c>.
    /// Actions accept parameters via query string or route as defined on each action.
    /// </summary>
    public sealed class ReferralLinkController : Controller
    {
        private readonly IReferralLinkService linkService;

        /// <summary>
        /// Initializes a new instance of <see cref="ReferralLinkController"/>.
        /// </summary>
        /// <param name="linkService">Service used to create and query referral links.</param>
        public ReferralLinkController(IReferralLinkService linkService)
        {
            this.linkService = linkService;
        }

        /// <summary>
        /// Generates a vendor-backed deferred deep link and returns a channel-specific share payload.
        /// Note: this action currently takes the required inputs from the query string.
        /// </summary>
        /// <param name="referrerUserId">Identifier of the user creating the referral.</param>
        /// <param name="referrerReferralCode">Referral code belonging to the referrer.</param>
        /// <param name="channel">The share channel for which to build the payload.</param>
        /// <returns>Returns a <see cref="ReferralLinkResponse"/> containing the generated URL and share payload.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ReferralLinkResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.TooManyRequests)]
        public async Task<ActionResult<ReferralLinkResponse>> CreateLink(
            [FromQuery] string referrerUserId,
            [FromQuery] string referrerReferralCode,
            [FromQuery] ShareChannel channel)
        {
            if (string.IsNullOrWhiteSpace(referrerUserId))
            {
                throw new ApiValidationException("reffererUserId is required.", new { parameterName = "referrerUserId" });
            }

            if (string.IsNullOrWhiteSpace(referrerReferralCode))
            {
                throw new ApiValidationException("referrerReferralCode is required.", new { parameterName = "referrerReferralCode" });
            }
            var response = await this.linkService.CreateReferralLinkAsync(referrerUserId, referrerReferralCode, channel);

            if (response == null)
            {
                throw new ApiVendorNotFoundException("Failed to create referral link due to vendor service error.", null);
            }
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a page of referral links for the given user using cursor pagination.
        /// </summary>
        /// <param name="userId">User identifier to filter referral links.</param>
        /// <param name="limit">Maximum number of items to return in the page.</param>
        /// <param name="nextCursor">Optional opaque cursor to fetch the next page.</param>
        /// <returns>A paged collection of <see cref="ReferralLinkEntity"/> items and an optional next cursor.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ReferralLinkResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<IReadOnlyCollection<ReferralLinkEntity>>> GetLinksByUserId(
            [FromQuery] string userId,
            [FromQuery] int limit,
            [FromQuery] string? nextCursor)
        {
            var response = await this.linkService.GetReferralsByUserId(userId, limit, nextCursor);
            if (response == null || response.Items.Count() == 0)
            {
                throw new ApiNotFoundException($"No referral links found for user with id '{userId}'.", new { userId });
            }
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a single referral link by its identifier.
        /// </summary>
        /// <param name="referralId">The referral identifier (GUID) supplied in the route.</param>
        /// <returns>The matching <see cref="ReferralLinkEntity"/>, or a 404 when not found.</returns>
        [HttpGet("/{referralId:guid}")]
        [ProducesResponseType(typeof(ReferralLinkResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ReferralLinkEntity>> GetLinkById(
            [FromRoute] Guid referralId)
        {
            var response = await this.linkService.GetReferralByIdAsync(referralId);
            if (response == null)
            {
                throw new ApiNotFoundException($"Referral link with id '{referralId}' was not found.", new { referralId });
            }
            return Ok(response);
        }
    }
}
