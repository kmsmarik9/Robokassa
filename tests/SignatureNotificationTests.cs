using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using KmsDev.Robokassa.Exceptions;
using KmsDev.Robokassa.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KmsDev.Robokassa.Tests;

public sealed class SignatureNotificationTests
{
    [Theory]
    [InlineData(RobokassaHashAlgorithm.Md5, "c76b1b2ce919d2475134b5de5b379618")]
    [InlineData(RobokassaHashAlgorithm.Ripemd160, "d2095bbe06043637f8e5815404686ce217d803b9")]
    [InlineData(RobokassaHashAlgorithm.Sha1, "5782845c756e9e852aea191d53b3b8e987adf61f")]
    [InlineData(RobokassaHashAlgorithm.Sha256, "c71475e78058610a889d314755fc7c9038fda650e70f4b0bfa8fb0f08a6e797a")]
    [InlineData(RobokassaHashAlgorithm.Sha384, "1fec598c208d8ae43d59027bf847f5fcfb964229b4b7197f6d56ab8e9026c3f22e89250538a908c7c94cf6b47447004c")]
    [InlineData(RobokassaHashAlgorithm.Sha512, "710bb513633aab791626479cd8400fb47bb2068a3af47a10f207c54988e670ef1252471de5eb291486a439ad8ea69c6f5b29047234e328f56c13012b796a5c33")]
    public void HashMatchesKnownSignatures(RobokassaHashAlgorithm algorithm, string expected)
    {
        Assert.Equal(expected, RobokassaSignature.Hash("demo:100.00:42:test", algorithm));
    }

    [Fact]
    public void ResultPreservesRawSumAndSortsShp()
    {
        using var client = Client(new RobokassaMerchantCredentials
        {
            MerchantLogin = "demo",
            Password2 = "test",
            IsTest = false
        });
        var fields = Fields();
        var result = client.Api.Notifications.VerifyResultUrl(fields, 100m);
        Assert.Equal("100.000000", result.OutSum);
        Assert.Equal(100m, result.Amount);
        Assert.Equal("OK42", client.Api.Notifications.ResultResponse(result));
        Assert.Throws<RobokassaException>(() => client.Api.Notifications.VerifyResultUrl(fields, 101m));
    }

    [Theory]
    [InlineData("OutSum", "101.000000")]
    [InlineData("InvId", "43")]
    [InlineData("SignatureValue", "gggg")]
    [InlineData("Shp_a", "подмена")]
    public void RejectsTamperedCallback(string key, string value)
    {
        using var client = Client(new RobokassaMerchantCredentials
        {
            MerchantLogin = "demo",
            Password2 = "test",
            IsTest = false
        });
        var fields = Fields();
        fields[key] = value;
        Assert.Throws<RobokassaException>(() => client.Api.Notifications.VerifyResultUrl(fields));
    }

    [Fact]
    public void DuplicateCaseInsensitiveFieldsAreRejected()
    {
        using var client = new TestClient();
        var fields = Fields().ToList();
        fields.Add(new KeyValuePair<string, string>("outsum", "1"));
        Assert.Throws<RobokassaException>(() => client.Client.Api.Notifications.VerifyResultUrl(fields));
    }

    [Fact]
    public void SuccessCannotBeUsedToAcknowledgeResult()
    {
        using var client = Client(new RobokassaMerchantCredentials
        {
            MerchantLogin = "demo",
            Password1 = "test",
            IsTest = false
        });
        var notification = client.Api.Notifications.VerifySuccessUrl(Fields());
        Assert.False(notification.IsResultNotification);
        Assert.Throws<ArgumentException>(() => client.Api.Notifications.ResultResponse(notification));
    }

    [Fact]
    public void TestCallbacksUseTestPassword2()
    {
        using var client = new TestClient(test: true);
        var fields = new Dictionary<string, string>
        {
            ["OutSum"] = "100.00",
            ["InvId"] = "42",
            ["SignatureValue"] = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes("100.00:42:test2")))
        };
        Assert.Equal(100m, client.Client.Api.Notifications.VerifyResultUrl(fields).Amount);
    }

    [Fact]
    public void JwsRequiresTrustedCertificateCorrectMerchantAndSignature()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=fixture", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        using var client = new TestClient();
        var jws = MakeJws(rsa, "demo");
        var result = client.Client.Api.Notifications.VerifyResultUrl2(jws, certificate, maxAge: TimeSpan.FromMinutes(5));
        Assert.Equal("HOLD", result.Data.State);
        Assert.Throws<RobokassaException>(() => client.Client.Api.Notifications.VerifyResultUrl2(MakeJws(rsa, "other-shop"), certificate));
        Assert.Throws<RobokassaException>(() => client.Client.Api.Notifications.VerifyResultUrl2(jws[..^4] + "aaaa", certificate));
        Assert.Throws<RobokassaException>(() => client.Client.Api.Notifications.VerifyResultUrl2(jws, certificate, expectedAlgorithm: "none"));
        Assert.Throws<RobokassaException>(() => client.Client.Api.Notifications.VerifyResultUrl2(jws, certificate, maxAge: TimeSpan.FromMinutes(5), now: DateTimeOffset.UtcNow.AddDays(1)));
        using var other = RSA.Create(2048);
        using var otherCertificate = new CertificateRequest("CN=wrong", other, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
            .CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        Assert.Throws<RobokassaException>(() => client.Client.Api.Notifications.VerifyResultUrl2(jws, otherCertificate));
    }

    private static string MakeJws(RSA rsa, string shop)
    {
        var header = Inspect.B64("{\"alg\":\"RS256\",\"typ\":\"JWT\"}");
        var payload = Inspect.B64(JsonSerializer.Serialize(new
        {
            header = new { type = "PaymentStateNotification", version = "1.0.0", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            data = new { shop, opKey = "op", invId = "42", paymentMethod = "BankCard", incSum = "100.00", state = "HOLD" }
        }));
        var input = header + "." + payload;
        return input + "." + Inspect.B64(rsa.SignData(Encoding.ASCII.GetBytes(input), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    private static Dictionary<string, string> Fields() => new()
    {
        ["OutSum"] = "100.000000",
        ["InvId"] = "42",
        ["SignatureValue"] = "4E4AFAA4B39D1963FD295097C49375E3",
        ["Shp_b"] = "2",
        ["Shp_a"] = "заказ"
    };

    private static OwnedClient Client(RobokassaMerchantCredentials credentials)
    {
        var services = new ServiceCollection();
        services.AddRobokassaSystem(configurer => configurer.AddSingletonClient(credentials));
        return new OwnedClient(services.BuildServiceProvider());
    }

    private sealed class OwnedClient : IDisposable
    {
        private readonly ServiceProvider _provider;

        public OwnedClient(ServiceProvider provider)
        {
            _provider = provider;
            Api = provider.GetRequiredService<IRobokassaClient>().Api;
        }

        public RobokassaClientApiContainer Api { get; }

        public void Dispose() => _provider.Dispose();
    }
}
