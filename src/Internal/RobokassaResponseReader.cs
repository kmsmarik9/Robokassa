using KmsDev.Robokassa.Exceptions;
using System.Text;
using System.Text.Json;

namespace KmsDev.Robokassa
{
    internal static class RobokassaResponseReader
    {
        public static async Task<string> ReadBodyAsync(HttpResponseMessage response, string operation, int maxResponseBytes, CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentLength > maxResponseBytes)
            {
                throw new RobokassaException(RobokassaErrorKind.InvalidResponse, operation);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            var chunk = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + count > maxResponseBytes)
                {
                    throw new RobokassaException(RobokassaErrorKind.InvalidResponse, operation);
                }

                buffer.Write(chunk, 0, count);
            }

            return Encoding.UTF8.GetString(buffer.ToArray());
        }

        public static TResponse Parse<TResponse>(IRobokassaResponseParser parser, string body, string operation, string? successField)
            where TResponse : IRobokassaResponse
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException();
                }

                if (successField is not null)
                {
                    if (!doc.RootElement.TryGetProperty(successField, out var success)
                        || success.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    {
                        throw new JsonException();
                    }

                    if (!success.GetBoolean())
                    {
                        throw new RobokassaException(RobokassaErrorKind.Api, operation);
                    }
                }
            }
            catch (RobokassaException)
            {
                throw;
            }
            catch (JsonException)
            {
                throw new RobokassaException(RobokassaErrorKind.InvalidResponse, operation);
            }

            try
            {
                return parser.Parse<TResponse>(body);
            }
            catch (JsonException)
            {
                throw new RobokassaException(RobokassaErrorKind.InvalidResponse, operation);
            }
        }
    }

    internal interface IRobokassaResponseParser
    {
        TResponse Parse<TResponse>(string body)
            where TResponse : IRobokassaResponse;
    }

    internal sealed class RobokassaJsonResponseParser : IRobokassaResponseParser
    {
        public TResponse Parse<TResponse>(string body)
            where TResponse : IRobokassaResponse
        {
            return JsonSerializer.Deserialize<TResponse>(body, RobokassaWire.Json) ?? throw new JsonException();
        }
    }
}
