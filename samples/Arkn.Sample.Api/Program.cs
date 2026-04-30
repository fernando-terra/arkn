using Arkn.Http.Extensions;
using Arkn.Sample.Api.Features.HttpDemo;
using Arkn.Sample.Api.Features.Users;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

// Register ExternalUserClient with Arkn.Http — 3 retries, 30s timeout
builder.Services
    .AddArknHttp<ExternalUserClient>("https://jsonplaceholder.typicode.com")
    .WithRetry(maxAttempts: 3, baseDelay: TimeSpan.FromMilliseconds(200))
    .WithTimeout(TimeSpan.FromSeconds(30));

var app = builder.Build();

app.MapUsersEndpoints();
app.MapHttpDemoEndpoints();
app.Run();
