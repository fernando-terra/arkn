# Arkn.Results

Explicit success/failure without exceptions. **Zero external dependencies.**

```bash
dotnet add package Arkn.Results
```

## Error types

```csharp
Error.Failure("Order.Failed",       "Order processing failed");
Error.NotFound("Product.NotFound",  "Product not found");
Error.Validation("Email.Invalid",   "Email is not valid");
Error.Conflict("User.Exists",       "User already exists");
Error.Unauthorized("Auth.Required", "Authentication required");
Error.Forbidden("User.Forbidden",   "Access denied");
```

## Functional API

```csharp
Result<User> result = await GetUserAsync(id);

// Map — transform success value
Result<string> name = result.Map(u => u.Name);

// Bind — chain operations
Result<Profile> profile = result.Bind(u => GetProfileAsync(u.Id));

// Ensure — validate inline
result.Ensure(u => u.IsActive, Error.Validation("User.Inactive", "User must be active"));

// Tap — side effects without breaking the chain
result.Tap(u => logger.LogInformation("Got user {Id}", u.Id));

// Match — branch on outcome
IResult response = result.Match(
    onSuccess: user  => Results.Ok(user),
    onFailure: error => Results.Problem(error.Message));
```

## Implicit conversions

```csharp
Result<User> success = user;                              // T → Result<T>
Result<User> failure = Error.NotFound("U.NF", "msg");    // Error → Result<T>
```

## Multiple errors

```csharp
var errors = validationErrors.Select(e => Error.Validation(e.Field, e.Message));
return Result.Failure<CreateOrderResponse>(errors);

result.Errors // IReadOnlyList<Error>
```
