namespace KmsDev.Robokassa
{
    /// <summary>
    /// Собирает клиент для конкретного магазина. Один процесс может держать несколько магазинов.
    /// </summary>
    public interface IRobokassaClientBuilder
    {
        IRobokassaClient Build(RobokassaMerchantCredentials credentials, CancellationToken cancellationToken = default);
    }
}
