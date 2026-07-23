---
id: "0032"
title: Interpolating a log message instead of using a template
category: logging
tags: [logging, ILogger, structured-logging, string-interpolation]
author: helga-pawlowska
rule: "never log a bare **string** - pass a template so every value becomes a named field"
---

# #0032 - Interpolating a log message instead of using a template

## 💥 Symptom

A customer says every payment is being declined, and you need their history
now. You open the log sink and search by their id - `CustomerId:
4087` - and get nothing. You can *see* the line in the raw stream:
`Payment declined for customer 4087, amount 149.99`. It is right there. But the
query returns zero matches, so the dashboard, the alert rule, and the "all
failures for this customer" panel are all empty. The text was logged; the field
was not.

## 🔍 The Offending Code

```csharp
sink.LogWarning($"Payment declined for customer {customerId}, amount {amount}");
```

It looks like structured logging. It is not.

## 🧠 What's Actually Going On

String interpolation runs at the call site, *before* the logger is involved.
The `$"..."` is evaluated by your code into one finished `string`, and that
string is all `LogWarning` ever receives. The logger has nothing left to take
apart: no `customerId` argument, no field name, just characters.

A message template is the opposite deal. You hand the logger the *unrendered*
template and the values separately:

```csharp
sink.LogWarning("Payment declined for customer {CustomerId}, amount {Amount}",
    customerId, amount);
```

Now the logger parses `{CustomerId}` and `{Amount}` itself and pairs them with
the arguments, producing structured state - `CustomerId = 4087`,
`Amount = 149.99` - that the sink indexes as queryable fields. Both spellings
render the *same* human-readable line; only the template gives the sink
something to search by. The two are one `$` apart.

There is a second, quieter cost. Every interpolated line is a unique string
(`...customer 4087...`, `...customer 5120...`), so the sink cannot group them.
The template version carries a stable message id (`{OriginalFormat}` =
`"Payment declined for customer {CustomerId}, amount {Amount}"`), so "how many
payment declines today" is one grouped query instead of a substring guess.

## ✅ The Fix

Use a message template and pass the values as arguments. Full version in
[Good.cs](Good.cs). Placeholder names are `{PascalCase}` field names, not
`{0}` positions - the name becomes the key you search by.

| Approach | When it's the right call |
|---|---|
| Message template with `{Named}` placeholders | Always, for anything a logger receives. |
| String interpolation `$"..."` | Only for strings that never touch a logger - `ToString()`, exception messages you construct yourself, UI text. |

## 😈 The Even Worse Sibling

Placeholders bind by **position, not name**. Swap the arguments -
`"{CustomerId} declined {Amount}"` called with `(amount, customerId)` - and the
sink faithfully records `CustomerId = 149.99` and `Amount = 4087`. Nothing
crashes, nothing warns; the incident search now returns a *wrong* row with total
confidence. The crash in this exhibit is the *lucky* outcome - it at least tells
you the field is missing.

## 🎓 Advanced Nuance

The interpolated call is not always as harmless as "just a plain string." Since
C# 10, `LoggerExtensions` overloads bind the interpolated argument to a
`LogValuesFormatter`-style path in some libraries, but the built-in
`Microsoft.Extensions.Logging` overloads still take `(string message,
params object[] args)` - so `$"..."` collapses to a pre-rendered message with an
empty args array and **no** captured fields. The analyzer that catches this is
CA2254 ("template should be a static expression"): it *does* fire here, but
ships as a suggestion, so most builds never surface it. One green build at a
time, a codebase quietly loses the ability to query its own logs.

## 🔎 How to Find It in Your Codebase

- **Grep:** `Log(Trace|Debug|Information|Warning|Error|Critical)\(\$"` - any
  logger call whose first argument is an interpolated string is the bug.
- **Analyzer:** enable **CA2254** as a warning (not suggestion) in
  `.editorconfig`: `dotnet_diagnostic.CA2254.severity = warning`. It flags every
  non-constant log template, including interpolation.
- **Also watch** [0010-immortal-subscriber](../../events/0010-immortal-subscriber/)-style
  "it compiled, so it's wired up" reasoning: like an event that silently never
  fires, an interpolated log silently never indexes.
- **Code review tell:** a `{` immediately after `Log...("` is a template; a `$`
  immediately after `Log...(` is not.
