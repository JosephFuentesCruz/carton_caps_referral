using carton_caps_referral.Contracts.Common;
using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using System.Collections.Concurrent;
using System.Text;

namespace carton_caps_referral.Repositories.InMemory
{
    public sealed class InMemoryReferralLinkRepository : IReferralLinkRepository
    {
        /// <summary>
        /// In-memory implementation of <see cref="IReferralLinkRepository"/> for testing and development.
        /// Stores referral link entities in a thread-safe <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, ReferralLinkEntity> _store = new();
        /// <summary>
        /// Persists the referral link into the in-memory store.
        /// </summary>
        public Task<ReferralLinkEntity> CreateReferralLink(ReferralLinkEntity referralLink)
        {
            this._store[referralLink.Id] = referralLink;
            return Task.FromResult(referralLink);
        }

        public Task<ReferralLinkEntity?> GetReferralById(Guid referralId)
        {
            return this._store.TryGetValue(referralId, out var referral)
                ? Task.FromResult<ReferralLinkEntity?>(referral)
                : Task.FromResult<ReferralLinkEntity?>(null);
        }

        /// <summary>
        /// Returns a paged list of referral links for a given user using cursor pagination.
        /// </summary>
        public Task<(IReadOnlyList<ReferralLinkEntity> Items, string? NextCursor)> GetReferralsByUserId(string userId, int limit, string? cursor)
        {
            limit = NormalizeLimit(limit);
            var (cursorTicks, cursorId) = DecodeCursor(cursor);

            var query = this._store.Values
                .Where(r => r.ReferrerUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id);

            if (cursorTicks.HasValue && cursorId.HasValue)
            {
                query = query.
                    Where(r =>
                    r.CreatedAt.UtcTicks < cursorTicks.Value ||
                    (r.CreatedAt.UtcTicks == cursorTicks.Value && r.Id.CompareTo(cursorId.Value) < 0))
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id);
            }

            var page = query.Take(limit + 1).ToList();
            string? nextCursor = null;

            if (page.Count > limit)
            {
                var lastItem = page[limit - 1];
                nextCursor = EncodeCursor(lastItem.CreatedAt.UtcTicks, lastItem.Id);
                page = page.Take(limit).ToList();
            }

            return Task.FromResult<(IReadOnlyList<ReferralLinkEntity> Items, string? NextCursor)>((page, nextCursor));
        }

        /// <summary>
        /// Finds a referral by the vendor token.
        /// </summary>
        public Task<ReferralLinkEntity?> GetReferralByVendorToken(string vendorToken)
        {
            var referral = this._store.Values
                .FirstOrDefault(r => r.VendorToken == vendorToken);
            return Task.FromResult<ReferralLinkEntity?>(referral);
        }

        /// <summary>
        /// Ensures limit falls within acceptable bounds.
        /// </summary>
        private static int NormalizeLimit(int limit)
        {
            if (limit <= 0)
            {
                return 20;
            }
            else if (limit > 100)
            {
                return 100;
            }
            return limit;
        }

        /// <summary>
        /// Encodes pagination cursor as base64 of "ticks|guid".
        /// </summary>
        private static string EncodeCursor(long ticks, Guid id)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ticks}|{id}"));
        }

        /// <summary>
        /// Decodes a base64 cursor into ticks and guid parts; throws <see cref="ApiValidationException"/>
        /// on invalid format.
        /// </summary>
        private static (long? ticks, Guid? id) DecodeCursor(string? cursor)
        {
            if (string.IsNullOrWhiteSpace(cursor)) return (null, null);

            try
            {
                var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                var parts = raw.Split('|', 2);
                if (parts.Length != 2) return (null, null);

                return (long.Parse(parts[0]), Guid.Parse(parts[1]));
            }
            catch (Exception ex)
            {
                throw new ApiValidationException("Invalid cursor format.", new { cursorError = ex.GetType().Name });
            }
        }
    }
}
