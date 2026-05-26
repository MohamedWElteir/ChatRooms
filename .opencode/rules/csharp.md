---
paths:
  - "**/*.cs"
---
# C# Rules

## Naming
- Classes, methods, properties: PascalCase
- Parameters, locals: camelCase
- Private fields: _camelCase
- Constants: PascalCase (not SCREAMING_SNAKE)

## Structure
- One class per file, filename matches class name
- Constructors first, then public methods, then private methods
- Keep methods under 30 lines; extract if longer

## Patterns
- Null checks: use `ArgumentNullException.ThrowIfNull()`
- String formatting: interpolation over concatenation
- Async: always `async Task`, never `async void` except event handlers
- Cancellation: propagate `CancellationToken` through async chains
- Dispose: implement IDisposable via `using` wherever possible
