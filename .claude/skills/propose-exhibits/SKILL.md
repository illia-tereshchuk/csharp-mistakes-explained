---
name: propose-exhibits
description: Generate a menu of candidate exhibits for the curator to choose from, grouped by hall, each with a one-line hook. Use when the curator asks "what next", wants options to pick from, wants to restock the backlog, or wants to see coverage across halls.
---

# Propose exhibits

Produce a menu the curator picks from. This skill only *proposes* - the curator
names the specific cases to build, then the `add-exhibit` skill builds each one.

## Sources and filters

- Pull queued ideas from `.claude/memory/backlog.md`.
- **Exclude everything in `.claude/memory/rejected.md`** - never re-propose a
  declined idea, and pre-filter anything that trips its reason categories
  (predictable finale, can't reproduce, timing-only, CI-would-lie,
  doesn't-happen-in-real-code).
- Apply the curation bar from `CLAUDE.md`: one broken mental model, reject
  predictable finales, prefer accessible + axiomatic, prefer silent-wrongness /
  money stakes.
- Balance against `.claude/memory/archetypes.md` - spread across the 7
  archetypes, don't stack one.

## Format (the full list, shuffled)

Show the **whole** eligible field, never a curated shortlist - every candidate
that survives the filters above belongs in the menu. Mark planned halls (a
candidate there opens the hall). Tag each line with its hall so the shape stays
readable even without grouping. One line per candidate:

```
slug [hall] - the twist (the mechanic, not just the restated title)
```

**Randomize the order on every showing.** Shuffle the candidates freshly each
time rather than sorting or leading with the same favorites, so the curator meets
a different first line each session and no candidate lives permanently at the
bottom. The archetype balance still matters - use it to decide what to *restock*
into the backlog, not to prune what you *show*.

The description is the "wait, WHAT?" - the hook that makes it exhibit-worthy,
not a full explanation. Keep each line scannable; the list is long by design.

## After proposing

Stop and let the curator select. Do not start building. When cases are named,
switch to `add-exhibit` for each. Move chosen ideas out of `backlog.md` as they
ship; add declined ones to `rejected.md` with a reason.
