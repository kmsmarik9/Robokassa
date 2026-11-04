using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Requests
{
    [RobokassaHost(RobokassaHost.Services)]
    public partial class GetInvoiceRequest : JwtPayloadRequestBase<GetInvoiceResponse>
    {
        private static readonly RobokassaRequestHttpSettings Settings = new()
        {
            Method = HttpMethod.Post,
            Url = "InvoiceServiceWebApi/api/GetInvoiceInformation"
        };

        [JsonPropertyName("Id")]
        public string? Id { get; init; }

        [JsonPropertyName("InvId")]
        public long? InvId { get; init; }

        public override RobokassaRequestHttpSettings GetRequestHttpSettings() => Settings;

        public override string GetOperation() => "invoices.GetInvoiceInformation";

        protected override RobokassaPasswordKind GetPasswordKind() => RobokassaPasswordKind.Management;

        protected override JsonNode GetJwtPayload(RobokassaMerchantCredentials credentials)
        {
            RobokassaInvoiceRules.ValidateIdentifier(Id, InvId, null);
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
        public static Task<GetInvoiceResponse> GetAsync(
            this RobokassaClientApiContainer.InvoicesSection invoicesSection,
            GetInvoiceRequest request,
            Action<GetInvoiceRequest.ResilienceDefaultSettings>? resilienceSettingsAction = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var settings = RobokassaRequestResilience.Prepare(invoicesSection, resilienceSettingsAction);
            return invoicesSection.RobokassaClient.SendRequestAsync(request, settings, cancellationToken);
        }
    }
}
