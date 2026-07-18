// Exhibit #0020: the fix

var signup = new DateTime(2026, 1, 31);

Console.WriteLine($"Signed up:   {signup:yyyy-MM-dd}");
Console.WriteLine("Billing schedule:");

var billing = signup;
for (int month = 1; month <= 12; month++)
{
    billing = signup.AddMonths(month); // every date computed from the ANCHOR
    Console.WriteLine($"  {billing:yyyy-MM-dd}");
}

Console.WriteLine($"Anniversary: {billing:yyyy-MM-dd}");

if (billing.Day != signup.Day)
{
    throw new InvalidOperationException(
        $"signed up on day {signup.Day}, anniversary on day {billing.Day} - the billing day drifted and never recovered");
}

Console.WriteLine("Twelve charges later, the billing day is still the billing day.");
