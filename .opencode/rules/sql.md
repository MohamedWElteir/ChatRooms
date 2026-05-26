---
paths:
  - "**/*.sql"
  - "**/*Repository*.cs"
  - "**/*Handler*.cs"
---
# SQL Rules
- Always use parameterized queries. Never string-concatenate SQL inputs.
- Every SELECT should have a WHERE or a documented reason it doesn't.
- Index columns used in WHERE, JOIN ON, and ORDER BY for large tables.
- Watch for implicit type conversions (NVARCHAR vs VARCHAR kills index seeks).
- No SELECT * in production code — list columns explicitly.
- Stored procedures: name them [Schema].[Action][Object] (e.g. billing.GetOpenEpisodes).
- Test queries in SSMS with SET STATISTICS IO ON before committing.
