using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KmsDev.Robokassa.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace KmsDev.Robokassa.Tests;

internal sealed class StubState
{
    internal Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Respond { get; set; } =
        (_, _) => Task.FromResult(Json("{\"isSuccess\":true}"));

    internal int Calls { get; set; }

    internal static HttpResponseMessage Json(string value, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(value, Encoding.UTF8, "application/json") };
}

internal sealed class ForwardingHandler : HttpMessageHandler
{
    private readonly StubState _state;

    public ForwardingHandler(StubState state) => _state = state;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _state.Calls++;
        return _state.Respond(request, cancellationToken);
    }
}

internal sealed class TestClient : IDisposable
{
    private readonly ServiceProvider _provider;

    internal StubState Handler { get; } = new();

    internal IRobokassaClient Client { get; }

    internal IServiceProvider Services => _provider;

    internal TestClient(bool test = false, RobokassaHashAlgorithm algorithm = RobokassaHashAlgorithm.Md5)
    {
        var services = new ServiceCollection();
        var state = Handler;
        services.AddRobokassaSystem(configurer =>
        {
            configurer.SetServicesBaseAddress("https://services.example/");
            configurer.SetMerchantBaseAddress("https://merchant.example/");
            configurer.ConfigureHttpClient = builder =>
                builder.ConfigurePrimaryHttpMessageHandler(() => new ForwardingHandler(state));
            configurer.AddSingletonClient(new RobokassaMerchantCredentials
            {
                MerchantLogin = "demo",
                Password1 = "test",
                Password2 = "result",
                Password3 = "refund",
                TestPassword1 = "test1",
                TestPassword2 = "test2",
                IsTest = test,
                HashAlgorithm = algorithm
            });
        });
        _provider = services.BuildServiceProvider();
        Client = _provider.GetRequiredService<IRobokassaClient>();
    }

    internal const string InvoiceResponse = """{"isSuccess":true,"id":"01234567-89ab-4cde-8f01-23456789abcd","invId":42,"url":"https://auth.robokassa.ru/merchant/Invoice/test"}""";

    public void Dispose() => _provider.Dispose();
}

internal static class Inspect
{
    internal static byte[] Decode(string value) =>
        Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/') + new string('=', (4 - value.Length % 4) % 4));

    internal static string B64(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    internal static string B64(string value) => B64(Encoding.UTF8.GetBytes(value));

    internal static async Task<JsonElement> Jwt(HttpRequestMessage request, string key = "demo:test", string expectedAlg = "MD5")
    {
        Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
        var raw = await request.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(raw);
        Assert.Equal(JsonValueKind.String, json.RootElement.ValueKind);
        var parts = json.RootElement.GetString()!.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.All(parts, part => Assert.DoesNotContain("=", part));
        using var header = JsonDocument.Parse(Decode(parts[0]));
        Assert.Equal(expectedAlg, header.RootElement.GetProperty("alg").GetString());
        var keyBytes = Encoding.UTF8.GetBytes(key);
        using HMAC hmac = expectedAlg switch
        {
            "MD5" => new HMACMD5(keyBytes),
            "HS1" => new HMACSHA1(keyBytes),
            "HS256" => new HMACSHA256(keyBytes),
            "HS384" => new HMACSHA384(keyBytes),
            "HS512" => new HMACSHA512(keyBytes),
            _ => throw new ArgumentException()
        };
        Assert.Equal(hmac.ComputeHash(Encoding.ASCII.GetBytes(parts[0] + "." + parts[1])), Decode(parts[2]));
        return JsonDocument.Parse(Decode(parts[1])).RootElement.Clone();
    }
}
