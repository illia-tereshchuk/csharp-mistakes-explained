# 🔢 numbers

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### remainder-is-not-modulo (A4)

- **Twist:** `%` is remainder, not modulo: a negative hash `% 10` is a
  negative bucket index. And the obvious fix, Math.Abs, throws on
  int.MinValue - the axiom is wrong twice.
- **Mechanic:** C# `%` keeps the sign of the dividend: `-7 % 3 == -1`, never
  2. `GetHashCode()` legitimately returns negatives for about half of all
  values, so `hash % buckets` is negative about half the time.
  `Math.Abs(int.MinValue)` throws OverflowException because +2147483648 does
  not fit in int. The correct form is `(int)((uint)hash % (uint)n)`.
- **Who hits it:** hand-rolled sharding and partitioning - "pick a
  queue/shard/bucket by key.GetHashCode() % N". Works for every key the dev
  tried, crashes (or mis-shards) on the first negative hash in production.
- **Repro:** IMPORTANT for the builder: do NOT use string hashes in the demo -
  string hashing is randomized per process, which would make the demo
  nondeterministic. Use keys whose hash you control: an int id (an int is its
  own hash, so a negative customer id like -12345 gives `-12345 % 10 == -5`)
  or a type with a hardcoded GetHashCode. Index an array with the result:
  IndexOutOfRangeException. No packages.
- **Damage:** crash on the first negative key; with the sloppier "fix"
  (`Math.Abs` or re-hashing), keys silently land in a different shard than
  the one that already holds their data.
- **😈 seed:** `Math.Abs(int.MinValue)` throws - and int.MinValue is a hash
  real values actually have. The crash hides for years behind its 1-in-4-billion
  trigger.
- **Verified:** ran on .NET 10 (2026-07-22): -7%3 == -1, -12345%10 == -5,
  Abs(int.MinValue) threw OverflowException.

### the-widening-that-came-too-late (A4,5)

- **Twist:** `long ms = 30 * 24 * 60 * 60 * 1000;` is *negative* - the
  result type is wide, but the arithmetic already ran in `int` and
  overflowed before the widening; the `long` you reached for to be safe
  never saw the true value.
- **Mechanic:** the operand types decide the arithmetic, not the target.
  `int * int` is an `int` operation; the widening to `long` (or the
  truncation-hiding widening to `double`) happens *after* the result
  already wrapped or truncated. Two faces from one broken belief: `int *
  int -> long` overflows to a garbage (often negative) value, and `int /
  int -> double` does integer division first (`7 / 8 == 0`, `(1+2)/2 ==
  1`) then widens the wrong answer to `0.0`. The fix is to widen one
  operand *before* the operator: `(long)a * b`, `(double)passed / total`.
- **Who hits it:** millisecond/byte/cent computations declared `long` for
  headroom (durations, file sizes, money in minor units) and rate/average
  computations declared `double` - the wide type is chosen deliberately,
  which is exactly what makes the overflow feel impossible.
- **Repro:** `long ms = 30*24*60*60*1000` prints -1702967296;
  `long u = 50000*50000` prints -1794967296; `double rate = 7/8` prints 0;
  each fixed by casting one operand first. Deterministic, no packages.
- **Damage:** a 30-day timeout that computes to a negative duration; a
  success rate that reports 0%; a total-units count that wraps negative -
  all assigned into a variable whose type swears it had room.
- **ADJACENCY (for the curator):** `rejected.md` has `int-overflow-in-cart`
  ("every junior has hit integer overflow - a primer"). This is a
  different mental model: the developer *did* use a wide type and still
  lost, because the conversion is too late - the belief under test is "the
  result type governs the math", not "ints can overflow". Flagged so the
  curator judges whether it clears the same floor.
- **😈 seed:** small inputs make it vanish - `2 days in ms` fits `int`
  fine, so the code ships correct and only the production-scale input
  (30 days, a real file size) crosses 2^31; the test data is exactly the
  range that hides it.
- **Verified:** ran on .NET 10 (2026-07-24): 30-day ms and 50000^2 both
  wrapped negative in `long`; `7/8` and `(1+2)/2` truncated to 0 and 1 in
  `double`; casting one operand first fixed each.

### cast-and-convert-disagree (A4)

- **Twist:** `(int)3.5` is 3 and `Convert.ToInt32(3.5)` is 4 - two
  spellings of "make it an int" that disagree on every non-integer: one
  truncates toward zero, the other rounds (banker's), so the same value
  becomes two different integers depending on which you typed.
- **Mechanic:** a `(int)` cast truncates toward zero (drops the
  fraction); `Convert.ToInt32(double)` and `Math.Round` use banker's
  rounding (to even): 2.5->2, 3.5->4, 2.7->3, -2.7->-3. They agree only on
  whole numbers, so tests built from integer fixtures never catch the
  divergence. No warning distinguishes the two calls.
- **Who hits it:** quantity/score/cents conversions where one code path
  casts and another Converts - a refactor swaps `(int)x` for
  `Convert.ToInt32(x)` (or a mapper picks Convert), and totals shift by
  one on the halves.
- **Repro:** a table over {2.5, 3.5, 2.7, -2.5, -2.7, 1.5}: `(int)` vs
  Convert.ToInt32 vs Math.Round, diverging on every fractional value.
  Deterministic, no packages.
- **Damage:** off-by-one quantities and misfooted totals that depend on
  which conversion helper a given line used - reconciliation finds sums
  that differ by the count of half-values, with no single wrong line.
- **NOTE (curator):** the banker's-rounding half overlaps shipped #0025
  (math-round-banker); the *new* mechanic here is the truncate-vs-round
  divergence between the two conversion spellings, not banker's rounding
  itself. Cross-link #0025; the curator judges whether it stands apart.
- **😈 seed:** the two are perfect synonyms on the happy path - every
  whole-number example in docs, tests, and code review agrees - so the
  divergence is invisible until real fractional data arrives.
- **Verified:** ran on .NET 10 (2026-07-24): (int) truncated,
  Convert.ToInt32 banker's-rounded, diverging on 3.5(3 vs 4), 2.7(2 vs 3),
  -2.7(-2 vs -3).

### float-widened-is-still-float (A4,5)

- **Twist:** `double d = 0.1f;` and d is 0.10000000149011612 - widening a
  float to double does not recover precision it never had, it exposes and
  carries the float's error forward; the "promote to double for accuracy"
  move adds garbage digits instead of removing them.
- **Mechanic:** a float holds ~7 significant digits; widening to double is
  exact *about the float's already-wrong value*, so the low bits print as
  noise (0.1f -> 0.10000000149...). Consequences: `0.1f == 0.1` is false
  (float literal vs double literal never match), and float accumulation
  drifts fast - one million `+= 0.1f` sums to 100958, not 100000, while
  the double loop stays at 100000.0000013. The fix is to keep the value
  double from the source, or round explicitly at the boundary.
- **Who hits it:** sensor/telemetry/graphics/ML pipelines that store
  `float` for size and later widen to `double` "for the math", and any
  running total accumulated in `float`.
- **Repro:** `(double)0.1f` prints the magnified error; `0.1f == 0.1`
  false; a 1e6-iteration `float` sum lands at 100958 vs the `double` sum
  at ~100000. Deterministic, no packages.
- **Damage:** aggregates that drift visibly wrong (a 0.96% error on a
  million-row float sum) and equality checks against double literals that
  never match - both blamed on "floating point" in general while the
  specific cause is the float source nobody widened correctly.
- **ADJACENCY:** shipped #0002 (doubles-for-money) is "don't use binary
  floats for money"; this is narrower and different - widening float to
  double is a lie, and float *accumulation* drifts an order of magnitude
  faster than double. Cross-link, keep distinct.
- **😈 seed:** the accumulation error grows with row count, so the report
  is accurate in dev (hundreds of rows), off by a rounding cent at
  thousands, and visibly wrong at millions - the bug scales in with
  success.
- **Verified:** ran on .NET 10 (2026-07-24): (double)0.1f ==
  0.10000000149011612; 0.1f == 0.1 false; 1e6 float sum 100958.34 vs
  double sum 100000.00000133288.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **Premise correction (verified 2026-07-24):** the old
  `double-to-decimal-carries-error` seed - "casting a double price to
  `decimal` freezes the binary error into the money type" - mostly does
  NOT hold: the double->decimal cast rounds to ~15 significant digits, so
  `(decimal)(0.1+0.2)` prints `0.3` and `(decimal)(19.99*100)` is exactly
  `1999`. The error survives only for values whose double representation
  is wrong beyond 15 sig figs (rare in money). Do not promote as framed;
  if revisited, the honest angle is "decimal-from-double gives false
  confidence at the 16th digit", which needs a real-damage scenario
  first.
