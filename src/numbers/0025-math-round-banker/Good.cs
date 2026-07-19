// Exhibit #0025: the fix

// Six line items, each landing exactly on half a cent.
decimal[] lineItems = [10.005m, 20.015m, 30.025m, 40.035m, 50.045m, 60.055m];

decimal systemTotal = 0m;
decimal accountantTotal = 0m;

Console.WriteLine("item      system   accountant");
foreach (var item in lineItems)
{
    var system = Math.Round(item, 2, MidpointRounding.AwayFromZero); // the rounding money means
    var accountant = Math.Round(item, 2, MidpointRounding.AwayFromZero);

    systemTotal += system;
    accountantTotal += accountant;

    Console.WriteLine($"{item,-9} {system,-8} {accountant}");
}

Console.WriteLine();
Console.WriteLine($"System invoice total:     {systemTotal}");
Console.WriteLine($"Accountant's paper total: {accountantTotal}");

if (systemTotal != accountantTotal)
{
    throw new InvalidOperationException(
        $"the invoice disagrees with the accountant by {accountantTotal - systemTotal}");
}

Console.WriteLine("The invoice matches the paper. Nobody calls finance.");
