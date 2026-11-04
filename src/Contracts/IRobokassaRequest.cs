using KmsDev.Robokassa.Requests;

namespace KmsDev.Robokassa
{
    [GetRequestNameForInherit]
    public interface IRobokassaRequest
    {
        RobokassaRequestHttpSettings GetRequestHttpSettings();

        HttpContent? GetRequestHttpContent(RobokassaMerchantCredentials credentials);

        string GetRequestName();

        string GetOperation();

        string? GetSuccessField();
    }

    public interface IRobokassaRequest<TResponse> : IRobokassaRequest
        where TResponse : IRobokassaResponse
    {
    }
}
