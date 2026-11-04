using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Requests
{
    [RobokassaHost(RobokassaHost.Services)]
    public partial class DeactivateInvoiceRequest : JwtPayloadRequestBase<InvoiceActionResponse>
    {
        private static readonly RobokassaRequestHttpSettings Settings = new()
        {
            Method = HttpMethod.Post,
            Url = "InvoiceServiceWebApi/api/DeactivateInvoice"
        };

        [JsonPropertyName("EncodedId")]
        public string? EncodedId { get; init; }

        [JsonPropertyName("Id")]
        public string? Id { get; init; }

        [JsonPropertyName("InvId")]
        public long? InvId { get; init; }

        public override RobokassaRequestHttpSettings GetRequestHttpSettings() => Settings;

        public override string GetOperation() => "invoices.DeactivateInvoice";

        protected override RobokassaPasswordKind GetPasswordKind() => RobokassaPasswordKind.Management;

        protected override JsonNode GetJwtPayload(RobokassaMerchantCredentials credentials)
        {
            RobokassaInvoiceRules.ValidateIdentifier(Id, InvId, EncodedId);
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
        public static Task<InvoiceActionResponse> DeactivateAsync(
            this RobokassaClientApiContainer.InvoicesSection invoicesSection,
            DeactivateInvoiceRequest request,
            Action<DeactivateInvoiceRequest.ResilienceDefaultSettings>? resilienceSettingsAction = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var settings = RobokassaRequestResilience.Prepare(invoicesSection, resilienceSettingsAction);
            return invoicesSection.RobokassaClient.SendRequestAsync(request, settings, cancellationToken);
        }
    }
}
