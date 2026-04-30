using Arkn.Sample.Api.Features.Users;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapUsersEndpoints();
app.Run();
