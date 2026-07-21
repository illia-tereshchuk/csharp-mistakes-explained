// Exhibit #0031: handing an async lambda to Parallel.ForEach

using System.Collections.Concurrent;

// Tonight's batch: eight paid orders, each owed a receipt email.
int[] orders = [4001, 4002, 4003, 4004, 4005, 4006, 4007, 4008];
var sent = new ConcurrentQueue<int>();

// "Send them in parallel to speed the nightly job up."
Parallel.ForEach(orders, async order =>
{
    await SendReceiptAsync(order); // 💥 async lambda binds to Action<T> -> async void
    sent.Enqueue(order);
});

// Parallel.ForEach has returned, so every receipt is out. Right?
Console.WriteLine($"Nightly job finished: {sent.Count} of {orders.Length} receipts sent.");

if (sent.Count != orders.Length)
{
    throw new InvalidOperationException(
        $"reported done after sending {sent.Count} of {orders.Length} receipts");
}

Console.WriteLine("Every paid order got its receipt.");

// Stands in for the email gateway. The one-second delay is the story, not the
// proof: the check above runs microseconds after the loop returns, and a receipt
// cannot land for a full second - so "0 sent" is a certainty, not a timing race.
static async Task SendReceiptAsync(int order) =>
    await Task.Delay(TimeSpan.FromSeconds(1));
