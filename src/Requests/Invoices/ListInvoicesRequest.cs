using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Requests
{
    [RobokassaHost(RobokassaHost.Services)]
    public partial class ListInvoicesRequest : JwtPayloadRequestBase<InvoiceInformationListResponse>
    {
        private static readonly RobokassaRequestHttpSettings Settings = new()
        {
            Method = HttpMethod.Post,
            Url = "InvoiceServiceWebApi/api/GetInvoiceInformationList"
        };

        [JsonPropertyName("CurrentPage")]
        public required int CurrentPage { get; init; }

        [JsonPropertyName("PageSize")]
        public required int PageSize { get; init; }

        [JsonPropertyName("InvoiceStatuses")]
        public required IReadOnlyList<string> InvoiceStatuses { get; init; }

        [JsonPropertyName("Keywords")]
        public string? Keywords { get; init; }

        [JsonPropertyName("DateFrom")]
        public required DateTimeOffset DateFrom { get; init; }

        [JsonPropertyName("DateTo")]
        public required DateTimeOffset DateTo { get; init; }

        [JsonPropertyName("IsAscending")]
        public bool? IsAscending { get; init; }

        [JsonPropertyName("InvoiceTypes")]
        public required IReadOnlyList<string> InvoiceTypes { get; init; }

        [JsonPropertyName("Aliases")]
        public IReadOnlyList<string>? Aliases { get; init; }

        [JsonPropertyName("PaymentAliases")]
        public IReadOnlyList<string>? PaymentAliases { get; init; }

        [JsonPropertyName("SumFrom")]
        public decimal? SumFrom { get; init; }

        [JsonPropertyName("SumTo")]
        public decimal? SumTo { get; init; }

        public override RobokassaRequestHttpSettings GetRequestHttpSettings() => Settings;

        public override string GetOperation() => "invoices.GetInvoiceInformationList";

        protected override RobokassaPasswordKind GetPasswordKind() => RobokassaPasswordKind.Management;

        protected override JsonNode GetJwtPayload(RobokassaMerchantCredentials credentials)
        {
            RobokassaInvoiceRules.ValidateList(this);
            return base.GetJwtPayload(credentials);
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
        public static Task<InvoiceInformationListResponse> ListAsync(
            this RobokassaClientApiContainer.InvoicesSection invoicesSection,
            ListInvoicesRequest request,
            Action<ListInvoicesRequest.ResilienceDefaultSettings>? resilienceSettingsAction = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var settings = RobokassaRequestResilience.Prepare(invoicesSection, resilienceSettingsAction);
            return invoicesSection.RobokassaClient.SendRequestAsync(request, settings, cancellationToken);
        }
    }
}
