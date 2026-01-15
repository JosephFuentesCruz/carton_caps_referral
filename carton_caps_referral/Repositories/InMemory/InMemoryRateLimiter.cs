using carton_caps_referral.Contracts.Common;
using System.Collections.Concurrent;

namespace carton_caps_referral.Repositories.InMemory
{
    public sealed class InMemoryRateLimiter : IRateLimiterRepository
    {
        /// <summary>
        /// In-memory, per-key rate limiter.
        /// </summary>
        /// <remarks>
        /// - Uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> to store a lightweight counter per key.
        /// - Each counter contains a <see cref="Counter.WindowStart"/> timestamp and a running <see cref="Counter.Count"/>.
        /// - The limiter enforces a fixed time <c>_window</c> and a maximum request <c>_limit</c> per window.
        /// - Note: this implementation's boolean return value is <c>true</c> when the request is allowed (not rate limited),
        ///   and <c>false</c> when the rate limit has been exceeded. The <paramref name="retryAfterSeconds"/> out
        ///   parameter indicates how many seconds the caller should wait before retrying when rate limited.
        /// </remarks>
        private readonly ConcurrentDictionary<string, Counter> _counters = new();
        private readonly int _limit;
        private readonly TimeSpan _window;

        /// <summary>
        /// Creates a new instance of <see cref="InMemoryRateLimiter"/>.
        /// </summary>
        /// <param name="limit">Maximum allowed requests within the configured time window.</param>
        /// <param name="window">The time window used to count requests.</param>
        public InMemoryRateLimiter(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
        }

        /// <summary>
        /// Checks whether the provided <paramref name="key"/> is rate limited.
        /// </summary>
        /// <param name="key">Unique key to identify the rate-limited subject (for example, user id or IP).</param>
        /// <param name="retryAfterSeconds">When rate limited, outputs the number of seconds the caller should wait before retrying; otherwise 0.</param>
        /// <returns>
        /// <c>true</c> if the request is allowed (not rate limited); <c>false</c> if the rate limit has been exceeded.
        /// </returns>
        public Task<bool> IsRateLimitedAsync(string key, out int retryAfterSeconds)
        {
            var now = DateTimeOffset.UtcNow;

            var counter = _counters.GetOrAdd(key, _ => new Counter { WindowStart = now, Count = 0 });

            lock (counter)
            {
                if (now - counter.WindowStart >= _window)
                {
                    counter.WindowStart = now;
                    counter.Count = 0;
                }

                counter.Count++;

                if (counter.Count <= _limit)
                {
                    retryAfterSeconds = 0;
                    return Task.FromResult(true);
                }

                var remaining = _window - (now - counter.WindowStart);
                retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
                return Task.FromResult(false);
            }
        }
    }
}
