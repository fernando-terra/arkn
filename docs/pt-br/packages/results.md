# Arkn.Results

Sucesso/falha explícitos sem exceções. **Zero dependências externas.**

```bash
dotnet add package Arkn.Results
```

## Tipos de erro

```csharp
Error.Failure("Order.Failed",       "Order processing failed");
Error.NotFound("Product.NotFound",  "Product not found");
Error.Validation("Email.Invalid",   "Email is not valid");
Error.Conflict("User.Exists",       "User already exists");
Error.Unauthorized("Auth.Required", "Authentication required");
Error.Forbidden("User.Forbidden",   "Access denied");
```

## API funcional

```csharp
Result<User> result = await GetUserAsync(id);

// Map — transforma o valor de sucesso
Result<string> name = result.Map(u => u.Name);

// Bind — encadeia operações
Result<Profile> profile = result.Bind(u => GetProfileAsync(u.Id));

// Ensure — valida inline
result.Ensure(u => u.IsActive, Error.Validation("User.Inactive", "User must be active"));

// Tap — efeitos colaterais sem quebrar a cadeia
result.Tap(u => logger.LogInformation("Got user {Id}", u.Id));

// Match — ramifica no resultado
IResult response = result.Match(
    onSuccess: user  => Results.Ok(user),
    onFailure: error => Results.Problem(error.Message));
```

## Conversões implícitas

```csharp
Result<User> success = user;                              // T → Result<T>
Result<User> failure = Error.NotFound("U.NF", "msg");    // Error → Result<T>
```

## Múltiplos erros

```csharp
var errors = validationErrors.Select(e => Error.Validation(e.Field, e.Message));
return Result.Failure<CreateOrderResponse>(errors);

result.Errors // IReadOnlyList<Error>
```
