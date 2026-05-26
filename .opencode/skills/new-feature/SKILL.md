---
description: >
  Scaffolds a new feature end-to-end in Clean Architecture. Use when adding
  a new entity, command, query, use case, or endpoint. Triggers: "add feature",
  "scaffold", "create command", "create query", "new use case", "add endpoint".
---
## Current Architecture Snapshot



## Instructions
Scaffold the full vertical slice for the requested feature:
1. Domain: Entity or Value Object (if new concept)
2. Application: Command or Query + Handler + Validator (FluentValidation)
3. Infrastructure: Repository implementation update if needed
4. API: Controller action or Minimal API endpoint
5. Tests: Handler unit test skeleton

Follow the exact file naming and namespace patterns found in the codebase snapshot.
Do NOT create abstractions that don't exist yet in the project.
