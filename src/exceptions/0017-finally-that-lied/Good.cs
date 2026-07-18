// Exhibit #0017: the fix

try
{
    ProcessPayment();
}
catch (Exception ex)
{
    Console.WriteLine("The error that reaches the logs:");
    Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
    Console.WriteLine();

    if (!ex.Message.Contains("gateway"))
    {
        throw new InvalidOperationException(
            $"the investigation starts from {ex.GetType().Name} - the real cause never reached the logs");
    }

    Console.WriteLine("The log tells the true story - debuggable.");
}

void ProcessPayment()
{
    GatewayConnection? connection = null;

    try
    {
        connection = GatewayConnection.Open(); // fails before assigning anything
        connection.Charge(149.99m);
    }
    finally
    {
        connection?.Close(); // cleanup guards its own state, the real error flies on
    }
}

class GatewayConnection
{
    public static GatewayConnection Open()
        => throw new InvalidOperationException("payment gateway rejected the connection: invalid merchant key");

    public void Charge(decimal amount) { }

    public void Close() { }
}
