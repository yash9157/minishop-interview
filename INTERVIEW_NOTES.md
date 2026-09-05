# Interview discussion notes

## Duplicate-processing question

If a client times out after user creation succeeds, retrying the same POST must not
repeat the operation.

- The client sends a stable `Idempotency-Key` for the logical request.
- The API stores that key and the created resource in the same database transaction
  as user creation, role assignment and the audit event.
- A unique index on `(Operation, Key)` is the final concurrency guard.
- Email has a unique database index and the user-role join table has a composite
  primary key, so data integrity does not depend only on an application check.
- A retry with a completed key returns the original resource. A concurrent request
  that loses the unique-key race is rolled back and can safely retry.

This project implements that approach for `POST /api/users` with the
`IdempotencyRecords` table. ASP.NET Core Identity supplies the unique normalized
email and composite user-role constraints.

## Ranking deduction question

The candidates who can be third are **A, B and D**.

Examples proving each possibility:

- A is third: E, D, A, B, C
- B is third: E, A, B, D, C
- D is third: E, A, D, B, C

E cannot be third because `E < A < B` would force B to rank 5. C cannot be third
because D must be rank 2 (D cannot be rank 1), which again forces B to rank 5.

## Production-deployment question

From the visible part of the question, Change A fixes a security vulnerability and
has passed all tests. Change B is an independent reporting/database-query refactor
whose performance testing is incomplete. I would separate the changes, deploy the
verified security fix through the normal emergency/change process, and hold Change B
until performance testing and rollback evidence are complete. A Friday-evening
combined release unnecessarily couples the urgent fix to an unverified change.
