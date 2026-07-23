// Exhibit #0032: the fix

#:package Microsoft.Extensions.Logging.Abstractions@9.0.0

using Microsoft.Extensions.Logging;

// A capturing sink stands in for Seq / Elasticsearch: it keeps the structured
// state the logger received, exactly as a real sink would index it.
var sink = new CapturingLogger();

int customerId = 4087;
decimal amount = 149.99m;

// The charge failed; we log it with a message template - the values stay named.
sink.LogWarning("Payment declined for customer {CustomerId}, amount {Amount}", customerId, amount);

// During the incident, on-call searches the sink for everything about customer 4087.
int hits = sink.CountWhere("CustomerId", customerId);

Console.WriteLine($"Log line:         {sink.Entries[0].Message}");
Console.WriteLine($"Search CustomerId=4087 -> {hits} match(es)");

// The template recorded CustomerId as a real field, so the search resolves it.
if (hits != 1)
{
    throw new InvalidOperationException(
        $"incident search for CustomerId=4087 found {hits} matches - the field was never recorded");
}

Console.WriteLine("Found the customer's failures in seconds.");

// --- a minimal structured sink; a real one (Seq, App Insights) indexes the same state ---
class CapturingLogger : ILogger
{
    public List<LogEntry> Entries { get; } = new();

    public void Log<TState>(LogLevel level, EventId id, TState state,
        Exception? error, Func<TState, Exception?, string> formatter)
    {
        var fields = new Dictionary<string, object?>();
        if (state is IReadOnlyList<KeyValuePair<string, object?>> pairs)
            foreach (var pair in pairs)
                fields[pair.Key] = pair.Value; // what a sink indexes and lets you query
        Entries.Add(new LogEntry(formatter(state, error), fields));
    }

    public int CountWhere(string field, object value) =>
        Entries.Count(e => e.Fields.TryGetValue(field, out var v) && Equals(v, value));

    public bool IsEnabled(LogLevel level) => true;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}

record LogEntry(string Message, Dictionary<string, object?> Fields);
