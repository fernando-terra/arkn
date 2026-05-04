# P03 — Minimal API endpoint with error mapping

**Context:** You are working in a .NET 10 Minimal API project using the Arkn framework.

**Task:** Write a static class `UserEndpoints` with a method `MapUserEndpoints(IEndpointRouteBuilder app)` that registers:

- `GET /users/{id:guid}` — calls `IUserService.GetByIdAsync(id)` and maps the result to HTTP

**Error mapping:**
- `ErrorType.NotFound` → 404 NotFound
- `ErrorType.Validation` → 400 BadRequest with `{ code, message }`
- anything else → 500 Problem

**Expected patterns:**
- Uses `.Match()` to branch on success/failure
- Uses static `Results.*` methods (Ok, NotFound, BadRequest, Problem)
- No try/catch blocks
