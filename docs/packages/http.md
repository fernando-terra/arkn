# Arkn.Http

Typed HTTP client with Result-based error handling, retry, OAuth2, rate limiting, caching and mTLS. **Zero external SDK dependency.**

```bash
dotnet add package Arkn.Http
```

## Typed client

Define a typed client extending `ArknHttpClient`:

```csharp
using Arkn.Http;
using Arkn.Results;

public sealed class PaymentClient(IArknHttp http)
    : ArknHttpClient(http, "https://api.payments.com")
{
    public Task<Result<PaymentDto>>           GetAsync(Guid id)                    => GetAs<PaymentDto>("/payments/{id}", id);
    public Task<Result<IReadOnlyList<PaymentDto>>> GetAllAsync()                   => GetAs<IReadOnlyList<PaymentDto>>("/payments");
    public Task<Result<PaymentDto>>           CreateAsync(PaymentRequest req)      => PostAs<PaymentDto>("/payments", req);
    public Task<Result<PaymentDto>>           UpdateAsync(Guid id, PaymentRequest req) => PutAs<PaymentDto>("/payments/{id}", req, id);
    public Task<Result>                       CancelAsync(Guid id)                 => Delete("/payments/{id}", id);
}
```

## DI registration

```csharp
builder.Services
    .AddArknHttp<PaymentClient>("https://api.payments.com")
    .WithRetry(maxAttempts: 3)
    .WithTimeout(TimeSpan.FromSeconds(30));
```

## Authentication

### API Key

```csharp
builder.Services
    .AddArknHttp<PaymentClient>("https://api.payments.com")
    .WithApiKey("X-Api-Key", config["Payment:ApiKey"]);
```

### OAuth2 Client Credentials

```csharp
builder.Services
    .AddArknHttp<PaymentClient>("https://api.payments.com")
    .WithClientCredentials(oauth =>
    {
        oauth.TokenEndpoint = "https://auth.payments.com/oauth2/token";
        oauth.ClientId      = config["Payment:ClientId"];
        oauth.ClientSecret  = config["Payment:ClientSecret"];
        oauth.Scope         = "payments:read payments:write";
    });
```

### Mutual TLS (mTLS)

```csharp
// From PFX file
.WithClientCertificate(cert => cert.FromPfx("certs/client.pfx", "password"))

// From PEM files
.WithClientCertificate(cert => cert.FromPem("certs/client.crt", "certs/client.key"))

// From X509Certificate2
.WithClientCertificate(cert => cert.FromCertificate(myCert))

// From Windows certificate store
.WithClientCertificate(cert => cert.FromStore(
    StoreName.My, StoreLocation.CurrentUser, thumbprint: "ABC123..."))
```

## Rate limit handling

```csharp
builder.Services
    .AddArknHttp<PaymentClient>("https://api.payments.com")
    .WithRateLimitHandling(rateLimit =>
    {
        rateLimit.RetryAfterHeader = true;   // respects Retry-After / X-RateLimit-Reset
        rateLimit.MaxRetries       = 2;
        rateLimit.DefaultBackoff   = TimeSpan.FromSeconds(5);
    });
```

## Response caching

```csharp
builder.Services
    .AddArknHttp<PaymentClient>("https://api.payments.com")
    .WithResponseCaching(cache =>
    {
        cache.MaxAge        = TimeSpan.FromMinutes(5);
        cache.VaryByHeaders = ["Accept-Language"];
    });
```

## Debug logging

```csharp
builder.Services
    .AddArknHttp<PaymentClient>("https://api.payments.com")
    .WithDebugLogging(DebugLoggingOptions.Development);   // full request + response
    // .WithDebugLogging(DebugLoggingOptions.Production);  // headers only
    // .WithDebugLogging(DebugLoggingOptions.FailuresOnly); // 4xx/5xx only
```

## Shorthand methods reference

| Method | HTTP verb | Returns |
|---|---|---|
| `GetAs<T>(path, ...args)` | GET | `Task<Result<T>>` |
| `PostAs<T>(path, body, ...args)` | POST | `Task<Result<T>>` |
| `PutAs<T>(path, body, ...args)` | PUT | `Task<Result<T>>` |
| `PatchAs<T>(path, body, ...args)` | PATCH | `Task<Result<T>>` |
| `Delete(path, ...args)` | DELETE | `Task<Result>` |
