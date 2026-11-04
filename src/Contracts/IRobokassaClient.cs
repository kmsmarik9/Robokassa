using KmsDev.Robokassa.Requests;

namespace KmsDev.Robokassa
{
    /// <summary>
    /// Клиент одного магазина Robokassa.
    /// </summary>
    public interface IRobokassaClient
    {
        internal RobokassaMerchantCredentials Credentials { get; }

        CancellationToken SelfCancellationToken { get; }

        RobokassaClientApiContainer Api { get; }

        Task<TResponse> SendRequestAsync<TResponse, TResilienceSettings>(
            IRobokassaRequest<TResponse> request,
            TResilienceSettings resilienceSettings,
            CancellationToken cancellationToken = default)
            where TResponse : IRobokassaResponse
            where TResilienceSettings : RobokassaRequestResilienceDefaultSettings;
    }
}
