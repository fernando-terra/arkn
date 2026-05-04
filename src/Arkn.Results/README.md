# Arkn.Results

> **Conventions you can read. Patterns you can enforce.**

`Result<T>` and `Error` — business failures as first-class types. No exceptions for domain logic.

## Install

```bash
dotnet add package Arkn.Results
```

## Quick example

```csharp
// Return results, never throw
public Result<User> GetUser(Guid id)
{
    var user = _repo.Find(id);
    return user is null
        ? Error.NotFound("User.NotFound", $"User {id} not found.")
        : Result.Success(user);
}

// Consume with Match
return result.Match(
    onSuccess: user  => Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound   => NotFound(),
        ErrorType.Validation => BadRequest(error.Message),
        _                    => Problem(error.Message)
    });
```

## API

```csharp
// Factories
Result.Success()                    // void success
Result.Success(value)               // typed success
Result<T>.Ok(value)                 // shorthand
Result.Failure(error)               // void failure
Result<T>.Fail(error)               // typed failure
Result<T>.Fail(errors)              // multiple errors

// Error factories — message is optional (defaults to code)
Error.NotFound("User.NotFound")                          // concise
Error.NotFound("User.NotFound", $"User '{id}' not found.") // explicit
Error.Validation("Order.QuantityInvalid")
Error.Conflict("Email.AlreadyRegistered")
Error.Unauthorized("Auth.TokenExpired")

// Chaining
result.Map(v => transform(v))
      .Bind(v => nextOperation(v))
      .Tap(v => sideEffect(v))
      .Match(onSuccess, onFailure);
```

## ErrorGroup pattern

Group all errors for a domain in one static class:

```csharp
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' not found.");

    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail", "Email is invalid.");

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("User.EmailAlreadyRegistered", "Already in use.");
}

// Usage
return user is null ? UserErrors.NotFound(id) : Result.Success(user);
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Results](https://www.nuget.org/packages/Arkn.Results)
