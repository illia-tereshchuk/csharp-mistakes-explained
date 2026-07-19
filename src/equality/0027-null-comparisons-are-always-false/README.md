---
id: "0027"
title: Splitting a set with two opposite comparisons
category: equality
tags: [nullable, comparison, lifted-operators]
rule: "never assume `>` and `<=` together cover a **nullable** value"
---

# #0027 - Splitting a Set With Two Opposite Comparisons

## 💥 Symptom

Two jobs sit in the retry queue forever. The dashboard says the queue has six
items; the worker reports four. Nothing errored, nothing was dead-lettered, no
row is missing from the table - the two items are simply never selected, by
either query, on any run. Every engineer who looks at the code confirms the
logic covers all cases, because it plainly does: one bucket takes everything
above the threshold, the other takes everything at or below it.

## 🔍 The Offending Code

```csharp
var dueNow    = queue.Where(i => i.NextRetryAt <= 0);
var scheduled = queue.Where(i => i.NextRetryAt > 0);   // 💥 null is in neither
```

## 🧠 What's Actually Going On

For nullable values the comparison operators are **lifted**, and a lifted
comparison with a null operand returns **false** - not null, not "unknown".
False.

So when `NextRetryAt` is null, both of these are false at the same time:

```text
null <= 0   ->  false
null >  0   ->  false
```

Two conditions that look like exact opposites are both false for the same
input, and the item falls through the gap between them. The set was never
partitioned; it was filtered twice, and whatever answered "no" to both queries
quietly stopped existing.

The twist is that **equality is null-aware and ordering is not**. `x == null`
works, `null == null` is true, `x != y` behaves - the `==` family handles null
deliberately. The `<`, `>`, `<=`, `>=` family silently answers false and moves
on. Same type, same expression, two different philosophies about null, and no
warning marks the boundary.

## ✅ The Fix

Decide out loud which side null belongs to:

```csharp
var dueNow    = queue.Where(i => i.NextRetryAt is null or <= 0);
var scheduled = queue.Where(i => i.NextRetryAt > 0);
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Pattern `is null or <= 0` | Null has a natural home - say which bucket it belongs to |
| Three buckets, null explicit | Null means "unknown", and unknown deserves its own handling |
| `GetValueOrDefault(x)` / `?? fallback` | The domain has a real default; collapse null before comparing |

The habit worth keeping: whenever you split a set in two, assert that the parts
add back up to the whole. That single check turns this entire class of bug into
a failing test.

## 😈 The Even Worse Sibling

The two buckets go to different consumers. "Due now" runs in the worker, "scheduled"
feeds the dashboard, and nobody ever compares the counts. The lost rows are not
lost from any single view - they are lost from the *sum* of two views nobody
adds up. That is the version that survives for years, because every screen is
individually correct.

## 🎓 Advanced Nuance

This breaks a law you rely on without noticing. For ordinary numbers exactly one
of `<`, `==`, `>` is true, so `!(x > n)` and `x <= n` are interchangeable - the
compiler, and your brain, treat them as the same expression. With a null in
play, all three are false at once, and `!(x > n)` becomes *true* while
`x <= n` stays false. Rewriting a condition by negation, normally a safe
refactor, silently changes which rows you get.

And moving the query to the database does not save you: SQL's three-valued logic
also drops NULL rows from both `WHERE NextRetryAt <= 0` and
`WHERE NextRetryAt > 0`. In-memory LINQ and SQL agree here, so you cannot catch
this by changing layers - only by naming null.

## 🔎 How to Find It in Your Codebase

- Any pair of filters meant to partition a set - `> / <=`, `< / >=` - where the
  compared member is nullable. Check that the counts add up to the total.
- A refactor that flipped a condition with `!` on a nullable comparison. It is
  not the same condition anymore.
- Queue or state tables with a nullable timestamp and a worker that selects by
  it. Rows that were never scheduled are the ones that disappear.
