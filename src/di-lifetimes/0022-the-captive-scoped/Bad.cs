#:package Microsoft.Extensions.DependencyInjection@10.*
#:property PublishAot=false

// Exhibit #0022: a singleton that captured a scoped dependency

using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddScoped<CurrentUser>();       // one per request
services.AddSingleton<CartService>();    // one for the whole app - and it takes a CurrentUser

using var root = services.BuildServiceProvider();

// Request A: Olena signs in and adds a laptop to her cart.
using (var requestA = root.CreateScope())
{
    var cart = requestA.ServiceProvider.GetRequiredService<CartService>();
    cart.SignIn("Olena");
    cart.Add("Laptop");
    Console.WriteLine($"Olena's session -> owner: {cart.Owner}, items: [{cart.ItemList}]");
}

// Request B: Ivan opens the app fresh. Different user, different scope, new session.
using (var requestB = root.CreateScope())
{
    var cart = requestB.ServiceProvider.GetRequiredService<CartService>();
    Console.WriteLine($"Ivan's session  -> owner: {cart.Owner}, items: [{cart.ItemList}]");

    if (cart.Owner == "Olena" || cart.Items.Count > 0) // 💥
    {
        throw new InvalidOperationException(
            $"Ivan is looking at {cart.Owner}'s cart - the singleton froze onto one CurrentUser for the whole app");
    }
}

Console.WriteLine("Each session saw its own user.");

class CurrentUser
{
    public string Name { get; set; } = "(anonymous)";
    public List<string> Items { get; } = [];
}

class CartService(CurrentUser user) // captures the first CurrentUser it is handed, forever
{
    public string Owner => user.Name;
    public IReadOnlyList<string> Items => user.Items;
    public string ItemList => string.Join(", ", user.Items);

    public void SignIn(string name) => user.Name = name;
    public void Add(string item) => user.Items.Add(item);
}
