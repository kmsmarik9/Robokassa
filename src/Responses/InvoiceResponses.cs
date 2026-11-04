using System.Text.Json;
using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Responses
{
    public sealed class CreateInvoiceResponse : IRobokassaJsonResponse
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("invId")]
        public required long InvId { get; init; }

        [JsonPropertyName("url")]
        public required string Url { get; init; }

        [JsonPropertyName("isSuccess")]
        public required bool IsSuccess { get; init; }
    }

    public sealed class InvoiceActionResponse : IRobokassaJsonResponse
    {
        [JsonPropertyName("isSuccess")]
        public required bool IsSuccess { get; init; }
    }

    public sealed class GetInvoiceResponse : IRobokassaJsonResponse
    {
        [JsonPropertyName("invoiceInformation")]
        public required InvoiceInformation InvoiceInformation { get; init; }

        [JsonPropertyName("isSuccess")]
        public required bool IsSuccess { get; init; }
    }

    public sealed class InvoiceInformationListResponse : IRobokassaJsonResponse
    {
        [JsonPropertyName("invoiceInformationList")]
        public required IReadOnlyList<InvoiceInformation> InvoiceInformationList { get; init; }

        [JsonPropertyName("total")]
        public required int Total { get; init; }

        [JsonPropertyName("isSuccess")]
        public required bool IsSuccess { get; init; }
    }

    public sealed class InvoiceInformation
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("invId")]
        public required long InvId { get; init; }

        [JsonPropertyName("invoiceType")]
        public required string InvoiceType { get; init; }

        [JsonPropertyName("created")]
        public required DateTimeOffset Created { get; init; }

        [JsonPropertyName("modified")]
        public required DateTimeOffset Modified { get; init; }

        [JsonPropertyName("merchantLogin")]
        public string? MerchantLogin { get; init; }

        [JsonPropertyName("culture")]
        public string? Culture { get; init; }

        [JsonPropertyName("outSum")]
        public required decimal OutSum { get; init; }

        [JsonPropertyName("expirationDate")]
        public DateTimeOffset? ExpirationDate { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("fiscalParentOpId")]
        public int? FiscalParentOpId { get; init; }

        [JsonPropertyName("merchantComments")]
        public string? MerchantComments { get; init; }

        [JsonPropertyName("invoiceItems")]
        public required IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> InvoiceItems { get; init; }

        [JsonPropertyName("customerNotificationEmail")]
        public string? CustomerNotificationEmail { get; init; }

        [JsonPropertyName("isCustomerNotificationEmailSent")]
        public required bool IsCustomerNotificationEmailSent { get; init; }

        [JsonPropertyName("opId")]
        public int? OpId { get; init; }

        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; init; }

        [JsonPropertyName("invoiceStatus")]
        public required string InvoiceStatus { get; init; }

        [JsonPropertyName("invoicePaymentUrl")]
        public string? InvoicePaymentUrl { get; init; }

        [JsonPropertyName("shortInvoicePaymentUrl")]
        public string? ShortInvoicePaymentUrl { get; init; }

        [JsonPropertyName("successUrl2Data")]
        public JsonElement? SuccessUrl2Data { get; init; }

        [JsonPropertyName("failUrl2Data")]
        public JsonElement? FailUrl2Data { get; init; }

        [JsonPropertyName("aliases")]
        public required IReadOnlyList<string> Aliases { get; init; }

        [JsonPropertyName("paymentAlias")]
        public string? PaymentAlias { get; init; }

        [JsonPropertyName("paymentCardNumber")]
        public string? PaymentCardNumber { get; init; }

        [JsonPropertyName("userFields")]
        public required IReadOnlyDictionary<string, string> UserFields { get; init; }

        [JsonPropertyName("payments")]
        public required IReadOnlyList<InvoicePayment> Payments { get; init; }

        [JsonPropertyName("customUserProperty")]
        public JsonElement? CustomUserProperty { get; init; }

        [JsonPropertyName("isWithoutFreeSale")]
        public required bool IsWithoutFreeSale { get; init; }

        [JsonPropertyName("additionalParameters")]
        public IReadOnlyDictionary<string, string>? AdditionalParameters { get; init; }
    }
}
