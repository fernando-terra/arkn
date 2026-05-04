# Arkn.Http

> **Conventions you can read. Patterns you can enforce.**

Fluent typed HTTP client where every call returns `Result<T>`. Built-in retry. No raw `HttpClient`.

## Install

```bash
dotnet add package Arkn.Http
```

## Quick example

```csharp
// Define your client
public class UserApiClient : ArknHttpClient
{
    public UserApiClient(HttpClient http) : base(http) { }

    public Task<Result<User>> GetUserAsync(Guid id, CancellationToken ct = default) =>
        GetAsync<User>($"/users/{id}", ct);

    public Task<Result<User>> CreateUserAsync(CreateUserRequest req, CancellationToken ct = default) =>
        PostAsync<CreateUserRequest, User>("/users", req, ct);
}

// Register
builder.Services.AddArknHttpClient<UserApiClient>(options =>
{
    options.BaseUrl    = "https://api.example.com";
    options.MaxRetries = 3;
});

// Use — every call returns Result<T>
Result<User> result = await client.GetUserAsync(id);
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Http](https://www.nuget.org/packages/Arkn.Http)
