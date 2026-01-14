using carton_caps_referral.Contracts.DTOs;

namespace carton_caps_referral.Services
{
    public interface IDeepLinkService
    {
        /// <summary>
        /// Resolves a deep link provided by an external vendor token.
        /// </summary>
        /// <param name="vendorToken">The vendor-supplied token representing the deep link.</param>
        /// <returns>
        /// A <see cref="DeepLinkResolveResponseReferred"/> wrapped in a <see cref="Task"/>,
        /// containing resolution information including the referred user data.
        /// </returns>
        Task<DeepLinkResolveResponseReferred> ResolveAsync(string vendorToken);
    }
}
