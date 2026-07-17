// Exhibit #0009: the fix

// The premium report: a header with a count, then the rows.
var products = new List<Product>
{
    new("Espresso machine", 450m),
    new("Grinder", 120m),
    new("Kettle", 35m),
    new("Scale", 60m),
    new("Roaster", 2800m),
};

var premium = products.Where(p => p.Price > 100m).ToList(); // the recipe runs ONCE, here

int headerCount = premium.Count; // just reads a stored number
Console.WriteLine($"Premium products: {headerCount}");

// Black Friday hits between the header and the body.
foreach (var product in products)
{
    product.Price *= 0.5m;
}

int printed = 0;
foreach (var product in premium) // walks the snapshot, no re-run
{
    Console.WriteLine($"  {product.Name}: {product.Price}");
    printed++;
}

if (printed != headerCount)
{
    throw new InvalidOperationException(
        $"The header promised {headerCount} rows, the body printed {printed}");
}

Console.WriteLine("Header and body agree. Report shipped.");

class Product(string name, decimal price)
{
    public string Name { get; } = name;
    public decimal Price { get; set; } = price;
}
