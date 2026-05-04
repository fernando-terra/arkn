var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Example: map Result<T> to HTTP responses
app.MapGet("/items/{id:int}", (int id) =>
{
    Result<string> result = id > 0
        ? Result.Success($"Item {id}")
        : Result.Failure<string>(Error.NotFound("Item.NotFound", $"Item {id} not found"));

    return result.Match(
        onSuccess: value  => Results.Ok(new { value }),
        onFailure: error  => error.Type switch
        {
            ErrorType.NotFound   => Results.NotFound(new { error.Code, error.Message }),
            ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
            _                    => Results.Problem(error.Message)
        });
});

app.Run();
