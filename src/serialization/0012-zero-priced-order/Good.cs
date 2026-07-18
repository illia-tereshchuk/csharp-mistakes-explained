#:property PublishAot=false

// Exhibit #0012: the fix

using System.Text.Json;

// The payment webhook body, exactly as the gateway sends it.
var json = """
{
    "orderId": 1042,
    "customer": "Olena",
    "amount": 149.99,
    "currency": "EUR"
}
""";

// Web defaults: camelCase naming, case-insensitive matching - what ASP.NET uses.
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
var order = JsonSerializer.Deserialize<Order>(json, options)!;

Console.WriteLine($"Order #{order.OrderId} for customer '{order.Customer}'");
Console.WriteLine($"Charged: {order.Amount} {order.Currency}");

if (order.Amount != 149.99m)
{
    throw new InvalidOperationException(
        $"The gateway charged 149.99, we recorded {order.Amount}");
}

Console.WriteLine("Amounts match. Books are balanced.");

class Order
{
    public required int OrderId { get; set; }
    public required string Customer { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
}
