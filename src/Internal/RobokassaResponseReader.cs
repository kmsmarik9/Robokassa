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
                    throw new RobokassaException(RobokassaErrorKind.InvalidResponse, operation);
                }

                if (successField is not null)
                {
                    if (!doc.RootElement.TryGetProperty(successField, out var success)
                        || success.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    {
                        throw new RobokassaException(RobokassaErrorKind.InvalidResponse, operation);
                    }

                    if (!success.GetBoolean())
                    {
                        throw new RobokassaException(RobokassaErrorKind.Api, operation, detail: ReadMessage(doc.RootElement));
                    }
                }
            }
            catch (RobokassaException)
            {
                throw;
            }
            catch (JsonException exception)
            {
                throw InvalidJson(operation, exception);
            }

            try
            {
                return parser.Parse<TResponse>(body);
            }
            catch (JsonException exception)
            {
                throw InvalidJson(operation, exception);
            }
        }

        private static readonly string BareJsonExceptionMessage = new JsonException().Message;

        private static RobokassaException InvalidJson(string operation, JsonException exception)
        {
            var detail = exception.Message == BareJsonExceptionMessage ? null : exception.Message;
            return new RobokassaException(RobokassaErrorKind.InvalidResponse, operation, detail: detail, innerException: exception);
        }

        public static string? ErrorDetail(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            var text = body.Trim();
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var message = ReadMessage(doc.RootElement);
                    if (message is not null)
                    {
                        return message;
                    }
                }
            }
            catch (JsonException)
            {
            }

            const int limit = 512;
            return text.Length <= limit ? text : text[..limit];
        }

        private static string? ReadMessage(JsonElement root)
        {
            if (!root.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var text = message.GetString();
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
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
