using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Repositories;
using carton_caps_referral.Services;
using Moq;

namespace carton_caps_referral.Tests.Services
{
    public sealed class ReferralLinkServiceTest
    {
        private readonly Mock<IReferralLinkRepository> _referralLinkRepositoryMock = new(MockBehavior.Strict);
        private readonly Mock<IDeferredLinkVendorRepository> _deferredLinkVendorRepositoryMock = new(MockBehavior.Strict);

        private ReferralLinkService CreateService()
        {
            return new ReferralLinkService(
                _referralLinkRepositoryMock.Object,
                _deferredLinkVendorRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateReferralLinkAsync_WhenVendorThrows_ThrowsApiVendorNotFoundException()
        {
            var userId = "user123";
            var referralCode = "referralCode";
            var channel = ShareChannel.SMS;

            _deferredLinkVendorRepositoryMock
                .Setup(repo => repo.GenerateDeferredLink(referralCode))
                .ThrowsAsync(new KeyNotFoundException("Vendor down"));

            var service = CreateService();

            var ex = await Assert.ThrowsAsync<ApiVendorNotFoundException>(() => service.CreateReferralLinkAsync(userId, referralCode, channel));
            Assert.Contains("unavailable", ex.Message, StringComparison.OrdinalIgnoreCase);

            _deferredLinkVendorRepositoryMock.Verify(v => v.GenerateDeferredLink(referralCode), Times.Once);
            _referralLinkRepositoryMock.VerifyNoOtherCalls();
            _deferredLinkVendorRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateReferralLinAsync_WhenValid_PersistsEntityAndReturnsResponse()
        {
            var userId = "user123";
            var referralCode = "referralCode";
            var channel = ShareChannel.SMS;

            var vendorLink = new DeepLink
            { Url = "https://vendor.com/deeplink", VendorToken = "vendorToken" };

            _deferredLinkVendorRepositoryMock
                .Setup(repo => repo.GenerateDeferredLink(referralCode))
                .ReturnsAsync(vendorLink);

            _referralLinkRepositoryMock
                .Setup(repo => repo.CreateReferralLink(It.IsAny<ReferralLinkEntity>()))
                .ReturnsAsync((ReferralLinkEntity e) => e);

            var service = CreateService();

            var response = await service.CreateReferralLinkAsync(userId, referralCode, channel);

            Assert.Equal(vendorLink.Url, response.ShareUrl);
            Assert.NotNull(response.SharePayload);
            Assert.Equal(channel, response.SharePayload.Channel);
            Assert.Contains(vendorLink.Url, response.SharePayload.Message);
            Assert.Null(response.SharePayload.Subject);

            _referralLinkRepositoryMock.Verify(
                repo => repo.CreateReferralLink(
                    It.Is<ReferralLinkEntity>(entity =>
                        entity.ReferrerUserId == userId &&
                        entity.ReferrerReferralCode == referralCode &&
                        entity.Channel == channel &&
                        entity.VendorToken == vendorLink.VendorToken &&
                        entity.ShareUrl == vendorLink.Url &&
                        entity.DestinationType == DeepLinkDestinationType.AUTH_GATE_REFERRAL &&
                        entity.Id != Guid.Empty)),
                Times.Once);

            _deferredLinkVendorRepositoryMock.Verify(v => v.GenerateDeferredLink(referralCode), Times.Once);
            _referralLinkRepositoryMock.VerifyNoOtherCalls();
            _deferredLinkVendorRepositoryMock.VerifyNoOtherCalls();
        }

        public async Task CreateReferralLinkAsync_WhenEmail_ReturnsSubjectAndEmailMessage()
        {
            var userId = "user123";
            var referralCode = "referralCode";
            var channel = ShareChannel.EMAIL;

            var vendorLink = new DeepLink
            { 
                Url = "https://vendor.com/deeplink",
                VendorToken = "vendorToken" 
            };

            _deferredLinkVendorRepositoryMock
                .Setup(repo => repo.GenerateDeferredLink(referralCode))
                .ReturnsAsync(vendorLink);

            _referralLinkRepositoryMock
                .Setup(repo => repo.CreateReferralLink(It.IsAny<ReferralLinkEntity>()))
                .ReturnsAsync((ReferralLinkEntity e) => e);

            var service = CreateService();

            var response = await service.CreateReferralLinkAsync(userId, referralCode, channel);

            Assert.Equal(vendorLink.Url, response.ShareUrl);
            Assert.Equal(channel, response.SharePayload.Channel);
            Assert.False(string.IsNullOrWhiteSpace(response.SharePayload.Subject));
            Assert.Contains(vendorLink.Url, response.SharePayload.Message);

            _deferredLinkVendorRepositoryMock.Verify(v => v.GenerateDeferredLink(referralCode), Times.Once);
            _referralLinkRepositoryMock.Verify(r => r.CreateReferralLink(It.IsAny<ReferralLinkEntity>()), Times.Once);
            _referralLinkRepositoryMock.VerifyNoOtherCalls();
            _deferredLinkVendorRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetReferralByIdAsync_WhenFound_ReturnsEntity()
        {
            var id = Guid.NewGuid();
            var entity = new ReferralLinkEntity
            {
                Id = id,
                ReferrerUserId = "user123",
                ReferrerReferralCode = "referralCode",
                Channel = ShareChannel.SMS,
                VendorToken = "vendorToken",
                ShareUrl = "https://vendor.com/deeplink",
                DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL
            };

            _referralLinkRepositoryMock
                .Setup(repo => repo.GetReferralById(id))
                .ReturnsAsync(entity);

            var service = CreateService();

            var result = await service.GetReferralByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);

            _referralLinkRepositoryMock.Verify(repo => repo.GetReferralById(id), Times.Once);
            _referralLinkRepositoryMock.VerifyNoOtherCalls();
            _deferredLinkVendorRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetReferralByIdAsync_WhenNotFound_ReturnsNull()
        {
            var id = Guid.NewGuid();

            _referralLinkRepositoryMock
                .Setup(repo => repo.GetReferralById(id))
                .ReturnsAsync((ReferralLinkEntity?)null);

            var service = CreateService();

            var result = await service.GetReferralByIdAsync(id);

            Assert.Null(result);

            _referralLinkRepositoryMock.Verify(repo => repo.GetReferralById(id), Times.Once);
            _referralLinkRepositoryMock.VerifyNoOtherCalls();
            _deferredLinkVendorRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetReferallsByUserId_ReturnsPageResponseWithItemsAndNextCursor()
        {
            var userId = "user123";
            var limit = 20;
            var cursor = "cursor1";

            var items = new List<ReferralLinkEntity>
            {
                new ReferralLinkEntity
                {
                    Id = Guid.NewGuid(),
                    ReferrerUserId = userId,
                    ReferrerReferralCode = "referralCode1",
                    Channel = ShareChannel.SMS,
                    VendorToken = "vendorToken1",
                    ShareUrl = "https://vendor.com/deeplink1",
                    DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL
                }
            };

            var nextCursor = "cursor2";

            _referralLinkRepositoryMock
                .Setup(repo => repo.GetReferralsByUserId(userId, limit, cursor))
                .ReturnsAsync((items, nextCursor));

            var service = CreateService();

            var result = await service.GetReferralsByUserId(userId, limit, cursor);

            Assert.NotNull(result);
            Assert.Equal(items, result.Items);
            Assert.Equal(nextCursor, result.NextCursor);

            _referralLinkRepositoryMock.Verify(
                repo => repo.GetReferralsByUserId(userId, limit, cursor),
                Times.Once);
            _referralLinkRepositoryMock.VerifyNoOtherCalls();
            _deferredLinkVendorRepositoryMock.VerifyNoOtherCalls();
        }
    }
}
