# P01 — Find user by ID

**Context:** You are working in a .NET 10 project that uses the Arkn framework.

**Task:** Write a C# class `UserService` with a method `GetByIdAsync(Guid id, CancellationToken ct)` that:
1. Receives a `Guid id`
2. Looks up a user from a `List<User>` in-memory store (inject via constructor)
3. Returns the user if found
4. Returns a not-found error if the user does not exist

The `User` record has: `Guid Id`, `string Name`, `string Email`.

**Expected patterns:**
- Method returns `Task<Result<User>>`
- Uses `Error.NotFound("User.NotFound", ...)` for the failure case
- No exceptions thrown
