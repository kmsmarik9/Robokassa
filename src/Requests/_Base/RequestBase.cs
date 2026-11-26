namespace KmsDev.Robokassa.Requests
{
    public abstract class RequestBase<TResponse> : IRobokassaRequest<TResponse>
        where TResponse : IRobokassaResponse
    {
        public virtual string GetRequestName()
        {
            throw new NotImplementedException();
        }

        public abstract RobokassaRequestHttpSettings GetRequestHttpSettings();

        public virtual string? GetSuccessField() => "isSuccess";

        public abstract HttpContent? GetRequestHttpContent(RobokassaMerchantCredentials credentials);
    }
}
