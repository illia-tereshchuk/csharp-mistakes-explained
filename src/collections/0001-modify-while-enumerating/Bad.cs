// Exhibit #0001: modifying a collection while iterating over it

var subscribers = new List<Subscriber>
{
    new("olena@example.com", IsActive: true),
    new("ivan@example.com", IsActive: false),
    new("petro@example.com", IsActive: false),
    new("maria@example.com", IsActive: true),
};

Console.WriteLine($"Subscribers before cleanup: {subscribers.Count}");

// Purging inactive subscribers... right in the middle of the loop
foreach (var subscriber in subscribers)
{
    if (!subscriber.IsActive)
    {
        Console.WriteLine($"Removing {subscriber.Email}");
        subscribers.Remove(subscriber); // 💥 the bomb is planted here
    }
}

Console.WriteLine($"Subscribers after cleanup: {subscribers.Count}");

record Subscriber(string Email, bool IsActive);
