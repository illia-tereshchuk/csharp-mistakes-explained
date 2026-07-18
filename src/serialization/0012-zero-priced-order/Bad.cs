#:property PublishAot=false

// Exhibit #0012: a JSON payload that maps to nothing

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

var order = JsonSerializer.Deserialize<Order>(json)!;

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
    public int OrderId { get; set; }
    public string Customer { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
}
