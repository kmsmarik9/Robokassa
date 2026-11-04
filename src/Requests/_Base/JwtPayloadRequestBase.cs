using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KmsDev.Robokassa.Requests
{
    public abstract class JwtPayloadRequestBase<TResponse> : RequestBase<TResponse>
        where TResponse : IRobokassaResponse
    {
        public override HttpContent? GetRequestHttpContent(RobokassaMerchantCredentials credentials)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            var payload = GetJwtPayload(credentials);
            var password = ResolvePassword(credentials, GetPasswordKind());
            var jwt = RobokassaSignature.Jwt(payload, credentials.MerchantLogin + ":" + password, credentials.HashAlgorithm);
            return new StringContent(JsonSerializer.Serialize(jwt), Encoding.UTF8, "application/json");
        }

        protected abstract RobokassaPasswordKind GetPasswordKind();

        protected virtual JsonNode GetJwtPayload(RobokassaMerchantCredentials credentials)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            return RobokassaWire.Payload(this, credentials.MerchantLogin);
        }

        private static string ResolvePassword(RobokassaMerchantCredentials credentials, RobokassaPasswordKind kind) => kind switch
        {
            RobokassaPasswordKind.Payment => credentials.PaymentPassword,
            RobokassaPasswordKind.Management => RobokassaMerchantCredentials.Require(credentials.Password1, nameof(RobokassaMerchantCredentials.Password1)),
            RobokassaPasswordKind.Notification => credentials.NotificationPassword,
            RobokassaPasswordKind.Refund => RobokassaMerchantCredentials.Require(credentials.Password3, nameof(RobokassaMerchantCredentials.Password3)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }
}
