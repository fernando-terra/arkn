# Primeiros Passos

> **v0.3.0** — Este guia cobre a versão estável atual.

## Instalação

Instale apenas os pacotes que você precisar:

```bash
# Abstrações do core (Entity, ValueObject, AggregateRoot)
dotnet add package Arkn.Core

# Padrão Result — sucesso/falha explícito
dotnet add package Arkn.Results

# Logging estruturado com sinks plugáveis
dotnet add package Arkn.Logging

# Agendador de cron jobs zero-dependência
dotnet add package Arkn.Jobs

# Abstrações de notificações + Slack
dotnet add package Arkn.Notifications
dotnet add package Arkn.Extensions.Notifications.Slack

# Cliente HTTP tipado com retry, OAuth2 e mTLS
dotnet add package Arkn.Http

# Analyzers Roslyn — verificação em tempo de compilação (ARK001–ARK008)
dotnet add package Arkn.Analyzers

# Source generator — elimina o boilerplate de fábricas de Error
dotnet add package Arkn.SourceGen

# Servidor MCP — scaffold e validação de ferramentas para assistentes de IA
dotnet tool install -g Arkn.MCP
```

## Início rápido com o padrão Result

```csharp
using Arkn.Results;

// Defina erros (manualmente ou via Arkn.SourceGen)
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User {id} was not found.");

    public static Error InvalidEmail =>
        Error.Validation("User.InvalidEmail", "Email address is not valid.");
}

// Retorne Result nos métodos de domínio/aplicação
public async Task<Result<UserDto>> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    if (user is null) return UserErrors.NotFound(id);
    return new UserDto(user.Id, user.Name, user.Email);
}

// Trate no ponto de entrada (ex: Minimal API)
app.MapGet("/users/{id:guid}", async (Guid id, IUserService svc) =>
{
    var result = await svc.GetUserAsync(id);
    return result.Match(
        onSuccess: dto   => Results.Ok(dto),
        onFailure: error => error.Type switch
        {
            ErrorType.NotFound   => Results.NotFound(new { error.Code, error.Message }),
            ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
            _                    => Results.Problem(error.Message)
        });
});
```

## Desenvolvimento assistido por IA

O Arkn inclui um **servidor Model Context Protocol** que se integra ao Claude, Cursor e GitHub Copilot, dando ao seu assistente de IA conhecimento direto dos padrões do Arkn:

```bash
# Instale a ferramenta MCP
dotnet tool install -g Arkn.MCP
```

Adicione à configuração do seu cliente de IA (exemplo para Claude Desktop):

```json
{
  "mcpServers": {
    "arkn": { "command": "arkn-mcp", "args": [] }
  }
}
```

Uma vez conectado, seu assistente pode:
- Gerar grupos de erros (`scaffold_errors`)
- Criar scaffold de jobs e clientes HTTP
- Validar código contra as regras ARK001–ARK008
- Pesquisar a documentação do Arkn de forma inline

→ Veja o [guia completo do Servidor MCP](/pt-br/mcp) para detalhes de configuração.

## Use os templates dotnet new

```bash
# Instale os templates
dotnet new install Arkn.Templates

# Crie um projeto Minimal API
dotnet new arkn-api -n MyApi

# Crie um Worker com background jobs
dotnet new arkn-job -n MyWorker

# Crie uma class library
dotnet new arkn-lib -n MyLibrary
```
