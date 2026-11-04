using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Requests
{
    [RobokassaHost(RobokassaHost.Services)]
    public partial class CreateInvoiceRequest : JwtPayloadRequestBase<CreateInvoiceResponse>
    {
        private static readonly RobokassaRequestHttpSettings Settings = new()
        {
            Method = HttpMethod.Post,
            Url = "InvoiceServiceWebApi/api/CreateInvoice"
        };

        [JsonPropertyName("InvoiceType")]
        public string InvoiceType { get; init; } = "OneTime";

        [JsonPropertyName("Culture")]
        public string Culture { get; init; } = "ru";

        [JsonPropertyName("InvId")]
        public long? InvId { get; init; }

        [JsonPropertyName("OutSum")]
        public decimal? OutSum { get; init; }

        [JsonPropertyName("ExpirationDate")]
        public DateTimeOffset? ExpirationDate { get; init; }

        [JsonPropertyName("Description")]
        public string? Description { get; init; }

        [JsonPropertyName("FiscalParentOpId")]
        public long? FiscalParentOpId { get; init; }

        [JsonPropertyName("MerchantComments")]
        public string? MerchantComments { get; init; }

        [JsonPropertyName("UserFields")]
        public IReadOnlyDictionary<string, string>? UserFields { get; init; }

        [JsonPropertyName("InvoiceItems")]
        public IReadOnlyList<InvoiceItem>? InvoiceItems { get; init; }

        [JsonPropertyName("SuccessUrl2Data")]
        public RedirectData? SuccessUrl2Data { get; init; }

        [JsonPropertyName("FailUrl2Data")]
        public RedirectData? FailUrl2Data { get; init; }

        [JsonPropertyName("Aliases")]
        public IReadOnlyList<string>? Aliases { get; init; }

        [JsonPropertyName("Payments")]
        public IReadOnlyList<InvoicePayment>? Payments { get; init; }

        [JsonPropertyName("CustomUserProperty")]
        public InvoiceCustomUserProperty? CustomUserProperty { get; init; }

        [JsonPropertyName("IsWithoutFreeSale")]
        public bool? IsWithoutFreeSale { get; init; }

        [JsonPropertyName("Sno")]
        public string? Sno { get; init; }

        [JsonPropertyName("AdditionalParameters")]
        public InvoiceParameters? AdditionalParameters { get; init; }

        public override RobokassaRequestHttpSettings GetRequestHttpSettings() => Settings;

        public override string GetOperation() => "invoices.CreateInvoice";

        protected override RobokassaPasswordKind GetPasswordKind() => RobokassaPasswordKind.Payment;

        protected override JsonNode GetJwtPayload(RobokassaMerchantCredentials credentials)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            RobokassaInvoiceRules.ValidateCreate(this, credentials.IsTest);
            var payload = (JsonObject)base.GetJwtPayload(credentials);
            if (credentials.IsTest)
            {
                var additional = payload["AdditionalParameters"] as JsonObject ?? new JsonObject();
                additional["IsTest"] = "1";
                payload["AdditionalParameters"] = additional;
            }

            return payload;
        }

        public class ResilienceDefaultSettings : RobokassaRequestResilienceDefaultSettings
        {
        }
    }
}

namespace KmsDev.Robokassa
{
    public static partial class RobokassaExtensions
    {
        public static async Task<CreateInvoiceResponse> CreateAsync(
            this RobokassaClientApiContainer.InvoicesSection invoicesSection,
            CreateInvoiceRequest request,
            Action<CreateInvoiceRequest.ResilienceDefaultSettings>? resilienceSettingsAction = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var settings = RobokassaRequestResilience.Prepare(invoicesSection, resilienceSettingsAction);
            var response = await invoicesSection.RobokassaClient.SendRequestAsync(request, settings, cancellationToken);
            if (string.IsNullOrWhiteSpace(response.Id)
                || response.InvId <= 0
                || !Uri.TryCreate(response.Url, UriKind.Absolute, out var url)
                || url.Scheme is not ("http" or "https"))
            {
                throw new RobokassaException(RobokassaErrorKind.InvalidResponse, request.GetOperation());
            }

            return response;
        }

        public static async Task<string> CreatePaymentUrlAsync(
            this RobokassaClientApiContainer.InvoicesSection invoicesSection,
            CreateInvoiceRequest request,
            Action<CreateInvoiceRequest.ResilienceDefaultSettings>? resilienceSettingsAction = default,
            CancellationToken cancellationToken = default)
        {
            var response = await invoicesSection.CreateAsync(request, resilienceSettingsAction, cancellationToken);
            return response.Url;
        }
    }
}
