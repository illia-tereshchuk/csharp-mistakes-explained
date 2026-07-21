# Backlog

Candidate exhibits the curator picks from. `A` = archetype (see
`archetypes.md`). `propose-exhibits` turns this into a hall-grouped menu; the
curator selects. Ids are assigned at build time. No difficulty levels - every
mistake is equally able to take down prod.

Everything here has already passed the filters: not primer-level, not a vacuum
scenario, reproducible deterministically in a single console file, not proven by
timing, not dependent on an unpinned machine environment.

## Opened halls

### collections
| slug | A | the twist |
|------|:--:|---|
| sort-is-unstable | 4 | List.Sort silently reorders equal elements, so a "sort by date, then by name" two-pass gives an order neither pass asked for; OrderBy is stable, Sort is not. |
| dictionary-order-illusion | 6 | Enumeration order looks like insertion order until one Remove; the next Add reuses the freed slot and the sequence rearranges. |
| removeat-in-forward-loop | 5 | A delete done forward: RemoveAt inside a `for` shifts every later index down one, so the loop skips the element that slid into the freed slot - and unlike foreach it never throws. |
| getvalueordefault-hides-missing | 4,5 | `dict.GetValueOrDefault(sku)` returns `default(decimal)` for an absent key - a real 0.00 and "not priced" are the same value, so the order ships free with nothing thrown. |

### numbers
| slug | A | the twist |
|------|:--:|---|
| negative-modulo-sign | 4,5 | `-7 % 3` is `-1`, not `2` (the sign follows the dividend), so `x % 2 == 1` never matches a negative odd number and a hash-into-buckets step drops every negative key into the wrong lane. |
| abs-of-minvalue-throws | 5 | `Math.Abs(int.MinValue)` throws OverflowException: the positive counterpart doesn't fit in an int, so the one call everyone trusts to "just make it positive" has a hole at a single input. |
| double-to-decimal-carries-error | 4 | Reading a price as `double` then casting to `decimal` freezes the binary rounding error into the money type - the `decimal` is exact about a number that was already wrong. |

### async
| slug | A | the twist |
|------|:--:|---|
| the-collected-timer | 6 | A Timer with no stored reference gets collected mid-run; the "every minute" job stops with no error. |
| semaphore-never-released | 5 | An exception between Wait and Release leaks a permit forever; the next caller waits on a semaphore nobody will free. |
| startnew-async-does-not-unwrap | 1,5 | `Task.Factory.StartNew(async ...)` hands back a `Task<Task>`; awaiting the outer one returns the instant the inner work *starts*, so the "done" you awaited is a lie (Task.Run unwraps, StartNew does not). |
| threadstatic-lost-across-await | 6,1 | A `[ThreadStatic]` "current user" set before `await` is gone after it: the continuation resumes on a different pool thread and the ambient value is now someone else's - this is what AsyncLocal exists for. |
| parallel-foreach-swallows-async | 1,5 | An `async` lambda handed to `Parallel.ForEach` binds as `async void`: the loop returns before the bodies finish and any exception inside them is lost with nowhere to surface. |

### linq
| slug | A | the twist |
|------|:--:|---|
| average-of-empty-throws | 5 | `.Sum()` of an empty sequence is a peaceful 0, but `.Average()` of the same empty sequence throws InvalidOperationException - the "no reviews yet" product crashes the ratings page. |
| zip-drops-the-tail | 5 | `Zip` stops at the shorter sequence: pair 100 ids with 99 names and you silently get 99 rows, no error, the last record simply gone. |
| distinctby-keeps-the-wrong-one | 5 | `DistinctBy(x => x.Id)` keeps the *first* row per key; feed it events oldest-first and every duplicate resolves to the stale version, not the latest one you meant to keep. |

### events
| slug | A | the twist |
|------|:--:|---|
| one-handler-kills-the-rest | 5 | A subscriber that throws stops the invocation list; everyone registered after it never runs, and the publisher never knows. |
| invoke-race-on-null-check | 1 | `if (E != null) E(...)`: the last subscriber unsubscribes between the check and the call - NRE from an event that "cannot be null". |
| static-event-pins-every-subscriber | 6 | A `static` event is a permanent GC root: every object that ever subscribed is held for the life of the process, so short-lived handlers accumulate forever even though the publisher looks stateless. |

### value-types
| slug | A | the twist |
|------|:--:|---|
| the-vanishing-mutation | 3 | Mutating a struct taken from a List edits a copy; the same code on an array works, so nobody suspects the collection. |
| default-struct-skips-constructor | 3,5 | `default(Money)` and `new Money[10]` never run your constructor, so the currency-code invariant it enforces is simply absent: a zero-filled struct that no code path could have created legally. |

### exceptions
| slug | A | the twist |
|------|:--:|---|
| activator-hides-the-real-error | 5 | Reflection wraps the constructor's exception in TargetInvocationException, so the catch for the real type never fires. |
| poisoned-static-constructor | 5,6 | A static constructor that throws once poisons the type forever: every later access - even after the transient cause is gone - throws TypeInitializationException wrapping the original, for the life of the process. |
| aggregateexception-hides-the-type | 5 | Blocking on a task with `.Result`/`.Wait()` wraps the real failure in AggregateException, so your `catch (TimeoutException)` never fires - the exception you handle isn't the one that was thrown. |
| exception-filter-runs-anyway | 5 | A `catch (...) when (Audit(e))` filter runs *before* the stack unwinds and runs even when it returns false and the catch is skipped - side effects in the guard fire on exceptions you never handled. |

### orm
| slug | A | the twist |
|------|:--:|---|
| stale-tracked-entity | 5 | The change tracker returns the entity it cached earlier, so a fresh query hands back data that is already out of date. |
| untranslatable-where | 4 | A helper method inside Where cannot become SQL; EF stops the query dead rather than quietly pulling the whole table. |
| savechanges-without-transaction | 5 | Two `SaveChanges` calls in one method aren't one unit of work: the first commits, the second throws, and the database is left in the half-written state the code assumed was impossible. |

### serialization
| slug | A | the twist |
|------|:--:|---|
| enum-json-is-a-number | 4,5 | System.Text.Json writes enums as their integer by default; insert a member in the middle and every stored document silently remaps to the next enum name - yesterday's "Shipped" is today's "Cancelled". |
| json-cycle-throws | 5 | A parent that references its children which reference the parent serializes fine right up until it throws JsonException at runtime: the default serializer has no cycle handling, and the object graph you log every day is the one that finally loops. |
| json-case-sensitive-by-default | 4,5 | System.Text.Json matches property names case-sensitively (Newtonsoft did not); one `"userId"` vs `"UserId"` and the field stays `default` with nothing logged - a migration that "changed only the library" drops data. |

### di-lifetimes
| slug | A | the twist |
|------|:--:|---|
| the-silent-override | 4 | Two registrations of one interface are legal; the last wins, and the handler you debugged for an hour never resolved. |
| scoped-from-root-lives-forever | 6,5 | Resolving a scoped service straight from the root `IServiceProvider` gives it singleton lifetime by accident: it's never disposed and its per-request state leaks across every request, the mirror image of the captive dependency. |

### datetime
| slug | A | the twist |
|------|:--:|---|
| kind-blind-equality | 4 | 14:00 UTC equals 14:00 Local, because `==` compares ticks and ignores Kind. |
| the-25-hour-day | 6 | AddHours(24) is not "tomorrow, same time" across DST; the daily job fires an hour off, twice a year. |
| ambiguous-date-parse | 4 | The exact string "02/03/2026" parses to two different real dates under two explicitly-set cultures (US vs UK) with no error either way - `DateTime.Parse` of an unpinned format is a coin flip the code never sees land. |

### disposal
| slug | A | the twist |
|------|:--:|---|
| using-var-disposes-late | 6,1 | `using var conn = Open();` doesn't dispose at the next blank line - it disposes at the end of the whole method, so the connection you "closed" stays open across everything below it, and a mid-method check proves it. |
| double-dispose-crashes | 5 | A `using` block plus one explicit `Dispose()` calls Dispose twice; an implementation that isn't idempotent throws ObjectDisposedException on the second call, turning tidy cleanup into a crash. |
| read-before-flush-loses-data | 5 | A StreamWriter buffers; read the underlying stream before the writer is flushed or disposed and you get an empty or truncated payload - the bytes exist only in a buffer the reader can't see. |

### equality
| slug | A | the twist |
|------|:--:|---|
| equals-without-gethashcode | 2 | Override `Equals` but not `GetHashCode` and the object works perfectly in a `List.Contains` yet goes missing in a `HashSet` or dictionary: two "equal" instances land in different buckets and neither can find the other. |
| nan-equals-disagrees-with-operator | 4,2 | `NaN == NaN` is false but `NaN.Equals(NaN)` is true, so `List.Contains(NaN)` finds it while `Any(x => x == NaN)` never will - the collection's equality and the `==` operator flatly disagree on the same two values. |

### records
| slug | A | the twist |
|------|:--:|---|
| record-struct-is-mutable | 3,4 | `record struct Point(int X, int Y)` has settable properties by default - the immutability people assume from "record" applies to record *classes*; the struct version quietly lets a shared copy be edited under you. |
| with-skips-validation | 5 | Validation you put in a record's constructor body doesn't run on `with`: the copy uses the compiler's copy constructor, so an "impossible" invalid state is one `with { ... }` away from existing. |
| record-tostring-leaks-fields | 5 | A record's generated `ToString` prints every property, so the moment a `Password` or `Token` member joins the record it starts showing up verbatim in every log line that interpolates the object. |

## Planned halls (a candidate here opens the hall)

| hall | slug | A | the twist |
|------|------|:--:|---|
| nullability | null-forgiving-lies | 5 | `!` silences the compiler and changes nothing at runtime: a promise you made, not a check you performed. |
| nullability | default-of-t-is-null | 5,6 | A generic `T Get<T>()` that returns `default` hands back `null` for every reference `T` despite the non-nullable annotation: `default` ignores the `?`, so the "never null" contract is a compile-time fiction the runtime never signed. |
| generics | static-field-per-closed-type | 6 | A static field in `Cache<T>` is separate per T, so the "global" cache silently splits into many. |
| generics | generic-static-ctor-runs-per-type | 6 | A `static` constructor in `Registry<T>` that "runs once" actually runs once *per closed type*, so a one-time global initialization silently executes again for every distinct `T`. |
| enums | enum-accepts-undefined | 5 | A cast or Enum.Parse produces a value not in the enum, and every switch on it falls to default. |
| enums | hasflag-zero-always-true | 5 | `permissions.HasFlag(Permission.None)` is always true because every set contains the zero flag, so the guard meant to detect "no permissions" passes for everyone. |
| enums | flags-not-powers-of-two | 4,5 | A `[Flags]` enum numbered 1, 2, 3, 4 instead of 1, 2, 4, 8 overlaps bits: `Read \| Write` equals `Delete`, and `HasFlag` starts answering yes to permissions nobody granted. |
| enums | enum-default-is-zero | 5 | `default(Status)` is 0 whatever you named it; if `Active` happens to be the first member, every uninitialized DTO and struct field arrives already "Active" without anyone setting it. |
| enums | enum-reorder-shifts-values | 4 | Enum members persist to the database as their ordinal int; insert one in the middle and every stored row shifts meaning by one - the historical data now decodes to the wrong member with nothing to flag it. |
| inheritance | virtual-call-in-constructor | 1 | The base constructor calls an override that runs before the derived fields exist; the object sees its own state as null. |
| inheritance | new-hides-does-not-override | 4 | A method marked `new` hides rather than overrides, so the *same object* runs the base method through a base reference and the derived method through a derived reference - behavior depends on the variable's type, not the object's. |
| inheritance | default-arg-from-static-type | 4 | Default parameter values are baked in by the compiler from the *declared* type, so overriding a virtual method with a different default and calling it through a base reference uses the base's default against the derived body. |
| pattern-matching | switch-expression-not-exhaustive | 5 | One added enum member turns a compile-time warning into a runtime exception in a switch that "covered everything". |
| pattern-matching | earlier-pattern-shadows-later | 5 | A broad `when` guard or base-type case placed first swallows everything, leaving the specific arm below it unreachable - no error, the specialized branch just never runs. |
| pattern-matching | type-pattern-skips-null | 5 | `case string s:` does not match `null` (null is not an instance of anything), so a null value slips past the branch that "handles strings" and lands in the default arm you meant for other types. |
| boxing | mutating-a-boxed-struct | 3 | Calling a mutating method through an interface changes the box, not your variable. |
| boxing | unbox-must-match-exact-type | 4,5 | `(int)(object)42L` throws InvalidCastException: you can only unbox to the *exact* boxed type, not a convertible one, so reading a `long` out of an `object` column as `int` fails at runtime with values that would convert fine. |
| boxing | boxed-values-are-equal-not-same | 2 | Box the same `int` twice and `Equals` says equal while `ReferenceEquals` says no - each box is a fresh heap object, so identity-based caching or locking on boxed values silently treats one value as many. |
| memory | the-closure-that-held-everything | 6 | A lambda that captured one small variable keeps the whole captured state alive, big array included. |
| memory | finalizer-delays-gc | 6 | An object with a finalizer survives the collection that should have freed it - it goes on the finalization queue and needs a *second* GC - so "unreachable" and "collected" are one full cycle apart, provable with a WeakReference. |
| http | baseaddress-eats-your-path | 4 | A BaseAddress without a trailing slash silently drops its last segment: every `/v1/users` call goes to `/users`. Pure Uri math, no server. |
| http | leading-slash-ignores-baseaddress | 4 | A request path that starts with "/" throws away the BaseAddress path entirely: with base `.../v1`, `GetAsync("/users")` goes to `/users`, not `/v1/users` - the leading slash silently means "from the host root". |
| http | no-ensuresuccess-reads-error-body | 5 | `GetAsync` does not throw on 404 or 500; without `EnsureSuccessStatusCode` the code reads the error page's body as if it were the payload and deserializes an error into a "successful" empty object. |
| http | timeout-looks-like-cancellation | 4,5 | HttpClient's own timeout surfaces as TaskCanceledException, the same type a user cancel throws, so `catch (OperationCanceledException)` treats a server timeout as "user changed their mind" and skips the retry it needed. |
| configuration | binding-fails-silently | 5 | One typo'd key and the setting is the default; nothing throws, nothing logs, the feature is "off in prod only". |
| configuration | config-array-gap-truncates | 5 | Bind a config array whose indices skip a number (0, 1, 3) and the binder stops at the gap: index 3 is silently dropped, so the list is short with nothing logged. |
| configuration | ioptions-does-not-reload | 6 | `IOptions<T>` is a singleton snapshot taken once at first resolve; edit the config file at runtime and it never changes - the "hot reload" you tested with IOptionsMonitor isn't there for plain IOptions. |
| logging | interpolated-log-loses-everything | 4 | `$"..."` formats before the logger sees it, so the structured fields you search by never exist. |
| logging | log-args-evaluated-when-disabled | 5 | `logger.LogDebug("state {S}", ExpensiveDump())` still calls `ExpensiveDump()` even when Debug logging is off - the level check happens inside the logger, after your argument was already computed. |
| logging | log-args-bind-by-position | 4,5 | Structured log placeholders bind to arguments by position, not by name, so `"{User} did {Action}"` called with `(action, user)` records them swapped - every field is captured, just under the wrong key. |
| regex | missing-anchors-pass-anything | 5 | A "digits only" check without anchors accepts abc123def, because IsMatch looks for a match anywhere. |
| regex | dot-misses-newline | 5 | `.` doesn't match `\n` by default, so a `^\w+$`-style validator that looks clean passes an input whose second line is an attack payload - the check only ever saw the first line. |
| regex | unescaped-regex-input | 4,5 | Building a pattern from user text without `Regex.Escape` turns their `.` into "any char" and their `(` into a syntax error: a search box becomes either a match-everything filter or a runtime ArgumentException. |
| regex | slash-d-matches-unicode-digits | 5 | `\d` matches every Unicode decimal digit, not just 0-9, so a "digits only" check accepts Arabic-Indic or fullwidth numerals that then blow up `int.Parse` two layers downstream. |
| testing | async-void-test-always-passes | 5 | A failed assertion in an async void test is never observed: the suite stays green while the code is broken. |
| testing | static-state-leaks-between-tests | 6,5 | A static field mutated by one test is still there for the next: the suite goes green run in one order and red in another, and the culprit is shared state no test declared it owned. |
| testing | assert-equal-floats-no-tolerance | 4 | `Assert.Equal(0.3, 0.1 + 0.2)` fails: the two-argument overload compares doubles exactly, so a correct calculation reports as a test failure until someone adds the precision argument nobody remembered exists. |
| testing | collection-assert-is-ordered | 4 | `Assert.Equal` on two collections is order-sensitive; two sets with the same members in a different order report as not equal, failing a test whose code is perfectly correct. |
| io | readalltext-guesses-encoding | 5 | Reading a file written in another encoding mangles the text instead of failing, and the corruption ships downstream. |
| io | utf8-bom-breaks-parser | 5 | Writing text as UTF-8-with-BOM prepends three invisible bytes; the JSON or CSV reader downstream treats them as part of the first field or key, and the parse fails on a file that looks identical in every editor. |
| io | read-without-seeking-to-start | 5 | Write to a stream, then read it back without seeking to position 0, and you read nothing: the cursor is already at the end, so the "round trip" returns empty with no error. |
| strings | length-lies-about-emoji | 4 | `"👍".Length` is 2, so a 50-character truncate cuts an emoji in half and sends a replacement char to production. |
| strings | mojibake-factory | 4 | Text encoded twice comes back as "ÐŸÑ€Ð¸Ð²Ñ–Ñ‚" - readable proof of what a double round-trip does to bytes. |
| strings | trim-is-a-charset-not-a-prefix | 4,5 | `url.TrimStart("https://".ToCharArray())` doesn't strip a prefix - it strips any leading character in that *set*, so a URL beginning with an 's', 't', 'p', or '/' gets eaten letter by letter into garbage. |
| strings | unnormalized-strings-not-equal | 2,4 | "é" written as one code point and as "e" plus a combining accent look identical and print identical, but ordinal `==` says they differ - a username or coupon uniqueness check waves through a duplicate it can't see. |
| strings | split-keeps-empty-entries | 5 | `"a,,b".Split(',')` returns three items, not two: the empty middle field is kept by default, so a positional parser reads every column after the blank one shifted by one. |
| strings | interpolation-is-culture-sensitive | 4 | `$"{amount}"` formats with the current culture, so the same code builds `1,50` or `1.50` depending on the machine - shown here by pinning two cultures explicitly - and the CSV or SQL you generated is malformed in exactly one region. |
| strings | grapheme-cluster-splits | 4 | A family emoji (👨‍👩‍👧‍👦) is one glyph but many code points joined by zero-width joiners; even a rune-aware truncate cuts it apart, because only grapheme (`StringInfo`) boundaries match what the user sees as one character. |
| security | interpolated-injection | 4 | The same `$"...{name}..."` is safe in FromSqlInterpolated and an injection in FromSqlRaw: identical syntax, opposite fate. |
| security | guessable-random | 6 | `Random` for password-reset tokens: seed it the same way and you reproduce someone else's token. |
| security | unsalted-hash-reveals-duplicates | 5 | Hashing passwords with a bare SHA-256 and no salt makes identical passwords produce identical hashes, so one glance at the leaked table shows which accounts share a password - the hash hid the value but not the collisions. |
| reflection | gettype-is-exact-not-assignable | 4,5 | `obj.GetType() == typeof(Base)` is false for a derived instance, so a type-keyed handler dictionary built on `GetType()` misses every subclass and quietly falls through to the default. |
| reflection | getproperty-misses-nonpublic | 5 | `Type.GetProperty("x")` returns null for a non-public or static property because the default binding flags exclude them, so the reflective mapper skips fields it was written to copy and never says so. |
| reflection | activator-needs-parameterless-ctor | 5 | `Activator.CreateInstance<T>()` compiles for any `T` but throws MissingMethodException at runtime the moment `T` lacks a public parameterless constructor - a factory that "works for everything" until the first type that doesn't. |

## Seeds (brainstorm before proposing)

- **exceptions:** rethrow across an await boundary (the stack is already rebuilt) · `using` swallowing the body's exception when Dispose also throws.
- **linq / collections:** GroupBy on reference-equality keys · OrderBy with a non-deterministic key · Single vs First surprising on duplicate data.
- **async (hard to make deterministic - vet before promoting):** ConcurrentDictionary.GetOrAdd running its factory more than once under contention · a ValueTask awaited twice · Task.WhenAny leaving the losing tasks' exceptions unobserved · a shared `List<T>` written from several tasks inside Task.WhenAll.
- **io / memory (timing- or race-shaped - promote only with a hard assertion):** `File.Exists` then `File.Open` TOCTOU · a WeakReference checked then used after a collection · `Directory.GetFiles` order not being guaranteed.
- **reflection, equality, memory:** halls still thin - restock further when their first exhibit lands.
