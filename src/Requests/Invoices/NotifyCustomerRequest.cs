using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace KmsDev.Robokassa.Requests
{
    [RobokassaHost(RobokassaHost.Services)]
    public partial class NotifyCustomerRequest : JwtPayloadRequestBase<InvoiceActionResponse>
    {
        private static readonly RobokassaRequestHttpSettings Settings = new()
        {
            Method = HttpMethod.Post,
            Url = "InvoiceServiceWebApi/api/NotifyCustomer"
        };

        [JsonPropertyName("InvId")]
        public required long InvId { get; init; }

        [JsonPropertyName("CustomerNotificationEmail")]
        public string? CustomerNotificationEmail { get; init; }

        [JsonPropertyName("CustomerNotificationName")]
        public string? CustomerNotificationName { get; init; }

        [JsonPropertyName("CustomerNotificationPhone")]
        public string? CustomerNotificationPhone { get; init; }

        public override RobokassaRequestHttpSettings GetRequestHttpSettings() => Settings;

        public override string GetOperation() => "invoices.NotifyCustomer";

        protected override RobokassaPasswordKind GetPasswordKind() => RobokassaPasswordKind.Management;

        protected override JsonNode GetJwtPayload(RobokassaMerchantCredentials credentials)
        {
            RobokassaWire.Positive(InvId);
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
        public static Task<InvoiceActionResponse> NotifyCustomerAsync(
            this RobokassaClientApiContainer.InvoicesSection invoicesSection,
            NotifyCustomerRequest request,
            Action<NotifyCustomerRequest.ResilienceDefaultSettings>? resilienceSettingsAction = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var settings = RobokassaRequestResilience.Prepare(invoicesSection, resilienceSettingsAction);
            return invoicesSection.RobokassaClient.SendRequestAsync(request, settings, cancellationToken);
        }
    }
}
