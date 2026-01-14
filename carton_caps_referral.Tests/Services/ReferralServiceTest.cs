using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Repositories;
using carton_caps_referral.Services;
using Moq;

namespace carton_caps_referral.Tests.Services
{
    public sealed class ReferralServiceTest
    {
        private readonly Mock<IReferralRepository> _referralRepositoryMock = new(MockBehavior.Strict);

        private ReferralService CreateService()
        {
            return new ReferralService(_referralRepositoryMock.Object);
        }

        [Fact]
        public async Task GetReferralsByUserId_CallsRepositoryWithSameArguments_AndReturnsPagedResponse()
        {
            var userId = "test-user-id";
            var limit = 20;
            var cursor = "c1";

            var items = new List<ReferralSummary>
            {
                new ReferralSummary
                {
                    ReferralId = Guid.NewGuid(),
                    ReferrerUserId = userId,
                    DisplayName = "Jenny S.",
                    Status = ReferralStatus.COMPLETE,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    ReferralLinkId = Guid.NewGuid()
                },
                new ReferralSummary
                {
                    ReferralId = Guid.NewGuid(),
                    ReferrerUserId = userId,
                    DisplayName = "Invited friend",
                    Status = ReferralStatus.PENDING,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    ReferralLinkId = Guid.NewGuid()
                }
            };

            var nextCursor = "c2";

            _referralRepositoryMock
                .Setup(repo => repo.GetReferralSummariesByUserId(userId, limit, cursor))
                .ReturnsAsync((items, nextCursor));

            var service = CreateService();

            var result = await service.GetReferralsByUserId(userId, limit, cursor);

            Assert.NotNull(result);
            Assert.Equal(items, result.Items);
            Assert.Equal(nextCursor, result.NextCursor);

            _referralRepositoryMock.Verify(repo => repo.GetReferralSummariesByUserId(userId, limit, cursor), Times.Once);
            _referralRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetReferralsByUserId_WhenCursorIsNull_PassesNullToRepository()
        {
            var userId = "test-user-id";
            var limit = 10;
            string? cursor = null;

            var items = new List<ReferralSummary>();
            string? nextCursor = null;

            _referralRepositoryMock
                .Setup(repo => repo.GetReferralSummariesByUserId(userId, limit, cursor))
                .ReturnsAsync((items, nextCursor));

            var service = CreateService();

            var result = await service.GetReferralsByUserId(userId, limit, cursor);

            Assert.Empty(result.Items);
            Assert.Null(result.NextCursor);

            _referralRepositoryMock.Verify(repo => repo.GetReferralSummariesByUserId(userId, limit, cursor), Times.Once);
            _referralRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetReferralsByUserId_WhenRepositoryThrows_BubblesException()
        {
            var userId = "user1";
            var limit = 20;
            var cursor = "cursor1";

            _referralRepositoryMock
                .Setup(repo => repo.GetReferralSummariesByUserId(userId, limit, cursor))
                .ThrowsAsync(new ApiValidationException("Invalid cursor", new {cursor}));

            var service = CreateService();

            await Assert.ThrowsAsync<ApiValidationException>(() => service.GetReferralsByUserId(userId, limit, cursor));

            _referralRepositoryMock.Verify(repo => repo.GetReferralSummariesByUserId(userId, limit, cursor), Times.Once);
            _referralRepositoryMock.VerifyNoOtherCalls();
        }
    }
}
