// Exhibit #0011: the fix

// The coffee shop loyalty card: three purchases, points every time.
var card = new LoyaltyCard();

card.Register(100);
card.Register(100);
card.Register(100);

Console.WriteLine("Purchases registered: 3 x 100 points");
Console.WriteLine($"Card balance:         {card.Balance}");

if (card.Balance != 300)
{
    throw new InvalidOperationException(
        $"300 points were added, the card holds {card.Balance}");
}

Console.WriteLine("All points accounted for. Enjoy your free espresso.");

class LoyaltyCard
{
    private PointsBalance _balance; // the field mutates; the struct never does

    public int Balance => _balance.Value;

    public void Register(int points) => _balance = _balance.Add(points); // explicit value semantics
}

readonly struct PointsBalance
{
    private readonly int _value;

    private PointsBalance(int value) => _value = value;

    public int Value => _value;

    public PointsBalance Add(int points) => new(_value + points);
}
