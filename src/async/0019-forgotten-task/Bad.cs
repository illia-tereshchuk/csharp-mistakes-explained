// Exhibit #0019: the fire-and-forgotten task

var auditLog = new List<string>();
var failureReported = false;

try
{
    await ProcessOrder("ORD-1042");
    Console.WriteLine("Order pipeline reports: success");
}
catch (Exception ex)
{
    failureReported = true;
    Console.WriteLine($"Order pipeline reports: FAILED - {ex.Message}");
}

await Task.Delay(300); // the process lives on; the audit had every chance to finish

Console.WriteLine($"Audit records: {auditLog.Count}");

if (auditLog.Count == 0 && !failureReported)
{
    throw new InvalidOperationException(
        "no audit record, no error report - the failure is buried inside a task nobody holds");
}

Console.WriteLine("Either the record exists or someone was told it doesn't. Both are fine.");

async Task ProcessOrder(string orderId)
{
    await ChargeCard(orderId);
    SaveAuditRecord(orderId); // 💥 fire and forget - "audit must not slow the request"
}

async Task ChargeCard(string orderId) => await Task.Delay(20);

async Task SaveAuditRecord(string orderId)
{
    await Task.Delay(50); // the write to the audit store
    ValidateSchema();
    auditLog.Add(orderId);
}

void ValidateSchema()
    => throw new InvalidOperationException("audit store rejected the record: schema mismatch");
