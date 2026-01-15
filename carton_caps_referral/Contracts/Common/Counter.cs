namespace carton_caps_referral.Contracts.Common
{
    public sealed class Counter
    {
        public DateTimeOffset WindowStart { get; set; }
        public int Count { get; set; } 
    }
}
