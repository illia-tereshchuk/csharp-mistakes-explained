// Exhibit #0031: the fix

using System.Collections.Concurrent;

// Tonight's batch: eight paid orders, each owed a receipt email.
int[] orders = [4001, 4002, 4003, 4004, 4005, 4006, 4007, 4008];
var sent = new ConcurrentQueue<int>();

// Parallel.ForEachAsync takes a Func<T, CancellationToken, ValueTask> and awaits
// every body, so the method does not return until the last receipt is out.
await Parallel.ForEachAsync(orders, async (order, ct) =>
{
    await SendReceiptAsync(order);
    sent.Enqueue(order);
});

// Parallel.ForEachAsync has returned, so every receipt is out.
Console.WriteLine($"Nightly job finished: {sent.Count} of {orders.Length} receipts sent.");

if (sent.Count != orders.Length)
{
    throw new InvalidOperationException(
        $"reported done after sending {sent.Count} of {orders.Length} receipts");
}

Console.WriteLine("Every paid order got its receipt.");

// Stands in for the email gateway - the same one-second call as in Bad.cs.
static async Task SendReceiptAsync(int order) =>
    await Task.Delay(TimeSpan.FromSeconds(1));
