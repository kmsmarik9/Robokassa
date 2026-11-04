using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Reflection;

namespace KmsDev.Robokassa.Requests
{
    public sealed class RobokassaRequestsConfigurer
    {
        public IServiceCollection Services { get; }

        public RobokassaRequestsConfigurer(IServiceCollection services)
        {
            Services = services;
        }

        internal void ConfigureServices(
            Uri servicesBaseAddress,
            Uri merchantBaseAddress,
            bool useDangerousAcceptAnyServerCertificateValidator,
            Action<IHttpClientBuilder>? configureHttpClient)
        {
            foreach (var requestType in GetLoadableTypes().Where(IsConcreteRequest))
            {
                var requestName = typeof(RobokassaRequestNameCache<>)
                    .MakeGenericType(requestType)
                    .GetField(nameof(RobokassaRequestNameCache<IRobokassaRequest>.Value), BindingFlags.Public | BindingFlags.Static)!
                    .GetValue(null)!
                    .ToString()!;

                var host = requestType.GetCustomAttribute<RobokassaHostAttribute>(inherit: false)?.Host
                    ?? throw new InvalidOperationException($"{requestType.FullName} не помечен {nameof(RobokassaHostAttribute)}");
                    
                var baseAddress = host switch
                {
                    RobokassaHost.Services => servicesBaseAddress,
                    RobokassaHost.Merchant => merchantBaseAddress,
                    _ => throw new InvalidOperationException($"{requestType.FullName} указывает неизвестный хост {host}")
                };

                var httpClientBuilder = Services
                    .AddHttpClient(requestName, client =>
                    {
                        client.BaseAddress = baseAddress;
                        client.Timeout = Timeout.InfiniteTimeSpan;
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(RobokassaClientInternal.UserAgentProduct + "/" + RobokassaClientInternal.UserAgentVersion);
                    })
                    .SetHandlerLifetime(TimeSpan.FromHours(1));

                if (configureHttpClient is not null)
                {
                    configureHttpClient(httpClientBuilder);
                }
                else
                {
                    httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        var handler = new HttpClientHandler
                        {
                            AllowAutoRedirect = false
                        };
                        if (useDangerousAcceptAnyServerCertificateValidator)
                        {
                            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        }

                        return handler;
                    });
                }

                httpClientBuilder.AddResilienceHandler($"robokassa_resilience_handler:{requestName}", pipeline =>
                {
                    pipeline.AddTimeout(new HttpTimeoutStrategyOptions
                    {
                        Timeout = TimeSpan.FromSeconds(15),
                        TimeoutGenerator = static args =>
                        {
                            if (RobokassaHttpRequestOptionsKeyForResilienceSettingsCache<RobokassaRequestResilienceDefaultSettings>.TryGetFromContext(args.Context, out var settings))
                            {
                                return ValueTask.FromResult(settings.Timeout);
                            }

                            return ValueTask.FromResult(TimeSpan.FromSeconds(15));
                        }
                    });
                });
            }

            foreach (var responseType in GetLoadableTypes().Where(IsJsonResponse))
            {
                var responseName = typeof(RobokassaResponseNameCache<>)
                    .MakeGenericType(responseType)
                    .GetField(nameof(RobokassaResponseNameCache<IRobokassaResponse>.Value), BindingFlags.Public | BindingFlags.Static)!
                    .GetValue(null)!
                    .ToString()!;

                Services.AddKeyedSingleton(typeof(IRobokassaResponseParser), responseName, typeof(RobokassaJsonResponseParser));
            }
        }

        private static bool IsConcreteRequest(Type type) =>
            type is { IsClass: true, IsAbstract: false, IsGenericType: false }
            && type.IsAssignableTo(typeof(IRobokassaRequest));

        private static bool IsJsonResponse(Type type) =>
            type is { IsClass: true, IsAbstract: false, IsGenericType: false }
            && type.IsAssignableTo(typeof(IRobokassaJsonResponse));

        private static IEnumerable<Type> GetLoadableTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types.Where(type => type is not null).Cast<Type>().ToArray();
                }

                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }
    }
}
