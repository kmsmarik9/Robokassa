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
        Assert.Equal("CreateInvoiceRequest", error.Operation);
        Assert.Equal("ошибка robokassa: Http, операция CreateInvoiceRequest: nope", error.Message);
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
        Assert.Equal("ошибка robokassa: Api, операция CreateInvoiceRequest", error.Message);
        Assert.Equal(1, client.Handler.Calls);
    }

    [Fact]
    public async Task UnsuccessfulJsonKeepsApiMessage()
    {
        using var client = new TestClient();
        client.Handler.Respond = (_, _) => Task.FromResult(StubState.Json("""{"isSuccess":false,"message":"Некорректно составлен запрос"}"""));
        var error = await Assert.ThrowsAsync<RobokassaException>(() =>
            client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m }));
        Assert.Equal(RobokassaErrorKind.Api, error.Kind);
        Assert.Equal("ошибка robokassa: Api, операция CreateInvoiceRequest: Некорректно составлен запрос", error.Message);
    }

    [Fact]
    public async Task InvalidJsonIsInvalidResponse()
    {
        using var client = new TestClient();
        client.Handler.Respond = (_, _) => Task.FromResult(StubState.Json("not-json"));
        var error = await Assert.ThrowsAsync<RobokassaException>(() =>
            client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m }));
        Assert.Equal(RobokassaErrorKind.InvalidResponse, error.Kind);
        Assert.StartsWith("ошибка robokassa: InvalidResponse, операция CreateInvoiceRequest:", error.Message);
        Assert.IsAssignableFrom<System.Text.Json.JsonException>(error.InnerException);
    }

    [Fact]
    public async Task TransportFailureKeepsOriginalMessage()
    {
        using var client = new TestClient();
        client.Handler.Respond = (_, _) => throw new HttpRequestException("соединение сброшено");
        var error = await Assert.ThrowsAsync<RobokassaException>(() =>
            client.Client.Api.Invoices.CreateAsync(new CreateInvoiceRequest { OutSum = 1m }));
        Assert.Equal(RobokassaErrorKind.Transport, error.Kind);
        Assert.Equal("ошибка robokassa: Transport, операция CreateInvoiceRequest: соединение сброшено", error.Message);
        Assert.IsType<HttpRequestException>(error.InnerException);
    }
}
