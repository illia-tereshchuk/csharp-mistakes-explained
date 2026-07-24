# 🧪 testing

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### async-void-test-always-passes (A5)

- **Twist:** A failed assertion in an async void test is never observed by
  the runner: the suite is green while the code under test is broken.
- **Mechanic:** an async void method returns at its first await; the runner
  sees a normal return and marks the test passed; the assertion failure
  later surfaces as an unobserved async-void exception (crashing something
  unrelated, or nothing). HONESTY NOTE for the proposer: modern frameworks
  (xUnit v3, NUnit) detect and fail async void *test methods*, so frame the
  who around delegate-based helpers, custom runners, and
  `Assert.ThrowsAsync`-style lambdas passed as `Action` - or present the
  history angle openly and let the curator judge.
- **Who hits it:** teams with homegrown test helpers taking `Action`
  callbacks that someone hands an async lambda; the same shape as
  parallel-foreach-async-lie, in the one place where silent success costs
  the most trust.
- **Repro:** a minimal hand-rolled runner (reflection over methods, no
  framework packages) invoking an async void "test" whose assertion fails
  after `await Task.Yield()`; runner prints PASS; gate with a TCS to show
  the failure arriving after the verdict, deterministically.
- **Damage:** the safety net reports safety it is not providing - the whole
  point of a test suite, inverted.
- **Verified:** async-void semantics verified in batch 1 (parallel-foreach);
  the runner framing to verify at build.

### assert-equal-floats-no-tolerance (A4)

- **Twist:** `Assert.Equal(0.3, 0.1 + 0.2)` fails - correct production
  math reported as a bug - and the failure message rubs it in by printing
  "Expected: 0.29999999999999999": even the test's own 0.3 literal was
  never 0.3.
- **Mechanic:** the two-argument double overload compares exactly, and
  0.1 + 0.2 is 0.30000000000000004. The fix overloads differ in kind:
  `tolerance:` is an epsilon, `precision:` *rounds both sides* to N
  digits first - related but not interchangeable semantics (both verified
  passing). Decimal literals compare exact (0.1m + 0.2m == 0.3m), which
  trains people to expect the same from double.
- **Who hits it:** any test asserting computed doubles - totals,
  averages, percentages. The observed "fix" in the wild: hardcode
  0.30000000000000004 as the expected value, baking the rounding artifact
  into the spec.
- **Repro:** xunit.assert in a console file
  (`#:package xunit.assert@2.9.3`, `#:property PublishAot=false`): catch
  EqualException on the exact overload, then show tolerance, precision,
  and decimal all green. Deterministic.
- **Damage:** both directions hurt: red tests over correct code get the
  suite labeled flaky (it's deterministic, but nobody checks), and the
  hardcoded-artifact "fix" makes a legitimate refactor - reordering the
  additions - break the "specification".
- **😈 seed:** the framework confesses in the failure text - both numbers
  printed at full precision show the assertion was unwinnable as
  written - in output nobody reads past "Values differ".
- **Verified:** ran on .NET 10 (2026-07-24), xunit.assert 2.9.3: exact
  overload failed with the message above; tolerance 1e-12 and
  precision 15 passed; decimal passed.

### collection-assert-is-ordered (A4)

- **Twist:** Assert.Equal on two collections with the same members fails
  because the order differs - and whether "equal" even *means* ordered
  depends on the runtime type: the same items in HashSets pass the same
  assertion.
- **Mechanic:** Assert.Equal over sequences compares element-by-element
  in order; set types are special-cased to set semantics (verified:
  arrays fail, HashSets of the same items pass). Assert.Equivalent is
  the order-insensitive spelling. The sibling trap: Assert.Equal on two
  field-identical DTOs without Equals fails on reference equality while
  Equivalent passes - "equal" in one assert library is several different
  relations, selected by types.
- **Who hits it:** asserting results of GroupBy, Dictionary iteration,
  parallel processing, or SQL without ORDER BY - membership correct,
  order incidental - red the day a runtime upgrade or hash seed shuffles
  iteration order.
- **Repro:** three strings as arrays vs as HashSets through Assert.Equal
  (fail / pass), Equivalent (pass), plus the identical-DTO pair
  (fail / pass). xunit.assert in a console file, deterministic.
- **Damage:** red builds after infrastructure-only changes - and the
  observed fix is adding OrderBy to *production* code, a sort nobody
  asked for, to appease a test that asserted more than the requirement.
- **😈 seed:** a type refactor silently weakens the suite: change the
  production return from List to HashSet and the same assertion flips
  from ordered to set-wise - still green, now checking less.
- **Verified:** ran on .NET 10 (2026-07-24), xunit.assert 2.9.3: arrays
  failed, HashSets passed, Equivalent passed both, identical DTOs failed
  Equal and passed Equivalent.

### static-state-leaks-between-tests (A6,5)

- **Twist:** the suite is green; run the same two tests in the other
  order and it's red - a static field written by one test is still there
  for the next, and the failure lands on the *innocent* test.
- **Mechanic:** statics are process-wide and runners execute many tests
  per process, so any static write without teardown becomes an input to
  whoever runs later. Order dependency is the deterministic face
  (A,B green; B,A red - both orders verified in one run); under
  parallel-by-default runners the same leak wears a schedule and reads
  as flakiness.
- **Who hits it:** static config knobs, caches, service locators touched
  by tests - stable for months until a new test, a rename, or a runner
  upgrade reshuffles discovery order.
- **Repro:** a mini-runner executes {A: expects clean state,
  B: sets a static surcharge and asserts it} in both orders in one
  process: A,B both pass; B,A leaves A failing. Deterministic, no
  packages - the mechanic belongs to the language, not a framework.
- **Damage:** the failure attaches to the wrong test: A fails, B leaked -
  so the investigation, quarantine, or deletion hits the innocent one
  and the leak survives its own cleanup.
- **😈 seed:** refactors that touch zero logic - renaming a class,
  adding an unrelated test - reshuffle execution order and "break the
  build": git blame pins it on the rename, which is technically true and
  completely wrong.
- **Verified:** ran on .NET 10 (2026-07-24): order A,B green; order B,A
  left A failing on the leaked surcharge.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **Dead end (verified 2026-07-24, do not re-derive):** "Assert.Throws
  with an async lambda silently passes" - not in xunit 2.9:
  Assert.Throws&lt;T&gt;(Func&lt;Task&gt;) is marked [Obsolete] as a
  compile *error* (CS0619 pointing at ThrowsAsync), so both
  `() => FailAsync()` and `async () => ...` shapes fail the build. The
  surviving flavor of the trap is the Action-bound async-void shape
  already covered by async-void-test-always-passes.
