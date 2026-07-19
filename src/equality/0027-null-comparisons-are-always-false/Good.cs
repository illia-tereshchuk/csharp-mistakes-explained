// Exhibit #0027: the fix

// The retry queue. NextRetryAt is null for items that were never scheduled.
var queue = new List<QueueItem>
{
    new("job-1", NextRetryAt: -30),  // overdue
    new("job-2", NextRetryAt: -5),   // overdue
    new("job-3", NextRetryAt: 15),   // scheduled for later
    new("job-4", NextRetryAt: 60),   // scheduled for later
    new("job-5", NextRetryAt: null), // never scheduled
    new("job-6", NextRetryAt: null), // never scheduled
};

// The worker splits the queue in two, and says out loud where null belongs.
var dueNow = queue.Where(i => i.NextRetryAt is null or <= 0).ToList();
var scheduled = queue.Where(i => i.NextRetryAt > 0).ToList();

Console.WriteLine($"Queue size:      {queue.Count}");
Console.WriteLine($"Due now:         {dueNow.Count} ({string.Join(", ", dueNow.Select(i => i.Id))})");
Console.WriteLine($"Scheduled later: {scheduled.Count} ({string.Join(", ", scheduled.Select(i => i.Id))})");

var accountedFor = dueNow.Count + scheduled.Count;
Console.WriteLine($"Accounted for:   {accountedFor} of {queue.Count}");

if (accountedFor != queue.Count)
{
    var lost = queue.Count - accountedFor;
    throw new InvalidOperationException(
        $"{lost} item(s) are in neither bucket - they will never be picked up by either query");
}

Console.WriteLine("Every item is in exactly one bucket. The queue drains.");

record QueueItem(string Id, int? NextRetryAt);
