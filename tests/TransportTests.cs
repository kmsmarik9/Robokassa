using System.Net;
using KmsDev.Robokassa.Exceptions;
using KmsDev.Robokassa.Requests;

namespace KmsDev.Robokassa.Tests;

public sealed class TransportTests
{
    [Fact]
    public async Task HttpFailureIsReportedOnce()
    {
        using var client = new TestClient();
        client.Handler.Respond = (_, _) => Task.FromResult(StubState.Json("nope", HttpStatusCode.InternalServerError));
        var error = await Assert.ThrowsAsync<RobokassaException>(() =>
            client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m }));
        Assert.Equal(RobokassaErrorKind.Http, error.Kind);
        Assert.Equal(HttpStatusCode.InternalServerError, error.StatusCode);
        Assert.Equal("invoices.CreateInvoice", error.Operation);
        Assert.Equal(1, client.Handler.Calls);
    }

    [Fact]
    public async Task UnsuccessfulJsonIsApiError()
    {
        using var client = new TestClient();
        client.Handler.Respond = (_, _) => Task.FromResult(StubState.Json("{\"isSuccess\":false}"));
        var error = await Assert.ThrowsAsync<RobokassaException>(() =>
            client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m }));
        Assert.Equal(RobokassaErrorKind.Api, error.Kind);
        Assert.Equal(1, client.Handler.Calls);
    }

    [Fact]
    public async Task InvalidJsonIsInvalidResponse()
    {
        using var client = new TestClient();
        client.Handler.Respond = (_, _) => Task.FromResult(StubState.Json("not-json"));
        var error = await Assert.ThrowsAsync<RobokassaException>(() =>
            client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m }));
        Assert.Equal(RobokassaErrorKind.InvalidResponse, error.Kind);
    }
}
