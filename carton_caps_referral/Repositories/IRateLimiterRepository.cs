namespace carton_caps_referral.Repositories
{
    public interface IRateLimiterRepository
    {
        /// <summary>
        /// Checks if the given key has exceeded the allowed number of requests within the specified time window.
        /// </summary>
        /// <param name="key">The unique key to identify the rate limit (e.g., user ID, IP address).</param>
        /// <param name="retryAfterSeconds">Outputs the number of seconds to wait before making another request if rate limited.</param>
        /// <returns>True if the rate limit has been exceeded; otherwise, false.</returns>
        Task<bool> IsRateLimitedAsync(string key, out int retryAfterSeconds);
    }
}
