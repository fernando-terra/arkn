# Arkn.Http

> **Conventions you can read. Patterns you can enforce.**

Fluent typed HTTP client where every call returns `Result<T>`. Built-in retry, timeout, and pluggable auth interceptors. No raw `HttpClient`.

## Install

```bash
dotnet add package Arkn.Http
```

## Quick example

```csharp
// Define your typed client — minimal boilerplate
public class UserApiClient(IArknHttp http) : ArknHttpClient(http, "https://api.example.com")
{
    public Task<Result<User>>   GetAsync(Guid id)                => GetAs<User>("/users/{id}", id);
    public Task<Result<User>>   CreateAsync(CreateUserRequest r) => PostAs<User>("/users", r);
    public Task<Result<User>>   UpdateAsync(Guid id, UpdateUserRequest r) => PutAs<User>("/users/{id}", r, id);
    public Task<Result>         DeleteAsync(Guid id)             => Delete("/users/{id}", id);
}

// Register
builder.Services.AddArknHttp<UserApiClient>("https://api.example.com")
    .WithRetry(maxAttempts: 3)
    .WithTimeout(TimeSpan.FromSeconds(15));
```

## Shorthand methods

`ArknHttpClient` exposes concise protected methods — no more `.Request().Verb().As<T>()` chains:

```csharp
GetAs<T>(path, args)          // GET → Result<T>
PostAs<T>(path, body)         // POST → Result<T>
PutAs<T>(path, body, args)    // PUT → Result<T>
PatchAs<T>(path, body, args)  // PATCH → Result<T>
Delete(path, args)            // DELETE → Result

Get(path, args)               // GET → Result (no body)
Post(path, body)              // POST → Result (no body)
Put(path, body, args)         // PUT → Result (no body)
```

## Auth interceptors

### Bearer token (custom factory)

```csharp
builder.Services.AddArknHttp<UserApiClient>("https://api.example.com")
    .WithBearerAuth(async () =>
    {
        // Called once; token cached in InMemoryTokenStore for 55 min
        return await myAuthService.GetAccessTokenAsync();
    });
```

### OAuth2 Client Credentials (built-in, zero external deps)

```csharp
builder.Services.AddArknHttp<UserApiClient>("https://api.example.com")
    .WithClientCredentials(opts =>
    {
        opts.TokenUrl     = "https://auth.example.com/oauth/token";
        opts.ClientId     = "my-client";
        opts.ClientSecret = config["Auth:Secret"];
        opts.Scope        = "api.read api.write";
    });
```

The `ClientCredentialsInterceptor` fetches a token via `POST` to `TokenUrl`, caches it using `InMemoryTokenStore` (with buffer of 30s before expiry), and attaches it as `Authorization: Bearer <token>` on every request.

### Custom interceptor

```csharp
public sealed class ApiKeyInterceptor : IArknAuthInterceptor
{
    private readonly string _key;
    public ApiKeyInterceptor(string key) => _key = key;

    public Task ApplyAsync(HttpRequestMessage req, CancellationToken ct = default)
    {
        req.Headers.Add("X-Api-Key", _key);
        return Task.CompletedTask;
    }
}

builder.Services.AddArknHttp<UserApiClient>("https://api.example.com")
    .WithInterceptor(new ApiKeyInterceptor(config["ApiKey"]));
```

## Debug logging

Loga request e response completos via `IArknLogger` — flui para todos os sinks configurados (console, arquivo, **Application Insights**).

```csharp
// Dev — tudo em Debug
.WithDebugLogging()

// Preset de produção — 2xx em Info (chega no AppInsights), 4xx Warning, 5xx Error
.WithDebugLogging(DebugLoggingOptions.Production)

// Só falhas — 2xx são silenciosas
.WithDebugLogging(DebugLoggingOptions.FailuresOnly)

// Customizado
.WithDebugLogging(opts =>
{
    opts.SuccessLevel     = ArknLogLevel.Info;
    opts.LogResponseBody  = false;
    opts.LogHeaders       = false;
})
```

### Presets

| Preset | 2xx | 4xx | 5xx | Caso de uso |
|--------|-----|-----|-----|-------------|
| `Development` (default) | Debug | Warning | Error | Dev local |
| `Production` | Info | Warning | Error | Tracing completo no AppInsights |
| `FailuresOnly` | Trace (silencioso) | Warning | Error | Só erros em prod |

### Output de exemplo

```
→ GET https://api.example.com/users/123
  Authorization: Bearer eyJhbGc***    ← sanitizado

← 200 OK (87ms)
  Body: {
    "id": "123",
    "name": "Alice"
  }
```

### Integração com Application Insights

```csharp
// Program.cs
builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();  // dev
    if (!env.IsDevelopment())
        logging.AddApplicationInsights(opts =>
        {
            opts.ConnectionString = config["AI:ConnectionString"];
            opts.MinimumLevel     = ArknLogLevel.Info; // 2xx chegam se Production preset
        });
});

builder.Services.AddArknHttp<UserApiClient>("https://api.example.com")
    .WithDebugLogging(env.IsDevelopment()
        ? DebugLoggingOptions.Development
        : DebugLoggingOptions.Production);
```

- Headers sensíveis (`Authorization`, `Cookie`, `X-Api-Key`) sanitizados automaticamente
- Body de resposta formatado como JSON indentado quando possível
- Requer `IArknLogger` no DI via `AddArknLogging()`

## Token store

`InMemoryTokenStore` is registered as a singleton automatically when any auth method is configured. Tokens are cached with a 30-second expiry buffer and can be invalidated manually:

```csharp
var store = serviceProvider.GetRequiredService<IArknTokenStore>();
await store.InvalidateAsync("my-client_credentials");
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Http](https://www.nuget.org/packages/Arkn.Http)
