using carton_caps_referral.Contracts.Domain;
using carton_caps_referral.Contracts.DTOs;
using System.Collections.Concurrent;
using System.Text;

namespace carton_caps_referral.Repositories.InMemory
{
    public class InMemoryReferralRepository : IReferralRepository
    {
        /// <summary>
        /// In-memory implementation of <see cref="IReferralRepository"/> used for testing.
        /// Stores referral summaries in a thread-safe dictionary.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, ReferralSummary> _store = new();
        /// <summary>
        /// Inserts or updates a referral summary in the in-memory store.
        /// </summary>
        public Task<ReferralSummary> UpsertReferralSummary(ReferralSummary referralSummary)
        {
            this._store[referralSummary.ReferralId] = referralSummary;
            return Task.FromResult(referralSummary);
        }

        /// <summary>
        /// Retrieves referral summaries for the specified referred user using cursor pagination.
        /// </summary>
        public Task<(IReadOnlyList<ReferralSummary> Items, string? NextCursor)> GetReferralSummariesByUserId(string referredUserId, int limit, string? cursor)
        {
            limit = NormalizeLimit(limit);
            var (cursorTicks, cursorId) = DecodeCursor(cursor);

            var query = this._store.Values
                .Where(r => r.ReferrerUserId == referredUserId)
                .OrderByDescending(r => r.UpdatedAt)
                .ThenByDescending(r => r.ReferralId);

            if (cursorTicks.HasValue && cursorId.HasValue)
            {
                query = query.
                    Where(r =>
                    r.UpdatedAt.UtcTicks == cursorTicks.Value && r.ReferralId.CompareTo(cursorId.Value) < 0)
                .OrderByDescending(r => r.UpdatedAt)
                .ThenByDescending(r => r.ReferralId);
            }

            var page = query.Take(limit + 1).ToList();
            string? nextCursor = null;

            if (page.Count > limit)
            {
                var lastItem = page[limit - 1];
                nextCursor = EncodeCursor(lastItem.UpdatedAt.UtcTicks, lastItem.ReferralId);
                page = page.Take(limit).ToList();
            }

            return Task.FromResult<(IReadOnlyList<ReferralSummary> Items, string? NextCursor)>((page, nextCursor));
        }

        /// <summary>
        /// Ensures the provided page limit is within allowed bounds.
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
        /// Encodes the pagination cursor as base64 of "ticks|guid".
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
