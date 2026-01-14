using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Repositories;
using carton_caps_referral.Services;
using Moq;

namespace carton_caps_referral.Tests.Services
{
    public sealed class DeepLinkServiceTest
    {
        private readonly Mock<IReferralLinkRepository> _referralLinkMock = new(MockBehavior.Strict);
        private readonly Mock<IReferralRepository> _referralSummaryMock = new(MockBehavior.Strict);

        private DeepLinkService CreateService()
        {
            return new DeepLinkService(_referralLinkMock.Object, _referralSummaryMock.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ResolveAsync_WhenTokenIsNullOrWhitespace_ThrowsApiValidationException(string invalidToken)
        {
            var service = CreateService();

            var exception = await Assert.ThrowsAsync<ApiValidationException>(() => service.ResolveAsync(invalidToken));

            Assert.Contains("Vendor token cannot be null or empty", exception.Message, StringComparison.OrdinalIgnoreCase);
            _referralLinkMock.VerifyNoOtherCalls();
            _referralSummaryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ResolveAsync_WhenTokenNotFound_ThrowsApiValidationException()
        {
            var token = "abcd1234";

            _referralLinkMock
                .Setup(repo => repo.GetReferralByVendorToken(token))
                .ReturnsAsync((ReferralLinkEntity?)null);

            var service = CreateService();

            var ex = await Assert.ThrowsAsync<ApiValidationException>(() => service.ResolveAsync(token));
            Assert.Contains("No referral found", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(token, ex.Message);

            _referralLinkMock.Verify(repo => repo.GetReferralByVendorToken(token), Times.Once);
            _referralLinkMock.VerifyNoOtherCalls();
            _referralSummaryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ResolveAsync_WhenReferralExpired_ThrowsApiValidationException()
        {
            var token = "abcd1234";
            var referralLink = new ReferralLinkEntity
            {
                Id = Guid.NewGuid(),
                ReferrerUserId = "user123",
                ReferrerReferralCode = "refcode123",
                Channel = ShareChannel.EMAIL,
                VendorToken = token,
                ShareUrl = "https://example.com/referral",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL
            };

            _referralLinkMock
                .Setup(repo => repo.GetReferralByVendorToken(token))
                .ReturnsAsync(referralLink);

            var service = CreateService();
            var ex = await Assert.ThrowsAsync<ApiValidationException>(() => service.ResolveAsync(token));
            Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);

            _referralLinkMock.Verify(repo => repo.GetReferralByVendorToken(token), Times.Once);
            _referralLinkMock.VerifyNoOtherCalls();
            _referralSummaryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ResolveAsync_WhenValid_CallsUpsertAndReturnsExpectedResponse()
        {
            var token = "abcd1234";
            var linkId = Guid.NewGuid();
            var referral = new ReferralLinkEntity
            {
                Id = linkId,
                ReferrerUserId = "user123",
                ReferrerReferralCode = "refcode123",
                Channel = ShareChannel.EMAIL,
                VendorToken = token,
                ShareUrl = "https://example.com/referral",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
                DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL
            };

            _referralLinkMock
                .Setup(repo => repo.GetReferralByVendorToken(token))
                .ReturnsAsync(referral);

            _referralSummaryMock
                .Setup(repo => repo.UpsertReferralSummary(It.IsAny<ReferralSummary>()))
                .ReturnsAsync((ReferralSummary s) => s);

            var service = CreateService();
            var result = await service.ResolveAsync(token);

            Assert.True(result.IsReferred);
            Assert.Equal(referral.ReferrerReferralCode, result.ReferralCode);
            Assert.Equal(linkId, result.ReferralLinkId);

            Assert.NotNull(result.Destination);
            Assert.Equal(DeepLinkDestinationType.AUTH_GATE_REFERRAL, result.Destination.Type);

            Assert.NotNull(result.PrefillInfo);
            Assert.Equal(referral.ReferrerReferralCode, result.PrefillInfo.RegistrationReferalCode);

            _referralSummaryMock.Verify(r => r.UpsertReferralSummary(It.Is<ReferralSummary>(s =>
            s.ReferrerUserId == referral.ReferrerUserId &&
            s.ReferralLinkId == referral.Id &&
            s.Status == ReferralStatus.PENDING)),
            Times.Once);

            _referralLinkMock.Verify(r => r.GetReferralByVendorToken(token), Times.Once);
            _referralSummaryMock.VerifyNoOtherCalls();
            _referralLinkMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ResolveAsync_WhenDestinationTypeNull_DefaultToAuthGateDefault()
        {
            var token = "abc1234";
            var referral = new ReferralLinkEntity
            {
                Id = Guid.NewGuid(),
                ReferrerUserId = "12345",
                ReferrerReferralCode = "67890",
                Channel = ShareChannel.SMS,
                VendorToken = token,
                ShareUrl = $"https://mock.vendor.com/dl/{token}",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                DestinationType = null
            };

            _referralLinkMock
                .Setup(repo => repo.GetReferralByVendorToken(token))
                .ReturnsAsync(referral);

            _referralSummaryMock
                .Setup(repo => repo.UpsertReferralSummary(It.IsAny<ReferralSummary>()))
                .ReturnsAsync((ReferralSummary s) => s);

            var service = CreateService();
            var result = await service.ResolveAsync(token);

            Assert.Equal(DeepLinkDestinationType.AUTH_GATE_DEFAULT, result.Destination.Type);

            _referralSummaryMock.Verify(r => r.UpsertReferralSummary(It.IsAny<ReferralSummary>()), Times.Once);
            _referralLinkMock.Verify(r => r.GetReferralByVendorToken(token), Times.Once);
            _referralSummaryMock.VerifyNoOtherCalls();
            _referralLinkMock.VerifyNoOtherCalls();
        }
    }
}
