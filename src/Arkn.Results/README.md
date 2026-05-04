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

// Error factories
Error.Failure("Order.Failed", "msg")
Error.NotFound("User.NotFound", "msg")
Error.Validation("Email.Invalid", "msg")
Error.Conflict("Email.Taken", "msg")
Error.Unauthorized("Auth.Expired", "msg")

// Chaining
result.Map(v => transform(v))
      .Bind(v => nextOperation(v))
      .Tap(v => sideEffect(v))
      .Match(onSuccess, onFailure);
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Results](https://www.nuget.org/packages/Arkn.Results)
