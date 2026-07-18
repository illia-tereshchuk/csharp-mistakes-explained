# C# Bad Practices

Ideal tutorials teach you SOLID. **Bad** code is remembered better.

> Museum stats: **11** exhibits in **8** halls, latest addition - **#0011**.

### 🗂 Collections

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0001 | [Modifying a collection while iterating](src/collections/0001-modify-while-enumerating/) | 🟢 | `foreach` + `Remove` on the same list - partial execution and a crash |
| 0004 | [Mutating a dictionary key](src/collections/0004-dictionary-key-mutation/) | 🟡 | change a field on the key - `foreach` still shows the entry, lookups can't find it |

### 🔢 Numbers

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0002 | [Calculating money with double](src/numbers/0002-doubles-for-money/) | 🟢 | `0.1 + 0.2 != 0.3` - binary floats can't hold decimal cents |

### ⚡ Async & Threading

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0003 | [Incrementing a shared counter from parallel threads](src/async/0003-race-on-shared-counter/) | 🟢 | `counter++` from two threads - thousands of increments quietly vanish. |
| 0007 | [async void and the uncatchable exception](src/async/0007-async-void/) | 🟡 | an exception in `async void` sails past your try/catch and kills the process |

### 💥 Exceptions

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0005 | [Rethrowing with throw ex](src/exceptions/0005-throw-ex-stack-amnesia/) | 🟡 | `throw ex` wipes the stack trace - the investigation starts at the wrong line |

### 🔗 LINQ & Lambdas

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0006 | [A closure capturing the loop variable](src/linq/0006-closure-over-loop-variable/) | 🟢 | five callbacks, one shared `i` - every lambda reads the value after the loop ended |
| 0009 | [Enumerating a LINQ query twice](src/linq/0009-multiple-enumeration/) | 🟡 | the header counted 3 rows, the body printed 2 - each enumeration reruns the query |

### 🗄 ORM

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0008 | [The N+1 query problem](src/orm/0008-n-plus-one/) | 🟡 | loading 20 orders costs 21 SQL queries - one for the list, one more per row |

### 🔔 Events

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0010 | [A static event that never lets go](src/events/0010-immortal-subscriber/) | 🔴 | a closed widget keeps reacting to events - the static event pins it in memory forever |

### 📦 Value Types

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0011 | [A mutable struct behind a readonly field](src/value-types/0011-defensive-copy-ambush/) | 🔴 | every method call on the readonly field runs on a hidden copy - points added, balance forever 0 |

# To Be Continued

More halls under construction: **datetime**,
**strings & memory**, **DI lifetimes**, **security**.
