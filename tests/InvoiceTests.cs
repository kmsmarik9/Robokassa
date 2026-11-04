using System.Globalization;
using System.Text.Json;
using KmsDev.Robokassa.Models;
using KmsDev.Robokassa.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace KmsDev.Robokassa.Tests;

public sealed class InvoiceTests
{
    [Fact]
    public async Task CreateUsesInvoiceJwtAndStringParameters()
    {
        using var client = new TestClient(test: true);
        client.Handler.Respond = async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://services.example/InvoiceServiceWebApi/api/CreateInvoice", request.RequestUri!.AbsoluteUri);
            var payload = await Inspect.Jwt(request, "demo:test1");
            Assert.Equal("demo", payload.GetProperty("MerchantLogin").GetString());
            Assert.Equal(long.MaxValue, payload.GetProperty("InvId").GetInt64());
            Assert.Equal(99.99m, payload.GetProperty("OutSum").GetDecimal());
            Assert.Equal("OneTime", payload.GetProperty("InvoiceType").GetString());
            var additional = payload.GetProperty("AdditionalParameters");
            Assert.All(additional.EnumerateObject(), property => Assert.Equal(JsonValueKind.String, property.Value.ValueKind));
            Assert.Equal("1", additional.GetProperty("IsTest").GetString());
            Assert.Equal("true", additional.GetProperty("StepByStep").GetString());
            Assert.Equal("https://example.com/cb?a=1&b=2", additional.GetProperty("ResultURL2").GetString());
            Assert.Equal("[\"BankCard\",\"SBP\"]", additional.GetProperty("PaymentMethods").GetString());
            Assert.False(payload.TryGetProperty("Password1", out _));
            return StubState.Json(TestClient.InvoiceResponse);
        };

        var response = await client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest
        {
            InvId = long.MaxValue,
            OutSum = 99.99m,
            AdditionalParameters = new InvoiceParameters
            {
                StepByStep = true,
                ResultUrl2 = "https://example.com/cb?a=1&b=2",
                PaymentMethods = ["BankCard", "SBP"]
            }
        });

        Assert.Equal(42, response.InvId);
        Assert.Equal(1, client.Handler.Calls);
    }

    [Theory]
    [InlineData(RobokassaHashAlgorithm.Md5, "MD5")]
    [InlineData(RobokassaHashAlgorithm.Sha1, "HS1")]
    [InlineData(RobokassaHashAlgorithm.Sha256, "HS256")]
    [InlineData(RobokassaHashAlgorithm.Sha384, "HS384")]
    [InlineData(RobokassaHashAlgorithm.Sha512, "HS512")]
    public async Task JwtMatchesIndependentHmac(RobokassaHashAlgorithm algorithm, string header)
    {
        using var client = new TestClient(algorithm: algorithm);
        client.Handler.Respond = async (request, cancellationToken) =>
        {
            await Inspect.Jwt(request, expectedAlg: header);
            return StubState.Json(TestClient.InvoiceResponse);
        };

        await client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m });
    }

    [Fact]
    public async Task ItemOnlyInvoiceAllowsAutomaticNumberAndNoOutSum()
    {
        using var client = new TestClient();
        client.Handler.Respond = async (request, cancellationToken) =>
        {
            var payload = await Inspect.Jwt(request);
            Assert.False(payload.TryGetProperty("InvId", out _));
            Assert.False(payload.TryGetProperty("OutSum", out _));
            Assert.False(payload.TryGetProperty("AdditionalParameters", out _));
            Assert.Equal(1.25m, payload.GetProperty("InvoiceItems")[0].GetProperty("Quantity").GetDecimal());
            return StubState.Json(TestClient.InvoiceResponse);
        };

        await client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest
        {
            InvoiceItems = [new InvoiceItem { Name = "услуга", Quantity = 1.25m, Cost = 20m, Tax = "none" }]
        });
    }

    [Theory]
    [InlineData("StepByStep")]
    [InlineData("Recurring")]
    [InlineData("Token")]
    [InlineData("Split")]
    public async Task SpecialScenariosStayOnInvoiceApi(string scenario)
    {
        using var client = new TestClient();
        var parameters = scenario switch
        {
            "StepByStep" => new InvoiceParameters { StepByStep = true },
            "Recurring" => new InvoiceParameters { Recurring = true },
            "Token" => new InvoiceParameters { Token = "saved-card" },
            _ => new InvoiceParameters { Split = "{\"merchants\":[]}" }
        };
        client.Handler.Respond = async (request, cancellationToken) =>
        {
            Assert.EndsWith("CreateInvoice", request.RequestUri!.AbsolutePath);
            var payload = await Inspect.Jwt(request);
            Assert.True(payload.GetProperty("AdditionalParameters").TryGetProperty(scenario, out _));
            return StubState.Json(TestClient.InvoiceResponse);
        };

        await client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest
        {
            OutSum = 1m,
            AdditionalParameters = parameters
        });
    }

    [Fact]
    public async Task AllManagementMethodsUsePrimaryPasswordAndExactFieldCasing()
    {
        using var client = new TestClient(test: true);
        client.Handler.Respond = async (request, cancellationToken) =>
        {
            var payload = await Inspect.Jwt(request);
            Assert.False(payload.TryGetProperty("AdditionalParameters", out _));
            if (request.RequestUri!.AbsolutePath.EndsWith("GetInvoiceInformationList"))
            {
                Assert.Equal("NotPaid", payload.GetProperty("InvoiceStatuses")[0].GetString());
                Assert.Equal("Reusable", payload.GetProperty("InvoiceTypes")[0].GetString());
                return StubState.Json("""{"isSuccess":true,"invoiceInformationList":[],"total":0}""");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("GetInvoiceInformation"))
            {
                return StubState.Json("""{"isSuccess":true,"invoiceInformation":{"id":"id","invId":42,"invoiceType":"OneTime","created":"2026-01-01T00:00:00Z","modified":"2026-01-01T00:00:00Z","outSum":100,"invoiceItems":[],"isCustomerNotificationEmailSent":false,"invoiceStatus":"NotPaid","aliases":[],"userFields":{},"payments":[],"isWithoutFreeSale":false}}""");
            }

            Assert.Equal(42, payload.GetProperty("InvId").GetInt64());
            return StubState.Json("{\"isSuccess\":true}");
        };

        await client.Client.Api.Invoices.DeactivateAsync(new DeactivateInvoiceRequest { InvId = 42 });
        var invoice = await client.Client.Api.Invoices.GetAsync(new GetInvoiceRequest { InvId = 42 });
        Assert.Equal("NotPaid", invoice.InvoiceInformation.InvoiceStatus);
        await client.Client.Api.Invoices.NotifyCustomerAsync(new NotifyCustomerRequest
        {
            InvId = 42,
            CustomerNotificationEmail = "buyer@example.com"
        });
        await client.Client.Api.Invoices.ListAsync(new ListInvoicesRequest
        {
            CurrentPage = 1,
            PageSize = 10,
            InvoiceStatuses = ["NotPaid"],
            InvoiceTypes = ["Reusable"],
            DateFrom = DateTimeOffset.UtcNow.AddDays(-1),
            DateTo = DateTimeOffset.UtcNow
        });
        Assert.Equal(4, client.Handler.Calls);
    }

    [Fact]
    public async Task InvalidParametersFailBeforeNetwork()
    {
        using var client = new TestClient(test: true);
        await Assert.ThrowsAsync<ArgumentException>(() => client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = -1 }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest()));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest
        {
            OutSum = 1,
            AdditionalParameters = new InvoiceParameters { StepByStep = true, Token = "x" }
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest
        {
            OutSum = 1,
            AdditionalParameters = new InvoiceParameters { Split = "{}" }
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Client.Api.Invoices.DeactivateAsync(new DeactivateInvoiceRequest
        {
            InvId = 42,
            EncodedId = "x"
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Client.Api.Invoices.GetAsync(new GetInvoiceRequest()));
        Assert.Equal(0, client.Handler.Calls);
    }

    [Fact]
    public async Task DecimalSerializationDoesNotDependOnCurrentCulture()
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            using var client = new TestClient();
            client.Handler.Respond = async (request, cancellationToken) =>
            {
                var payload = await Inspect.Jwt(request);
                Assert.Equal("100.25", payload.GetProperty("OutSum").GetRawText());
                return StubState.Json(TestClient.InvoiceResponse);
            };
            await client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 100.25m });
        }
        finally
        {
            CultureInfo.CurrentCulture = old;
        }
    }

    [Fact]
    public async Task CreatePaymentUrlReturnsUrl()
    {
        using var client = new TestClient();
        client.Handler.Respond = (_, cancellationToken) => Task.FromResult(StubState.Json(TestClient.InvoiceResponse));
        var url = await client.Client.Api.Invoices.CreatePaymentUrlAsync(new CreateInvoiceRequest { OutSum = 1m });
        Assert.Equal("https://auth.robokassa.ru/merchant/Invoice/test", url);
    }

    [Fact]
    public async Task BuilderCreatesClientForAnotherMerchant()
    {
        using var client = new TestClient(test: true);
        var other = client.Services.GetRequiredService<IRobokassaClientBuilder>().Build(new RobokassaMerchantCredentials
        {
            MerchantLogin = "other",
            TestPassword1 = "pw",
            IsTest = true
        });
        client.Handler.Respond = async (request, cancellationToken) =>
        {
            var payload = await Inspect.Jwt(request, "other:pw");
            Assert.Equal("other", payload.GetProperty("MerchantLogin").GetString());
            return StubState.Json(TestClient.InvoiceResponse);
        };

        await other.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m });
    }
}
