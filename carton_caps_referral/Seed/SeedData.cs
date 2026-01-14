using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.DTOs;
using carton_caps_referral.Repositories;

namespace carton_caps_referral.Seed;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var linkRepo = services.GetRequiredService<IReferralLinkRepository>();
        var referralRepo = services.GetRequiredService<IReferralRepository>();

        var now = DateTimeOffset.UtcNow;

        var link1 = new ReferralLinkEntity
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ReferrerUserId = "mock-user-1",
            ReferrerReferralCode = "XYZG4D",
            Channel = ShareChannel.SMS,
            VendorToken = "seed_tok_1",
            ShareUrl = "https://mock.vendor/dl/seed_tok_1",
            CreatedAt = now.AddMinutes(-30),
            ExpiresAt = now.AddHours(12),
            DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL
        };

        var link2 = new ReferralLinkEntity
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ReferrerUserId = "mock-user-1",
            ReferrerReferralCode = "XYZG4D",
            Channel = ShareChannel.EMAIL,
            VendorToken = "seed_tok_2",
            ShareUrl = "https://mock.vendor/dl/seed_tok_2",
            CreatedAt = now.AddHours(-2),
            ExpiresAt = now.AddHours(6),
            DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL
        };

        var expiredLink = new ReferralLinkEntity
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            ReferrerUserId = "mock-user-2",
            ReferrerReferralCode = "ABCD12",
            Channel = ShareChannel.COPY_LINK,
            VendorToken = "seed_tok_expired",
            ShareUrl = "https://mock.vendor/dl/seed_tok_expired",
            CreatedAt = now.AddDays(-2),
            ExpiresAt = now.AddHours(-1), // expired
            DestinationType = DeepLinkDestinationType.AUTH_GATE_REFERRAL
        };

        await linkRepo.CreateReferralLink(link1);
        await linkRepo.CreateReferralLink(link2);
        await linkRepo.CreateReferralLink(expiredLink);

        // --- Seed: Referral Summaries (these power /v1/referrals “My Referrals”) ---
        // Adjust property names to match your ReferralSummary type exactly.
        await referralRepo.UpsertReferralSummary(new ReferralSummary
        {
            ReferralId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            ReferrerUserId = "mock-user-1",
            DisplayName = "Jenny S.",
            Status = ReferralStatus.COMPLETE,
            UpdatedAt = now.AddDays(-1),
            ReferralLinkId = link2.Id
        });

        await referralRepo.UpsertReferralSummary(new ReferralSummary
        {
            ReferralId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            ReferrerUserId = "mock-user-1",
            DisplayName = "Invited friend",
            Status = ReferralStatus.PENDING,
            UpdatedAt = now.AddMinutes(-10),
            ReferralLinkId = link1.Id
        });

        await referralRepo.UpsertReferralSummary(new ReferralSummary
        {
            ReferralId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            ReferrerUserId = "mock-user-1",
            DisplayName = "Unknown",
            Status = ReferralStatus.EXPIRED,
            UpdatedAt = now.AddDays(-3),
            ReferralLinkId = link1.Id
        });

        // Optional: show an INVALID/INELIGIBLE example (good for “abuse mitigation” story)
        await referralRepo.UpsertReferralSummary(new ReferralSummary
        {
            ReferralId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            ReferrerUserId = "mock-user-1",
            DisplayName = "User",
            Status = ReferralStatus.INVALID,
            UpdatedAt = now.AddDays(-2),
            ReferralLinkId = link2.Id
        });
    }
}
