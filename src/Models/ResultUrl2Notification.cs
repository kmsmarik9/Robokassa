using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Models
{
    public sealed class ResultUrl2Notification
    {
        [JsonPropertyName("header")]
        public required ResultUrl2NotificationHeader Header { get; init; }

        [JsonPropertyName("data")]
        public required ResultUrl2NotificationData Data { get; init; }
    }

    public sealed class ResultUrl2NotificationHeader
    {
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("version")]
        public required string Version { get; init; }

        [JsonPropertyName("timestamp")]
        public required string Timestamp { get; init; }
    }

    public sealed class ResultUrl2NotificationData
    {
        [JsonPropertyName("shop")]
        public required string Shop { get; init; }

        [JsonPropertyName("opKey")]
        public required string OpKey { get; init; }

        [JsonPropertyName("invId")]
        public required string InvId { get; init; }

        [JsonPropertyName("paymentMethod")]
        public required string PaymentMethod { get; init; }

        [JsonPropertyName("incSum")]
        public required string IncSum { get; init; }

        [JsonPropertyName("state")]
        public required string State { get; init; }
    }
}
