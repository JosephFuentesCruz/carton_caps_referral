using System.Text.Json.Serialization;

namespace carton_caps_referral.Contracts.Common
{
    public sealed record PagedResponse<T>
    {
        [JsonPropertyName("items")]
        public required IEnumerable<T> Items { get; init; }
        [JsonPropertyName("nextCursor")]
        public string? NextCursor { get; init; }
    }
}
