# The 7 archetypes

The taxonomy that emerged at ~25 candidates. Not halls (halls = tech area);
archetypes = the SHAPE of the wrong mental model. Use for curation balance,
tag design, and the future cross-index. Each exhibit maps to 1-2. The examples
below are illustrative, not exhaustive (34 exhibits now) - shipped ids first,
then a live candidate; rejected ideas never appear here.

| # | Archetype | The broken belief | Examples (shipped ids; candidate) |
|--:|-----------|-------------------|------------------------------|
| 1 | **Time gap** | capture/defer now, execute later - value read at the wrong moment | 0006, 0007, 0018, 0019, 0031; async the-cached-failure |
| 2 | **Broken identity contract** | equality / hash / interning stays stable while I mutate | 0001, 0004, 0013, 0023; boxing boxed-values-are-equal-not-same |
| 3 | **Hidden copy** | I mutated the thing; I mutated a copy | 0009, 0011, 0028; value-types the-vanishing-mutation |
| 4 | **Same name, different fate** | identical syntax = identical behavior | 0002, 0024, 0025, 0027, 0030, 0032; pattern-matching boxed-five-is-not-five |
| 5 | **Silent wrongness** | it threw, so I'd know / it didn't throw, so it's fine | pervasive; the fear ladder crash < wrong < *silently* wrong. Most 😈 sections. |
| 6 | **Environment as hidden input** | GC / threadpool / culture / timezone are constant | 0003, 0010, 0020, 0022; async the-collected-timer |
| 7 | **Discarded return** | the method mutated in place (it returned instead) | 0005; async trywrite-drops-silently, strings string methods |

## Why this matters at scale

- Halls stop scaling around dozens: `the-collected-timer` is async OR memory; `interpolated-injection` is security OR orm. **Tags + archetype are the real navigation**, halls are just the shelf.
- Balance target: don't stack one archetype. Track the mix; every batch should spread across archetypes.
- Archetype 5 (silent wrongness) is the house style, usually the 😈 section rather than a standalone exhibit.

## Fear ladder (recurring rhetorical device)

crash (loud, honest) < wrong result (visible) < **silently** wrong (invisible for months).
The 😈 section almost always pushes the exhibit one rung down this ladder, then
lands: "the crash in this exhibit is the *lucky* outcome."
