# Arkn.Relay 📡

O `Arkn.Relay` é o mediador in-memory oficial do framework Arkn. Ele foi projetado para desacoplar a intenção (IRequest) do processamento (IRequestHandler), garantindo alta performance e integração nativa com o Result Pattern.

## Por que usar o Arkn.Relay?

- **Zero Dependências:** Sem MediatR ou bibliotecas de terceiros.
- **Focado em Resultados:** Retorna nativamente `Result<T>` ou `Result`.
- **Performance de Elite:** Preparado para .NET 10 e despacho via Source Generators.
- **Opinativo e Simples:** Apenas Comandos, Queries e Behaviors, sem complexidade desnecessária.

## Instalação

```bash
dotnet add package Arkn.Relay
```

## Como usar

### 1. Definir uma Requisição (IRequest)

No Arkn, toda requisição deve retornar um `Result`.

```csharp
public record RegistrarUsuarioCommand(string Email, string Nome) : IRequest<UsuarioDto>;
```

### 2. Definir o Handler (IRequestHandler)

```csharp
public class RegistrarUsuarioHandler : IRequestHandler<RegistrarUsuarioCommand, UsuarioDto>
{
    public async Task<Result<UsuarioDto>> HandleAsync(RegistrarUsuarioCommand request, CancellationToken ct)
    {
        // Lógica de negócio aqui...
        return Result.Success(new UsuarioDto(request.Email));
    }
}
```

### 3. Configurar no Injeção de Dependência

```csharp
services.AddArknRelay();
services.AddArknHandler<RegistrarUsuarioCommand, UsuarioDto, RegistrarUsuarioHandler>();
```

### 4. Despachar via IRelay

```csharp
public class UsuarioController(IRelay relay) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RegistrarUsuarioCommand command)
    {
        var result = await relay.SendAsync(command);
        
        return result.Match(
            success => Ok(success),
            error => BadRequest(error)
        );
    }
}
```

## Próximos Passos

- [ ] Implementar Behaviors (Pipeline) para Validação e Logging automático.
- [ ] Implementar Source Generator para despacho Zero-Reflection.
- [ ] Integração com o `Arkn.Analyzers` para validar handlers em tempo de compilação.
