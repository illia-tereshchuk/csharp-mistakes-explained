---
id: "0028"
title: With-copying a record that holds a mutable collection
category: records
tags: [records, with-expression, shallow-copy]
rule: "never put a **mutable collection** in a record"
---

# #0028 - With-Copying a Record That Holds a Mutable Collection

## 💥 Symptom

The audit trail disagrees with itself. A dispute comes in: the customer says
they ordered two things, the confirmation email says two things, and the audit
record - the immutable snapshot of what was agreed, stored before anyone touched
it - lists three. The extra line was added during a revision hours later, to a
copy. Nobody edited the snapshot; there is no code that can edit the snapshot.

## 🔍 The Offending Code

```csharp
record Order(int Id, string Status, List<string> Items);

var revised = confirmed with { Status = "Revised" };
revised.Items.Add("Extended warranty");   // 💥 confirmed.Items grew too
```

## 🧠 What's Actually Going On

`with` performs a **shallow** copy: the compiler-generated copy constructor
copies each member as-is. For `Id` and `Status` that means new values. For
`Items` it means the same `List<string>` reference - one list, now reachable
from two records.

Records promise value semantics *for the record*. They cannot extend that
promise to what the record holds, and nothing in the syntax hints at the
boundary. `with` reads like "give me an independent copy", the type is called
immutable in every article about it, the members are init-only - and the list
inside is as mutable and as shared as it ever was. The immutability is one
level deep.

That is what makes the audit case so expensive: the whole point of the snapshot
was that nobody could change it, and the guarantee people relied on was the word
"record".

The rule this leaves you with is worth more than the bug itself: **a record is a
value only as deep as its members are values.** `int`, `string`, `DateTime`,
another record - all fine, all immutable, nothing to share. Put a `List`, an
array, a `Dictionary`, or any class you can mutate inside it, and you have a
class wearing a record's clothes: the syntax, the `with`, the generated
`Equals`, and none of the guarantees they imply. Records earn their keep on
flat, JSON-shaped data. The moment a mutable reference gets in, you have to
reason about aliasing exactly as you would with a class - except now nobody on
the team expects to.

## ✅ The Fix

Copy the collection along with the record:

```csharp
var revised = confirmed with { Status = "Revised", Items = [.. confirmed.Items] };
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Copy the collection in the `with` | A one-off revision; explicit at the call site where the copy is created |
| Hold an immutable type (`ImmutableArray<T>`) | The record is a real value - the type makes sharing harmless and the mistake impossible |
| Don't put mutable collections in records at all | The real fix. A record advertises value semantics; a `List` member quietly revokes them for every copy, every comparison, and every reader who trusted the keyword |

## 😈 The Even Worse Sibling

Skip `with` entirely and just pass the record around. Two services each hold
"their own" order, both are immutable records, and one of them adds a line item
through the shared list. Now there is no copy operation anywhere in the stack
trace to blame - the aliasing was created at construction time, by handing the
same list to two records, and every reviewer sees two immutable objects.

## 🎓 Advanced Nuance

The same root cause corrupts equality, and it does so in both directions at
once. A record's generated `Equals` compares members with
`EqualityComparer<T>.Default`, which for `List<T>` is reference equality:

```text
same items, two different lists  ->  records are NOT equal
one shared list                  ->  records ARE equal
```

So two records holding identical data compare as different, while two records
that share state - the dangerous pair, the one that will corrupt each other -
compare as the same. Value semantics invert exactly where you need them. That
also means a record with a `List` member is unsafe as a dictionary key or in a
`Distinct()`, for the reasons in
[0013-distinct-that-didnt](../../linq/0013-distinct-that-didnt/) and
[0004-dictionary-key-mutation](../../collections/0004-dictionary-key-mutation/).

`record struct` does not rescue you either: the copy is still member-wise, and a
reference member is still one reference.

## 🔎 How to Find It in Your Codebase

- Every `record` declaration with a `List<>`, `Dictionary<>`, array, or any
  mutable reference member - that member is shared by every copy ever made.
- Grep `with {` and check whether the record has collection members that the
  expression does not replace.
- Snapshot and audit code specifically: if the "before" object shares a
  collection with the "after" object, the snapshot is not a snapshot.
