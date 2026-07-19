# C# Mistakes Explained - project guide

A museum of small, runnable C# bad-practice exhibits. Each teaches one broken
mental model: a `Bad.cs` that fails when you run it, a mirror `Good.cs` that
does the same job right, and a `README.md` that explains the mechanic.

## How we work

- **I generate a full exhibit** (Bad.cs + Good.cs + README.md + the front-page
  row + calibration updates), **verify it by running both files**, and leave it
  uncommitted. The curator reads, tests, and commits - or tells me to commit.
- Detailed conventions load automatically when I edit exhibit files (see
  `.claude/rules/`). The end-to-end workflow lives in the `add-exhibit` skill.

## Always-on conventions

- **English** for everything committed (code, comments, docs, commit messages).
  Ukrainian stays in chat.
- **Hyphens only** - never em/en dashes, anywhere.
- **Solo project.** No external-contributor scaffolding (no CONTRIBUTING.md,
  issue templates, "PRs welcome"). Curated by one person.
- **Commits:** imperative subject <=50 chars, no trailing period; body explains
  *why*; one logical change per commit; trailer
  `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`. Exhibit commits are
  `Add exhibit #NNNN: <slug>`.

## Curation bar (when proposing exhibits)

The curator's taste is **subjective and case-by-case** - it lives in his
individual picks and rejections, not in categories. The `reject-exhibit` skill
and `.claude/memory/rejected.md` are the main channel for learning it: study
which specific cases he declines and why, and pre-filter future proposals
accordingly. The bar below is the current distillation.

- **One exhibit = one broken mental model.** Not a typo, not a whole
  architecture review. If explaining it needs two different "why"s, it's two
  exhibits.
- **Reject predictable finales.** If the reader guesses the outcome from the
  title, it's a listicle item, not an exhibit. An exhibit needs a mechanic
  twist - a "wait, WHAT?" even for someone who knows the bug.
- **Mind the floor and the ceiling.** This is a *popular* encyclopedia for
  marketing and self-teaching, not senior deep-weeds. Below the floor: primer-
  level bugs everyone knows from day one (integer overflow, null reference).
  Above the ceiling: esoteric internals only seniors chase for big money
  (lock-free memory barriers, IL rewriting, GC tuning). Aim for the middle band.
- **Prefer accessible + axiomatic:** everyday contracts everyone touches, with a
  rule that reads like an axiom (`never write async void outside event
  handlers`). Favor these over exotic API footguns.
- **A bug must answer who / where / how.** If you can't say who hits it, from
  where, and how, it's a vacuum example - not reproducible in any real context,
  so not interesting (this is why path-combine-betrayal was cut).
- **Prefer silent-wrongness and money/audit stakes** - they screenshot well and
  land emotionally.

## Tools (run from repo root)

- `dotnet run tools/next-id.cs` - next free exhibit number; fails on duplicates.
- `dotnet run tools/check-links.cs` - cross-references are links, links resolve.

## Memory

Committed project memory lives in `.claude/memory/`, indexed by `MEMORY.md`,
which is imported below so its index loads every session. Read `rejected.md`
before proposing; the topic files are read on demand. Memory updates are
committed **separately** from exhibit commits.

@.claude/memory/MEMORY.md
