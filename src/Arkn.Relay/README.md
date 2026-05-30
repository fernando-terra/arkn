# Arkn.Relay 📡

A lightweight, zero-dependency in-memory mediator for the Arkn framework.

## Overview

`Arkn.Relay` is the messaging backbone of the Arkn framework. It implements the Mediator pattern to decouple request senders from request handlers, with a focus on performance and native integration with the Arkn Result Pattern.

## Why Arkn.Relay?

- **Zero External Dependencies:** No MediatR, no reflection-heavy libraries.
- **Result Pattern First:** Designed to return `Result<T>` or `Result` natively.
- **High Performance:** Optimized for .NET 10, utilizing Source Generators to eliminate runtime reflection.
- **Opinionated & Simple:** No bloat, just commands, queries, and behaviors.

## Getting Started

### 1. Define a Request

```csharp
public record CreateUserCommand(string Email) : IRequest<UserDto>;
```

### 2. Define a Handler

```csharp
public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(CreateUserCommand request, CancellationToken ct)
    {
        // Implementation
        return Result.Success(new UserDto(request.Email));
    }
}
```

### 3. Send via Relay

```csharp
var result = await relay.SendAsync(new CreateUserCommand("hello@arkn.io"));
```

## Documentation

For full documentation, visit [docs.arkn.io](https://docs.arkn.io).

## License

This project is licensed under the Apache 2.0 License - see the LICENSE file for details.
