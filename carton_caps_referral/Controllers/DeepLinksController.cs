using System.Net;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Services;
using Microsoft.AspNetCore.Mvc;

namespace carton_caps_referral.Controllers
{
    [ApiController]
    [Route("api/deeplinks")]
    public class DeepLinksController : Controller
    {
        private readonly IDeepLinkService _deepLinkService;

        /// <summary>
        /// Initializes a new instance of <see cref="DeepLinksController"/>.
        /// </summary>
        /// <param name="deepLinkService">Service responsible for resolving vendor deep links.</param>
        public DeepLinksController(IDeepLinkService deepLinkService)
        {
            _deepLinkService = deepLinkService;
        }

        /// <summary>
        /// Public endpoint that resolves a deferred deep link token into a referral resolution.
        /// This endpoint is intentionally public to be callable on first app launch after install.
        /// </summary>
        /// <param name="request">The deep link resolve request containing the vendor token.</param>
        /// <returns>A <see cref="DeepLinkResolveResponseReferred"/> when referred (200 OK).</returns>
        [HttpPost("resolve")]
        [ProducesResponseType(typeof(DeepLinkResolveResponseReferred), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<DeepLinkResolveResponseReferred>> Resolve([FromBody] DeepLinkResolveRequest request)
        {
            var result = await _deepLinkService.ResolveAsync(request.Token);
            if (result == null)
            {
                throw new ApiValidationException("Failed to resolve deep link with the provided token.", new { parameterName = "token" });
            }
            return Ok(result);
        }
    }
}
