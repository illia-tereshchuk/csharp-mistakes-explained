// Exhibit #0011: a mutable struct behind a readonly field

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
    private readonly PointsBalance _balance; // readonly field + mutable struct = the ambush

    public int Balance => _balance.Value;

    public void Register(int points) => _balance.Add(points); // 💥 mutates a hidden copy
}

struct PointsBalance
{
    private int _value;

    public int Value => _value;

    public void Add(int points) => _value += points;
}
