using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Models
{
    public sealed class InvoiceItem
    {
        [JsonPropertyName("Name")]
        public required string Name { get; init; }

        [JsonPropertyName("Quantity")]
        public required decimal Quantity { get; init; }

        [JsonPropertyName("Cost")]
        public required decimal Cost { get; init; }

        [JsonPropertyName("Tax")]
        public required string Tax { get; init; }

        [JsonPropertyName("PaymentMethod")]
        public string? PaymentMethod { get; init; }

        [JsonPropertyName("PaymentObject")]
        public string? PaymentObject { get; init; }

        [JsonPropertyName("NomenclatureCode")]
        public string? NomenclatureCode { get; init; }

        [JsonPropertyName("AgentInfo")]
        public InvoiceAgentInfo? AgentInfo { get; init; }

        [JsonPropertyName("SupplierInfo")]
        public InvoiceSupplierInfo? SupplierInfo { get; init; }

        [JsonPropertyName("IsNonAgentItem")]
        public bool? IsNonAgentItem { get; init; }
    }

    public sealed class InvoiceAgentInfo
    {
        [JsonPropertyName("Type")]
        public string? Type { get; init; }
    }

    public sealed class InvoiceSupplierInfo
    {
        [JsonPropertyName("Name")]
        public string? Name { get; init; }

        [JsonPropertyName("Inn")]
        public string? Inn { get; init; }

        [JsonPropertyName("Phones")]
        public IReadOnlyList<string>? Phones { get; init; }
    }

    public sealed class InvoicePayment
    {
        [JsonPropertyName("Type")]
        public string? Type { get; init; }

        [JsonPropertyName("Sum")]
        public required decimal Sum { get; init; }
    }

    public sealed class InvoiceCustomUserProperty
    {
        [JsonPropertyName("Name")]
        public string? Name { get; init; }

        [JsonPropertyName("Value")]
        public string? Value { get; init; }
    }

    public sealed class RedirectData
    {
        [JsonPropertyName("Url")]
        public required string Url { get; init; }

        [JsonPropertyName("Method")]
        public required string Method { get; init; }
    }
}
