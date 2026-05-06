# Arkn.Analyzers

Analyzers Roslyn que impõem padrões Arkn em tempo de compilação.

```bash
dotnet add package Arkn.Analyzers
```

## Regras

| ID | Título | Severidade |
|---|---|---|
| ARK001 | Métodos de domínio devem retornar `Result` ou `Result<T>` | ⚠️ Warning |
| ARK002 | Código de erro deve seguir o padrão `Namespace.Reason` | ⚠️ Warning |
| ARK003 | `Result` não deve ser silenciosamente descartado | ⚠️ Warning |
| ARK004 | `IArknJob.ExecuteAsync` deve retornar `Task<Result>` | ❌ Error |
| ARK005 | `HttpClient` bruto em vez de `AddArknHttp<T>()` | ⚠️ Warning |
| ARK006 | `ILogger` do MEL usado onde `IArknLogger` está disponível | ⚠️ Warning |
| ARK007 | `throw new` em método de domínio | ⚠️ Warning |
| ARK008 | Bloco `catch` engolindo exceção sem `Result.Failure` | ⚠️ Warning |

## ARK001 — Retornos Result no Domínio

```csharp
// ❌ ARK001 — lança exceção em método de domínio
namespace MyApp.Domain.Entities;
public class Order : Entity {
    public void Cancel() {
        if (!IsActive) throw new InvalidOperationException("Already cancelled"); // ARK001
    }
}

// ✅ Correto
public Result Cancel() {
    if (!IsActive) return Error.Conflict("Order.AlreadyCancelled", "Order is already cancelled.");
    // ...
    return Result.Success();
}
```

## ARK002 — Nomenclatura de código de erro

```csharp
// ❌ ARK002
Error.NotFound("usernotfound", "msg");  // sem separador ponto
Error.NotFound("user.notfound", "msg"); // inicial minúscula

// ✅ Correto
Error.NotFound("User.NotFound", "msg");
```

## ARK003 — Descarte de Result

```csharp
// ❌ ARK003 — Result é descartado
GetUser(id); // resultado jogado fora

// ✅ Correto
var result = GetUser(id);
result.Match(onSuccess: ..., onFailure: ...);
```

## ARK004 — Tipo de retorno do Job

```csharp
// ❌ ARK004 — tipo de retorno incorreto
public class MyJob : IArknJob {
    public Task ExecuteAsync(ArknJobContext ctx) { ... }  // ARK004: Error
}

// ✅ Correto
public Task<Result> ExecuteAsync(ArknJobContext ctx) { ... }
```

## ARK005 — HttpClient bruto <Badge type="tip" text="v0.3.0" />

```csharp
// ❌ ARK005
var client = new HttpClient();

// ✅ Use cliente tipado via AddArknHttp
builder.Services.AddArknHttp<PaymentClient>("https://api.pay.com");
```

## ARK006 — MEL ILogger em vez de IArknLogger <Badge type="tip" text="v0.3.0" />

```csharp
// ❌ ARK006 — logger MEL em componente Arkn
public class MyService(ILogger<MyService> logger) { ... }
// ❌ Também dispara
Console.WriteLine("something happened");

// ✅ Use IArknLogger para logging estruturado roteado por sink
public class MyService(IArknLogger logger) { ... }
```

## ARK007 — throw em método de domínio

```csharp
// ❌ ARK007
public Result Process() {
    throw new InvalidOperationException("bad state"); // ARK007
}

// ✅ Retorne falha em vez disso
public Result Process() {
    return Result.Failure(Error.Failure("Order.InvalidState", "Invalid state."));
}
```

## ARK008 — Catch vazio engolindo exceções

```csharp
// ❌ ARK008 — exceção é engolida, o chamador não sabe que falhou
try { DoWork(); }
catch (Exception) { }  // ARK008

// ✅ Exponha a falha via Result
try { return DoWork(); }
catch (Exception ex) {
    return Result.Failure(Error.Failure("Service.Unexpected", ex.Message));
}
```

## Suprimindo uma regra

```csharp
#pragma warning disable ARK001
public void LegacyMethod() { throw new NotImplementedException(); }
#pragma warning restore ARK001
```
