using carton_caps_referral.Contracts.DTOs;

namespace carton_caps_referral.Repositories
{
    public interface IDeferredLinkVendorRepository
    {
        /// <summary>
        /// Requests a deferred deep link from an external vendor for the given referral code.
        /// </summary>
        /// <param name="referrerReferalCode">The referral code to include in the deferred link.</param>
        /// <returns>A <see cref="DeepLink"/> wrapped in a <see cref="Task"/> containing vendor token and URL.</returns>
        Task<DeepLink> GenerateDeferredLink(string referrerReferalCode);
    }
}
