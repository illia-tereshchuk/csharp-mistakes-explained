// Exhibit #0028: with-copying a record that holds a mutable collection

// The order the customer confirmed, kept for the audit trail.
var confirmed = new Order(1042, "Confirmed", ["Laptop", "Mouse"]);

var auditTrail = new List<Order> { confirmed };
Console.WriteLine($"Audit snapshot: {confirmed.Status}, {confirmed.Items.Count} items");

// Support opens a revision. `with` gives us a fresh record to edit.
var revised = confirmed with { Status = "Revised" };
revised.Items.Add("Extended warranty"); // 💥 the audit snapshot grows too

Console.WriteLine($"Revision:       {revised.Status}, {revised.Items.Count} items");
Console.WriteLine($"Audit snapshot: {auditTrail[0].Status}, {auditTrail[0].Items.Count} items");

if (auditTrail[0].Items.Count != 2)
{
    throw new InvalidOperationException(
        $"the audit trail now shows {auditTrail[0].Items.Count} items - the customer confirmed 2");
}

Console.WriteLine("The audit still shows what the customer agreed to.");

record Order(int Id, string Status, List<string> Items);
