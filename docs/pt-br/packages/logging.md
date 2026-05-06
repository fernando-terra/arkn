# Arkn.Logging

Logging estruturado com sinks plugáveis. **Zero dependências externas.**

```bash
dotnet add package Arkn.Logging
```

## Configuração

```csharp
builder.Services.AddArknLogging(logging =>
{
    logging.SetMinimumLevel(ArknLogLevel.Info);
    logging.AddConsoleSink();
    logging.AddFileSink("logs/app.log");
    logging.AddInMemorySink();    // usado por Arkn.Jobs para logs por execução
});
```

## Uso

```csharp
public class MyService(IArknLogger logger)
{
    public void DoWork()
    {
        var ctx = ArknLogContext.ForScope("operation-123")
            .With("UserId", 42)
            .With("Action", "ProcessOrder");

        logger.Info("Starting work", ctx);
        logger.Warning("Slow query detected", ctx);
        logger.Error("Operation failed", ex, ctx);
    }
}
```

## Sinks disponíveis

| Pacote | Sink | Observações |
|---|---|---|
| `Arkn.Logging` | `ConsoleLogSink` | Colorido por nível |
| `Arkn.Logging` | `FileSink` | Append, auto-flush |
| `Arkn.Logging` | `InMemoryLogSink` | Isolado por escopo, usado por Jobs |
| `Arkn.Extensions.Logging.Seq` | `SeqSink` | CLEF via HTTP, zero Serilog |
| `Arkn.Extensions.Logging.Elasticsearch` | `ElasticsearchSink` | Bulk API, zero NEST |
