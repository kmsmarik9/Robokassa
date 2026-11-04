using Microsoft.Extensions.DependencyInjection;

namespace KmsDev.Robokassa
{
    public sealed class RobokassaSystemConfigurer
    {
        public static readonly Uri DefaultServicesBaseAddress = new("https://services.robokassa.ru/");

        public static readonly Uri DefaultMerchantBaseAddress = new("https://auth.robokassa.ru/");

        public IServiceCollection Services { get; }

        /// <summary>
        /// Базовый адрес Invoice API и остальных сервисных методов.
        /// </summary>
        public Uri ServicesBaseAddress { get; private set; } = DefaultServicesBaseAddress;

        /// <summary>
        /// Базовый адрес классического платёжного хоста. В этой версии HTTP-вызовы на него не идут.
        /// </summary>
        public Uri MerchantBaseAddress { get; private set; } = DefaultMerchantBaseAddress;

        /// <summary>
        /// Только для тестов. Игнорировать ошибки сертификата сервера.
        /// </summary>
        public bool UseDangerousAcceptAnyServerCertificateValidator { get; set; }

        /// <summary>
        /// Дополнительная настройка именованных HttpClient. Если задана, заменяет обработчик по умолчанию.
        /// </summary>
        public Action<IHttpClientBuilder>? ConfigureHttpClient { get; set; }

        private RobokassaMerchantCredentials? _singletonCredentials;

        public RobokassaSystemConfigurer(IServiceCollection services)
        {
            Services = services;
        }

        public RobokassaSystemConfigurer SetServicesBaseAddress(string baseAddress)
        {
            ServicesBaseAddress = NormalizeBaseAddress(baseAddress, nameof(baseAddress));
            return this;
        }

        public RobokassaSystemConfigurer SetMerchantBaseAddress(string baseAddress)
        {
            MerchantBaseAddress = NormalizeBaseAddress(baseAddress, nameof(baseAddress));
            return this;
        }

        /// <summary>
        /// Регистрирует один магазин как <see cref="IRobokassaClient"/>.
        /// </summary>
        public RobokassaSystemConfigurer AddSingletonClient(RobokassaMerchantCredentials credentials)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            credentials.Validate();
            _singletonCredentials = credentials;
            return this;
        }

        internal void ConfigureServices()
        {
            if (_singletonCredentials is { } credentials)
            {
                Services.AddSingleton<IRobokassaClient>(sp =>
                {
                    var clientBuilder = sp.GetRequiredService<IRobokassaClientBuilder>();
                    return clientBuilder.Build(credentials);
                });
            }
        }

        internal static Uri NormalizeBaseAddress(string baseAddress, string paramName)
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
            {
                throw new ArgumentException("empty", paramName);
            }

            var normalized = baseAddress.Trim();
            if (!normalized.EndsWith('/'))
            {
                normalized += "/";
            }

            if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
                || uri.Scheme is not ("https" or "http")
                || !string.IsNullOrEmpty(uri.UserInfo)
                || !string.IsNullOrEmpty(uri.Query)
                || !string.IsNullOrEmpty(uri.Fragment)
                || !uri.AbsolutePath.EndsWith('/'))
            {
                throw new ArgumentException("базовый адрес должен быть абсолютным http(s) url без query и с / в конце", paramName);
            }

            return uri;
        }
    }
}
