using KmsDev.Robokassa.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace KmsDev.Robokassa
{
    public static partial class RobokassaExtensions
    {
        /// <summary>
        /// Регистрирует builder и именованные HttpClient для запросов Robokassa.
        /// Повтор денежных вызовов выключен: pipeline содержит только timeout.
        /// </summary>
        public static IServiceCollection AddRobokassaSystem(this IServiceCollection serviceCollection, Action<RobokassaSystemConfigurer>? systemConfigurer = null)
        {
            serviceCollection.AddSingleton<IRobokassaClientBuilder, RobokassaClientBuilderInternal>();

            var systemConf = new RobokassaSystemConfigurer(serviceCollection);
            systemConfigurer?.Invoke(systemConf);
            systemConf.ConfigureServices();

            new RobokassaRequestsConfigurer(serviceCollection)
                .ConfigureServices
                (
                    systemConf.ServicesBaseAddress,
                    systemConf.MerchantBaseAddress,
                    systemConf.UseDangerousAcceptAnyServerCertificateValidator,
                    systemConf.ConfigureHttpClient
                );

            return serviceCollection;
        }
    }
}
