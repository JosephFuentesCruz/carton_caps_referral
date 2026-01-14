using System.Text.Json.Serialization;

namespace carton_caps_referral.Contracts.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReferralStatus
    {
        PENDING,
        COMPLETE,
        EXPIRED,
        INVALID
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum  ShareChannel
    {
        SMS,
        EMAIL,
        SHARE_SHEET,
        COPY_LINK
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeepLinkDestinationType
    {
        AUTH_GATE_DEFAULT,
        AUTH_GATE_REFERRAL
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReferralDomainStatus
    {
        Pending,
        Complete,
        Expired,
        Invalid
    }
}
