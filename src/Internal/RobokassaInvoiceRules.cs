using KmsDev.Robokassa.Models;
using KmsDev.Robokassa.Requests;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KmsDev.Robokassa
{
    internal static class RobokassaInvoiceRules
    {
        public static void ValidateCreate(CreateInvoiceRequest request, bool isTest)
        {
            if (request.InvoiceType is not ("OneTime" or "Reusable")
                || request.Culture is not ("ru" or "en")
                || request.InvId < 0
                || request.OutSum <= 0
                || request.OutSum is null && request.InvoiceItems is not { Count: > 0 })
            {
                throw new ArgumentException("некорректные параметры счета");
            }

            ValidateItems(request.InvoiceItems);
            if (request.SuccessUrl2Data is { } success)
            {
                ValidateRedirect(success.Url, success.Method);
            }

            if (request.FailUrl2Data is { } fail)
            {
                ValidateRedirect(fail.Url, fail.Method);
            }

            var parameters = request.AdditionalParameters;
            if (parameters is null)
            {
                return;
            }

            if ((parameters.StepByStep == true ? 1 : 0) + (parameters.Recurring == true ? 1 : 0) + (parameters.Token is not null ? 1 : 0) > 1)
            {
                throw new ArgumentException("StepByStep, Recurring и Token несовместимы");
            }

            if (isTest && parameters.Split is not null)
            {
                throw new ArgumentException("сплитование несовместимо с тестовым режимом");
            }

            if (parameters.Token is not null && string.IsNullOrWhiteSpace(parameters.Token))
            {
                throw new ArgumentException("Token не может быть пустым");
            }

            if (parameters.Split is not null)
            {
                try
                {
                    if (JsonNode.Parse(parameters.Split) is not JsonObject)
                    {
                        throw new JsonException();
                    }
                }
                catch (JsonException)
                {
                    throw new ArgumentException("Split должен содержать json-объект");
                }
            }

            ValidateRedirect(parameters.SuccessUrl2, parameters.SuccessUrl2Method);
            ValidateRedirect(parameters.FailUrl2, parameters.FailUrl2Method);
            if (parameters.Culture is not null and not ("ru" or "en"))
            {
                throw new ArgumentException("неверный язык");
            }
        }

        public static void ValidateItems(IReadOnlyList<InvoiceItem>? items)
        {
            if (items is null)
            {
                return;
            }

            if (items.Count == 0
                || items.Any(i => i is null
                    || string.IsNullOrWhiteSpace(i.Name)
                    || i.Quantity <= 0
                    || i.Cost < 0
                    || string.IsNullOrWhiteSpace(i.Tax)))
            {
                throw new ArgumentException("некорректные позиции счета");
            }
        }

        public static void ValidateRedirect(string? url, string? method)
        {
            if (method is not null and not ("GET" or "POST") || method is not null && url is null)
            {
                throw new ArgumentException("неверный метод переадресации");
            }

            if (url is not null && (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")))
            {
                throw new ArgumentException("неверный url переадресации");
            }
        }

        public static void ValidateIdentifier(string? id, long? invId, string? encodedId)
        {
            if ((id is null ? 0 : 1) + (invId is null ? 0 : 1) + (encodedId is null ? 0 : 1) != 1
                || id is not null && (!Guid.TryParse(id, out var guid) || guid == Guid.Empty)
                || invId <= 0
                || encodedId is not null && string.IsNullOrWhiteSpace(encodedId))
            {
                throw new ArgumentException("нужен ровно один корректный идентификатор счета");
            }
        }

        public static void ValidateList(ListInvoicesRequest request)
        {
            if (request.CurrentPage < 1
                || request.PageSize < 1
                || request.DateFrom > request.DateTo
                || request.InvoiceStatuses is not { Count: > 0 }
                || request.InvoiceTypes is not { Count: > 0 }
                || request.InvoiceStatuses.Any(s => s is not ("NotPaid" or "Paid" or "Expired"))
                || request.InvoiceTypes.Any(s => s is not ("OneTime" or "Reusable"))
                || request.SumFrom < 0
                || request.SumTo < 0
                || request.SumFrom > request.SumTo)
            {
                throw new ArgumentException("некорректный фильтр счетов", nameof(request));
            }
        }
    }
}
