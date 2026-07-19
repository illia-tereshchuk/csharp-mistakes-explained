// Exhibit #0028: the fix

// The order the customer confirmed, kept for the audit trail.
var confirmed = new Order(1042, "Confirmed", ["Laptop", "Mouse"]);

var auditTrail = new List<Order> { confirmed };
Console.WriteLine($"Audit snapshot: {confirmed.Status}, {confirmed.Items.Count} items");

// Support opens a revision. The collection is copied along with the record.
var revised = confirmed with { Status = "Revised", Items = [.. confirmed.Items] };
revised.Items.Add("Extended warranty");

Console.WriteLine($"Revision:       {revised.Status}, {revised.Items.Count} items");
Console.WriteLine($"Audit snapshot: {auditTrail[0].Status}, {auditTrail[0].Items.Count} items");

if (auditTrail[0].Items.Count != 2)
{
    throw new InvalidOperationException(
        $"the audit trail now shows {auditTrail[0].Items.Count} items - the customer confirmed 2");
}

Console.WriteLine("The audit still shows what the customer agreed to.");

record Order(int Id, string Status, List<string> Items);
