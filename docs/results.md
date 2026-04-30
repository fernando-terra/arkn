# Arkn.Results

## Why Result Pattern?

Exceptions are expensive and represent exceptional control flow. The Result Pattern makes success and failure **explicit in the type system**, enabling:
- Railway-oriented programming
- No hidden control flow
- Composable error handling

## Quick Reference

```csharp
// Success
Result<User> result = Result.Success(user);
Result<User> result = user; // implicit conversion

// Failure
Result<User> result = Result.Failure<User>(Error.NotFound("User.NotFound", "Not found"));
Result<User> result = Error.NotFound("User.NotFound", "Not found"); // implicit

// No-value result
Result result = Result.Success();
Result result = Result.Failure(error);

// Map — transform the value
result.Map(u => u.Name)  // Result<string>

// Bind — chain operations
result.Bind(u => FindProfile(u.Id))  // Result<Profile>

// Match — branch on outcome
result.Match(
    onSuccess: u => Ok(u),
    onFailure: e => Problem(e.Message));

// Ensure — assert a condition
result.Ensure(u => u.IsActive, Error.Validation("User.Inactive", "User is inactive"));

// Tap — side effects without breaking the chain
result.Tap(u => logger.LogInformation("Got user {Id}", u.Id));

// Multiple errors (validation)
Result<T> result = Result.Failure<T>(new[] { error1, error2 });
result.Errors // IReadOnlyList<Error>
```

## Error types

| Factory | `ErrorType` | HTTP suggestion |
|---|---|---|
| `Error.Failure` | `Failure` | 500 |
| `Error.NotFound` | `NotFound` | 404 |
| `Error.Validation` | `Validation` | 400 |
| `Error.Conflict` | `Conflict` | 409 |
| `Error.Unauthorized` | `Unauthorized` | 401 |
| `Error.Forbidden` | `Forbidden` | 403 |
