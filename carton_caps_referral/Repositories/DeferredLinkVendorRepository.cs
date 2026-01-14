using carton_caps_referral.Contracts.DTOs;

namespace carton_caps_referral.Repositories
{
    public class DeferredLinkVendorRepository : IDeferredLinkVendorRepository
    {
        /// <summary>
        /// Simple vendor repository that generates mock deferred deep links.
        /// Used in development and testing to simulate an external vendor.
        /// </summary>
        public Task<DeepLink> GenerateDeferredLink(string referrerReferalCode)
        {
            string token = GenerateVendorToken();
            string url = $"https://mock.deferred-example.link/{token}?referral_Code={referrerReferalCode}";
            return Task.FromResult(new DeepLink { Url = url, VendorToken = token });
        }

        /// <summary>
        /// Generates a URL-safe base64 token to simulate a vendor token.
        /// </summary>
        private string GenerateVendorToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
