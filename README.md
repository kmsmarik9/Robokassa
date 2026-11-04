# KmsDev.Robokassa


Денежные вызовы не повторяются автоматически. Pipeline запроса — только timeout. `IsTest` по умолчанию `true`.

## Подключение

```csharp
using KmsDev.Robokassa;
using KmsDev.Robokassa.Models;
using KmsDev.Robokassa.Requests;

services.AddRobokassaSystem(configurer =>
{
    configurer.AddSingletonClient(new RobokassaMerchantCredentials
    {
        MerchantLogin = "demo",
        Password1 = "password1",
        Password2 = "password2",
        IsTest = false
    });
});
```

Несколько магазинов:

```csharp
var builder = serviceProvider.GetRequiredService<IRobokassaClientBuilder>();
var client = builder.Build(new RobokassaMerchantCredentials
{
    MerchantLogin = "demo",
    TestPassword1 = "test-password-1",
    TestPassword2 = "test-password-2"
});
```

## Счёт

```csharp
var invoice = await client.Api.Invoices.CreateAsync(new CreateInvoiceRequest
{
    InvId = 1001,
    OutSum = 100.00m,
    Description = "Заказ №1001",
    InvoiceItems =
    [
        new InvoiceItem
        {
            Name = "Услуга",
            Quantity = 1,
            Cost = 100.00m,
            Tax = "none"
        }
    ]
});
```


## ResultURL

```csharp
var notification = client.Api.Notifications.VerifyResultUrl(fields, expectedAmount: order.Amount);
var response = client.Api.Notifications.ResultResponse(notification);
```

`fields` — декодированные поля формы. Повторяющиеся ключи отклоняются. `OutSum` сравнивается как исходная строка. Ответ `OK{InvId}` отправляйте после записи оплаты. SuccessURL проверяется через `VerifySuccessUrl` и сам по себе оплату не подтверждает.

`VerifyResultUrl2` принимает компактный JWS и заранее полученный сертификат Robokassa.
