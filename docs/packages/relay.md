# Arkn.Relay 📡

`Arkn.Relay` is the official in-memory mediator for the Arkn framework. It is designed to decouple intention (IRequest) from processing (IRequestHandler), ensuring high performance and native integration with the Result Pattern.

## Why use Arkn.Relay?

- **Zero Dependencies:** No MediatR or third-party libraries.
- **Result-First:** Returns `Result<T>` or `Result` natively.
- **Elite Performance:** Optimized for .NET 10 and Zero-Reflection dispatch via Source Generators.
- **Opinionated & Simple:** Just Commands, Queries, and Behaviors, without unnecessary complexity.

## Installation

```bash
dotnet add package Arkn.Relay
```

## How to use

### 1. Define a Request (IRequest)

In Arkn, every request should return a `Result`.

```csharp
public record RegisterUserCommand(string Email, string Name) : IRequest<UserDto>;
```

### 2. Define the Handler (IRequestHandler)

```csharp
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(RegisterUserCommand request, CancellationToken ct)
    {
        // Business logic here...
        return Result.Success(new UserDto(request.Email));
    }
}
```

### 3. Configure Dependency Injection

```csharp
services.AddArknRelay();
services.AddArknHandler<RegisterUserCommand, UserDto, RegisterUserHandler>();
```

### 4. Dispatch via IRelay

```csharp
public class UserController(IRelay relay) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RegisterUserCommand command)
    {
        var result = await relay.SendAsync(command);
        
        return result.Match(
            success => Ok(success),
            error => BadRequest(error)
        );
    }
}
```

## Next Steps

- [ ] Implement Behaviors (Pipeline) for automatic Validation and Logging.
- [ ] Implement Source Generator for Zero-Reflection dispatch.
- [ ] Integration with `Arkn.Analyzers` to validate handlers at compile time.
