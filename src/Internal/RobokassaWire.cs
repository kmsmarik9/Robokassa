using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace KmsDev.Robokassa
{
    internal static partial class RobokassaWire
    {
        internal static readonly JsonSerializerOptions Json = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = false
        };

        [GeneratedRegex(@"^[0-9]+(?:\.[0-9]+)?$", RegexOptions.CultureInvariant)]
        private static partial Regex AmountPattern();

        internal static decimal Amount(string value)
        {
            if (value is null
                || !AmountPattern().IsMatch(value)
                || !decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount)
                || amount <= 0)
            {
                throw new ArgumentException("сумма должна быть положительным числом с точкой");
            }

            return amount;
        }

        internal static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);

        internal static void Positive(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        internal static JsonObject Payload(object request, string login)
        {
            var payload = JsonSerializer.SerializeToNode(request, request.GetType(), Json)!.AsObject();
            payload["MerchantLogin"] = login;
            return payload;
        }

        internal static IEnumerable<string> Shp(IReadOnlyDictionary<string, string>? fields)
        {
            if (fields is null)
            {
                return [];
            }

            foreach (var field in fields)
            {
                if (!field.Key.StartsWith("Shp_", StringComparison.OrdinalIgnoreCase)
                    || field.Value is null
                    || field.Key.Contains('=')
                    || field.Key.Contains(':'))
                {
                    throw new ArgumentException("пользовательские параметры должны иметь префикс Shp_ и строковое значение");
                }
            }

            return fields.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Key + "=" + p.Value);
        }
    }
}
