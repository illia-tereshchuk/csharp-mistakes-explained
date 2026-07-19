// Exhibit #0021: the fix

// Ship one order out to every warehouse in parallel.
var warehouses = new (string Name, string? Failure)[]
{
    ("Kyiv", null),
    ("Lviv", "conveyor jam"),
    ("Odesa", "out of stock"),
    ("Kharkiv", null),
    ("Dnipro", "label printer offline"),
};

var shipments = warehouses.Select(Ship).ToList();

var reported = new List<string>();
try
{
    await Task.WhenAll(shipments);
}
catch
{
    // WhenAll rethrew one fault; the complete set lives on the tasks themselves.
    foreach (var faulted in shipments.Where(s => s.IsFaulted))
        foreach (var ex in faulted.Exception!.InnerExceptions)
            reported.Add(ex.Message);
}

int actuallyFailed = warehouses.Count(w => w.Failure is not null);

Console.WriteLine($"Warehouses that failed:   {actuallyFailed}");
Console.WriteLine($"Failures the handler saw: {reported.Count}");
foreach (var r in reported)
    Console.WriteLine($"  - {r}");

if (reported.Count < actuallyFailed)
{
    throw new InvalidOperationException(
        $"{actuallyFailed - reported.Count} shipment failures were silently dropped");
}

Console.WriteLine("Every failed shipment was reported.");

Task Ship((string Name, string? Failure) warehouse) => Task.Run(async () =>
{
    await Task.Delay(20); // the dispatch call
    if (warehouse.Failure is not null)
        throw new InvalidOperationException($"{warehouse.Name}: {warehouse.Failure}");
});
