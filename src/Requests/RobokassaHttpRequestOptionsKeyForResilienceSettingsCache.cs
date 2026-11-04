using Polly;
using System.Diagnostics.CodeAnalysis;

namespace KmsDev.Robokassa.Requests
{
    public static class RobokassaHttpRequestOptionsKeyForResilienceSettingsCache<TSettings>
        where TSettings : RobokassaRequestResilienceDefaultSettings
    {
        public static readonly HttpRequestOptionsKey<TSettings> OptionKey = new("RobokassaResilienceSettings");

        public static void SetSettings(HttpRequestOptions requestOptions, TSettings settings)
        {
            requestOptions.Set(OptionKey, settings);
        }

        public static bool TryGetFromContext(ResilienceContext resilienceContext, [MaybeNullWhen(false)] out TSettings settings)
        {
            var options = resilienceContext.GetRequestMessage()?.Options;
            if (options != null && options.TryGetValue(OptionKey, out settings))
            {
                return true;
            }

            settings = default;
            return false;
        }
    }
}
