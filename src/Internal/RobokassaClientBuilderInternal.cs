namespace KmsDev.Robokassa
{
    internal sealed class RobokassaClientBuilderInternal : IRobokassaClientBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public RobokassaClientBuilderInternal(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRobokassaClient Build(RobokassaMerchantCredentials credentials, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            credentials.Validate();
            return new RobokassaClientInternal(_serviceProvider, credentials, cancellationToken);
        }
    }
}
