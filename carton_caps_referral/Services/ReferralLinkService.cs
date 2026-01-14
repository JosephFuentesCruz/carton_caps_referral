using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.DeepLinks;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Repositories;

namespace carton_caps_referral.Services
{
    public sealed class ReferralLinkService : IReferralLinkService
    {
        /// <summary>
        /// Service responsible for creating and retrieving referral links,
        /// interfacing with vendor deferred link provider and referral repositories.
        /// </summary>
        private readonly IReferralLinkRepository referralLinkRepository;
        private readonly IDeferredLinkVendorRepository deferredLinkVendorRepository;

        /// <summary>
        /// Initializes a new instance of <see cref="ReferralLinkService"/>.
        /// </summary>
        /// <param name="referralLinkRepository">Repository for storing referral link entities.</param>
        /// <param name="deferredLinkVendor">Vendor repository used to generate deferred vendor links.</param>
        public ReferralLinkService(IReferralLinkRepository referralLinkRepository, IDeferredLinkVendorRepository deferredLinkVendor)
        {
            this.referralLinkRepository = referralLinkRepository;
            this.deferredLinkVendorRepository = deferredLinkVendor;
        }

        /// <summary>
        /// Creates a referral link by requesting a vendor deferred link, persisting
        /// the generated referral record, and returning a response containing the
        /// share URL and payload.
        /// </summary>
        /// <param name="referrerUserId">The id of the user creating the referral.</param>
        /// <param name="referrerReferralCode">The referral code for the referrer.</param>
        /// <param name="channel">The channel chosen for sharing the link.</param>
        /// <returns>A <see cref="ReferralLinkResponse"/> wrapped in a <see cref="Task"/>.</returns>
        public async Task<ReferralLinkResponse> CreateReferralLinkAsync(string referrerUserId, string referrerReferralCode, ShareChannel channel)
        {
            DeepLink vendorLink;
            try
            {
                vendorLink = await deferredLinkVendorRepository.GenerateDeferredLink(referrerReferralCode);
            }
            catch (Exception ex)
            {
                throw new ApiVendorNotFoundException("Deep link vendor service is unavailable at this moment, please retry.", new { vendorError = ex.GetType().Name });
            }

            var entity = GenerateReferrallLink(
                referrerUserId,
                referrerReferralCode,
                channel,
                vendorLink.VendorToken,
                vendorLink.Url);

            await this.referralLinkRepository.CreateReferralLink(entity);

            return new ReferralLinkResponse
            {
                ReferralLinkId = entity.Id,
                ShareUrl = vendorLink.Url,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
                SharePayload = BuildSharePayload(channel, vendorLink.Url)
            };
        }

        /// <summary>
        /// Retrieves a referral link entity by its id.
        /// </summary>
        /// <param name="referralId">The referral identifier.</param>
        /// <returns>The matching <see cref="ReferralLinkEntity"/>, or <c>null</c> if not found.</returns>
        public async Task<ReferralLinkEntity?> GetReferralByIdAsync(Guid referralId)
        {
            var referralLink = await this.referralLinkRepository.GetReferralById(referralId);
            return referralLink;
        }

        /// <summary>
        /// Retrieves a paged list of referral links for a specific user.
        /// </summary>
        /// <param name="userId">User identifier to filter referral links.</param>
        /// <param name="limit">Maximum number of items to return.</param>
        /// <param name="cursor">Optional pagination cursor to continue from.</param>
        /// <returns>A <see cref="PagedResponse{ReferralLinkEntity}"/> containing the results.</returns>
        public async Task<PagedResponse<ReferralLinkEntity>> GetReferralsByUserId(string userId, int limit, string? cursor)
        {
            var (items, nextCursor) = await this.referralLinkRepository.GetReferralsByUserId(userId, limit, cursor);
            return new PagedResponse<ReferralLinkEntity>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }

        /// <summary>
        /// Builds a <see cref="ReferralLinkEntity"/> from provided parameters.
        /// </summary>
        private ReferralLinkEntity GenerateReferrallLink(string referrerUserId, string referrerReferralCode, ShareChannel channel, string vendorToken, string url)
        {
            return new ReferralLinkEntity
            {
                Id = Guid.NewGuid(),
                ReferrerUserId = referrerUserId,
                ReferrerReferralCode = referrerReferralCode,
                Channel = channel,
                VendorToken = vendorToken,
                ShareUrl = url,
                DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL,
            };
        }

        private static SharePayload BuildSharePayload(ShareChannel channel, string url)
        {
            switch (channel)
            {
                default:
                    return new SharePayload
                    {
                        Channel = channel,
                        Message = $"Hi! Join me in earning money for our school by using Carton Caps app. It's an easy way to make a difference. Use the link below to download the Carton Caps app: {url}",
                        Subject = null
                    };
                case ShareChannel.EMAIL:
                    return new SharePayload
                    {
                        Channel = channel,
                        Message = $"Join me in earning cash for our school using Carton Caps. It's an easy way to make a difference. All you have to do is buy Carton Caps participating products (like Cheerios!) and scan your grocery receipt.Carton Caps worth $.10 each and they add up fast! Twice a year, our school receives a check to help pay whatever we need - equipment supplies or experiences the kids love! Download the Carton Caps app here: {url}",
                        Subject = "You’re invited to try the Carton Caps app!"
                    };
            }
            ;
        }
    }
}
