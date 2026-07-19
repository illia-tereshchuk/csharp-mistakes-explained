# Backlog

Candidate exhibits the curator can pick from. `A` = archetype (see
`archetypes.md`). The curator selects; `propose-exhibits` turns this into a
hall-grouped menu. Ids are assigned at build time. No difficulty levels - every
mistake is equally able to take down prod.

## Pool

### async
| slug | A | the twist |
|------|:--:|---|
| the-collected-timer | 6 | Timer with no stored ref gets GC'd; force GC.Collect, ticks stop. |
| lock-on-a-string | 2 | `lock("cache")` in 2 classes = same interned object. Prove via ReferenceEquals. |

### datetime
| slug | A | the twist |
|------|:--:|---|
| kind-blind-equality | 4 | 14:00 UTC == 14:00 Local; `==` compares ticks, ignores Kind. |
| the-25-hour-day | 6 | AddHours(24) != "tomorrow same time" across DST. Pin TimeZoneInfo or CI lies. |

### strings (planned hall)
| slug | A | the twist |
|------|:--:|---|
| length-lies-about-emoji | 4 | "👍".Length==2; a 50-char truncate splits a grapheme, � in the push. StringInfo. |
| mojibake-factory | 4 | double-encode: "Привіт" -> "ÐŸÑ€Ð¸Ð²Ñ–Ñ‚". simple but byte-level finale. |

### di-lifetimes
| slug | A | the twist |
|------|:--:|---|
| the-silent-override | 4 | two registrations of one iface; last wins silently. |

### value-types
| slug | A | the twist |
|------|:--:|---|
| the-vanishing-mutation | 3 | mutate a struct from a collection -> mutates a copy; array works, List doesn't. Sibling of [0011-defensive-copy-ambush](../src/value-types/0011-defensive-copy-ambush/). |

### security (planned hall)
| slug | A | the twist |
|------|:--:|---|
| interpolated-injection | 4 | same `$"...{x}"` safe in FromSqlInterpolated, injection in FromSqlRaw. |
| guessable-random | 6 | `Random` for reset tokens; seed predictable, token reproducible. |

## Seeds (brainstorm before proposing)

- **exceptions:** exception filters (`when`) side effects · GC.KeepAlive myths · rethrow across await boundaries.
- **events:** a handler that throws takes down the rest of the invocation list · `?.Invoke` race between null-check and call.
- **serialization:** DateTime Kind lost in a JSON round-trip · polymorphic `$type` handling · reference loops.
- **linq / collections:** GroupBy on reference-equality keys · Contains vs Any confusion · OrderBy with a non-deterministic key.
- **new halls** (see `halls.md`): each planned hall needs its opening exhibit - nullability, generics, enums, inheritance, pattern-matching, records, equality, disposal, boxing, reflection, memory, http, configuration, logging, regex, testing, io.
