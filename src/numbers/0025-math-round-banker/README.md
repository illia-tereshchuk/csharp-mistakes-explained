---
id: "0025"
title: Rounding money the way school taught you
category: numbers
tags: [Math.Round, MidpointRounding, decimal]
rule: "never assume `Math.Round` rounds **half up**"
---

# #0025 - Rounding Money the Way School Taught You

## 💥 Symptom

Finance reports that the invoice total is a few cents off from what they
computed by hand. Not every invoice - most match perfectly, which is why it took
three months to notice and why the first two engineers who looked at it found
nothing. The amounts involved are trivial; the audit finding is not.

## 🔍 The Offending Code

```csharp
var line = Math.Round(10.005m, 2);   // 10.00, not 10.01
```

Every human on earth was taught that a half rounds up. `Math.Round` was not.

## 🧠 What's Actually Going On

`Math.Round` defaults to `MidpointRounding.ToEven` - "banker's rounding". A
value sitting exactly on the midpoint goes to the nearest **even** last digit,
not always upward:

```text
10.005 -> 10.00   (0 is even)
20.015 -> 20.02   (2 is even - agrees with school)
30.025 -> 30.02   (2 is even)
40.035 -> 40.04   (4 is even - agrees again)
```

That alternation is the whole reason this survives so long in production: the
default agrees with the accountant *half the time*, so the discrepancy looks
random rather than systematic.

The default is not a mistake. Rounding half up on a large set of numbers biases
every sum slightly upward; to-even cancels out over many values, which is what
you want for statistics - and it is the IEEE 754 default, so it shows up far
beyond .NET. It is simply the wrong policy for an invoice, where the contract
says "half a cent goes up" because a human wrote that sentence.

## ✅ The Fix

State the policy - never inherit it:

```csharp
Math.Round(10.005m, 2, MidpointRounding.AwayFromZero);   // 10.01
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `MidpointRounding.AwayFromZero` | Money and anything a human will check by hand - matches the written contract |
| `MidpointRounding.ToEven` (the default) | Statistics and large aggregates, where an upward bias would accumulate |
| Round once, at the boundary | Keep full precision through the calculation and round only when you present or settle - fewer rounding events, fewer places to disagree |

## 😈 The Even Worse Sibling

Rounding in stages. A value gets rounded to three decimals in one service,
then to two in another; both steps are individually correct, and the answer
differs from rounding once, because the first round manufactured a new midpoint
that never existed in the source data. Nobody owns the bug - each service is
right on its own.

## 🎓 Advanced Nuance

The number you compute and the number you print can disagree inside the same
framework. Formatting does **not** use `Math.Round`'s policy:

```csharp
Math.Round(10.005m, 2)      // 10.00  - to even
(10.005m).ToString("F2")    // 10.01  - away from zero
```

So a value stored after `Math.Round` and a value rendered with `F2` can differ
by a cent, and the invoice on screen will not match the invoice in the database.

Second: all of the above assumes `decimal`. Do this with `double` and there is
no midpoint to round at all - `10.005` is not exactly 10.005 in binary (see
[0002-doubles-for-money](../../numbers/0002-doubles-for-money/)), so the result
depends on which side of the midpoint the stored value actually landed. Two
different bugs stacked in one expression.

## 🔎 How to Find It in Your Codebase

- Grep `Math.Round(` and `decimal.Round(` - every call without an explicit
  `MidpointRounding` argument is inheriting a policy nobody chose.
- Money paths that round more than once between input and settlement.
- Test with values that land exactly on the midpoint (`x.xx5`), and check both
  parities - one even and one odd last digit. A suite full of `0.10` and `0.25`
  will never see this.
