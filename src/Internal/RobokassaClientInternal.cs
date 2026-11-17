using KmsDev.Robokassa.Exceptions;
using KmsDev.Robokassa.Requests;
using KmsDev.Robokassa.Responses;
using Microsoft.Extensions.DependencyInjection;
using Polly.Timeout;
using System.Net.Http.Headers;

namespace KmsDev.Robokassa
{
    internal sealed class RobokassaClientInternal : IRobokassaClient
    {
        public const string UserAgentProduct = "KmsDev.Robokassa";
        public const string UserAgentVersion = "1.0.0";

        private readonly IServiceProvider _serviceProvider;

        public RobokassaClientInternal(IServiceProvider serviceProvider, RobokassaMerchantCredentials credentials, CancellationToken cancellationToken = default)
        {
            _serviceProvider = serviceProvider;
            Credentials = credentials;
            SelfCancellationToken = cancellationToken;
            Api = new(this);
        }

        public RobokassaMerchantCredentials Credentials { get; }

        public CancellationToken SelfCancellationToken { get; }

        public RobokassaClientApiContainer Api { get; }

        public async Task<TResponse> SendRequestAsync<TResponse, TResilienceSettings>(
            IRobokassaRequest<TResponse> request,
            TResilienceSettings resilienceSettings,
            CancellationToken cancellationToken = default)
            where TResponse : IRobokassaResponse
            where TResilienceSettings : RobokassaRequestResilienceDefaultSettings
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(resilienceSettings);
            if (resilienceSettings.Timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(resilienceSettings));
            }

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(SelfCancellationToken, cancellationToken);
            var httpSettings = request.GetRequestHttpSettings();
            using var httpContent = request.GetRequestHttpContent(Credentials);

            var requestName = request.GetType().Name;

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(linked.Token);
            timeoutCts.CancelAfter(resilienceSettings.Timeout);

            using var httpRequestMessage = new HttpRequestMessage(httpSettings.Method, httpSettings.Url)
            {
                Content = httpContent
            };
            httpRequestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue(UserAgentProduct, UserAgentVersion));
            RobokassaHttpRequestOptionsKeyForResilienceSettingsCache<TResilienceSettings>.SetSettings(httpRequestMessage.Options, resilienceSettings);

            try
            {
                using var httpResponse = await _serviceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(request.GetRequestName())
                    .SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    string? detail = null;
                    try
                    {
                        var errorBody = await RobokassaResponseReader.ReadBodyAsync(httpResponse, requestName, Credentials.MaxResponseBytes, timeoutCts.Token);
                        detail = RobokassaResponseReader.ErrorDetail(errorBody);
                    }
                    catch (RobokassaException)
                    {
                    }

                    throw new RobokassaException(RobokassaErrorKind.Http, requestName, httpResponse.StatusCode, detail: detail);
                }

                var body = await RobokassaResponseReader.ReadBodyAsync(httpResponse, requestName, Credentials.MaxResponseBytes, timeoutCts.Token);
                var parser = _serviceProvider.GetRequiredKeyedService<IRobokassaResponseParser>(RobokassaResponseNameCache<TResponse>.Value);
                return RobokassaResponseReader.Parse<TResponse>(parser, body, requestName, request.GetSuccessField());
            }
            catch (RobokassaException)
            {
                throw;
            }
            catch (TimeoutRejectedException ex)
            {
                throw new RobokassaException(RobokassaErrorKind.Timeout, requestName, detail: ex.Message, innerException: ex);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && !SelfCancellationToken.IsCancellationRequested)
            {
                throw new RobokassaException(RobokassaErrorKind.Timeout, requestName, detail: ex.Message, innerException: ex);
            }
            catch (HttpRequestException ex)
            {
                throw new RobokassaException(RobokassaErrorKind.Transport, requestName, detail: ex.Message, innerException: ex);
            }
            catch (IOException ex)
            {
                throw new RobokassaException(RobokassaErrorKind.Transport, requestName, detail: ex.Message, innerException: ex);
            }
        }
    }
}
