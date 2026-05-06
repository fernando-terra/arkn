# Apresentando o Arkn

## Apresentando o Arkn: Um Framework .NET Zero-Dependência Construído para Clareza

**Convenções que você lê. Padrões que você impõe.**

_Construindo com Arkn — Parte 1 de 12_

---

Você já esteve nessa situação. Você começa um novo projeto .NET e, antes de escrever uma única linha de lógica de negócio, já está atolado até o pescoço em decisões de framework. Qual ORM? Qual abstração HTTP? Qual biblioteca de logging? Qual biblioteca de retry? Você instala pacotes NuGet que puxam outros pacotes NuGet, e de repente você tem um grafo de dependências que parece um prato de macarrão e um `appsettings.json` com cem chaves que você não entende completamente.

O código que você realmente se importa — o domínio, as regras, a lógica que torna seu produto valioso — fica enterrado sob camadas de cerimônia de framework.

O Arkn foi construído em torno de uma ideia diferente: **o framework precisa merecer cada linha que adiciona**.

---

## O Problema com Frameworks Pesados

O .NET moderno é extraordinariamente poderoso. Mas o ecossistema evoluiu em uma direção onde a conveniência muitas vezes supera a clareza. Você adiciona um pacote para retries HTTP e ele traz o Polly. Você adiciona um agendador de jobs e ele traz o Quartz com suas tabelas de banco de dados e configuração de clustering. Você adiciona logging estruturado e está configurando enrichers do Serilog antes de ter escrito um único modelo de domínio.

Cada uma dessas escolhas é individualmente razoável. Coletivamente, elas criam o que eu chamo de **gravidade do framework** — a tendência do framework de dominar a codebase, tornando difícil enxergar onde o framework termina e sua aplicação começa.

Os sintomas específicos:

- **Exceções como fluxo de controle.** Um registro ausente no banco causa uma `KeyNotFoundException` que se propaga por três camadas antes de ser capturada — ou pior, nunca é capturada.
- **Contratos implícitos.** Seu serviço retorna `User?`. Isso é "não encontrado" ou "ainda não carregado" ou "acesso negado"? Ninguém sabe até o runtime.
- **Lock-in.** Você quer trocar seu cliente HTTP por outro e percebe que cada serviço da sua codebase referencia tipos do RestSharp diretamente.
- **Boilerplate verboso.** A mesma lógica de retry copiada e colada em doze serviços porque a abstração nunca foi construída.

O Arkn é uma resposta a esses sintomas.

---

## Filosofia: Zero Lock-in, Composabilidade, Explicitidade

O Arkn é construído sobre três princípios que guiam cada decisão de API:

**Zero lock-in.** Todo pacote do Arkn depende apenas de outros pacotes Arkn e do .NET BCL. Sem Polly. Sem Serilog. Sem Hangfire. Sem ORMs. Se você quiser substituir o cliente HTTP do Arkn por outro amanhã, seu código de domínio fica intacto — porque ele só conhece `Result<T>`, não `ArknHttp`.

**Composabilidade.** Você instala apenas o que precisa. Usar `Arkn.Results` não te obriga a usar `Arkn.Http`. Usar `Arkn.Jobs` não te obriga a usar `Arkn.Notifications`. Cada pacote é independentemente útil e se integra naturalmente com os outros quando você escolhe combiná-los.

**Explicitidade.** Falhas são valores, não exceções. Toda operação que pode falhar retorna `Result<T>`. Todo erro tem um código legível por máquina, uma mensagem legível por humano e um tipo semântico. Não existe `User?` — existe `Result<User>`, e o compilador te força a tratar ambos os caminhos.

---

## Um Tour Rápido pelos Pacotes

Veja como o ecossistema Arkn está hoje:

| Pacote | Finalidade |
|---|---|
| `Arkn.Core` | Primitivos de domínio: `IEntity`, `IValueObject`, `IAggregateRoot` |
| `Arkn.Results` | `Result<T>`, `Error`, `ErrorType` — a língua franca do framework |
| `Arkn.Http` | Cliente HTTP tipado fluente com retry e timeout integrados |
| `Arkn.Jobs` | Agendador cron com retry, timeout, lock distribuído, DLQ e contratos `Result<T>` |
| `Arkn.Logging` | Logging estruturado com sinks plugáveis e bridge MEL |
| `Arkn.Notifications` | Abstração de notificações plugável |
| `Arkn.Extensions.Notifications.Slack` | Slack via Incoming Webhook + Block Kit, zero deps externas |
| `Arkn.Extensions.Notifications.Email` | Notificador SMTP + SendGrid, zero deps externas |
| `Arkn.Extensions.Notifications.Teams` | Microsoft Teams via Adaptive Cards 1.4, zero deps externas |
| `Arkn.Extensions.Notifications.Discord` | Discord via Embeds + Webhook, zero deps externas |
| `Arkn.Extensions.Logging.ApplicationInsights` | Sink para Application Insights — mapeia `LogEntry` para telemetria automaticamente |
| `Arkn.Extensions.Logging.Seq` | Sink para Seq via CLEF-over-HTTP com batching |
| `Arkn.Extensions.Logging.Elasticsearch` | Sink para Elasticsearch via Bulk API com índices por padrão de data |
| `Arkn.Analyzers` | Analyzers Roslyn (ARK001–ARK008) que impõem padrões Arkn em tempo de compilação |
| `Arkn.SourceGen` | Source generator — elimina boilerplate de fábricas de erros via `[ArknErrors]` |
| `Arkn.MCP` | Servidor MCP nativo — permite que assistentes de IA gerem e validem código Arkn corretamente |
| `Arkn.Templates` | Templates `dotnet new` para projetos de API, Worker e Library |

Cada pacote tem uma responsabilidade clara. Nenhum deles vai te surpreender.

---

## O Teaser do Result

A única ideia que unifica tudo no Arkn é `Result<T>`. Deixa eu mostrar a diferença que ela faz.

Aqui está um método de serviço tradicional:

```csharp
// Abordagem tradicional — o que significa null?
public async Task<User?> GetUserAsync(Guid id)
{
    var user = await _db.Users.FindAsync(id);
    return user; // null = não encontrado? não autorizado? não carregado?
}
```

E aqui o mesmo método com Arkn:

```csharp
// Abordagem Arkn — o tipo conta a história completa
public async Task<Result<User>> GetUserAsync(Guid id)
{
    var user = await _db.Users.FindAsync(id);

    if (user is null)
        return Error.NotFound("User.NotFound", $"User {id} does not exist.");

    return user; // conversão implícita para Result<User>
}
```

No ponto de chamada, o compilador te obriga a tratar ambos os resultados:

```csharp
var result = await userService.GetUserAsync(id);

return result.Match(
    onSuccess: user  => Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
        _                  => Problem(error.Message)
    });
```

Sem nulls escondidos. Sem exceções surpresa. Sem contratos ambíguos. O sistema de tipos faz o trabalho.

Vamos aprofundar o `Result<T>` no próximo post. Por enquanto, apenas observe que ele torna o caminho infeliz tão visível quanto o caminho feliz.

---

## Como Instalar

Todos os pacotes do Arkn têm como alvo .NET 9 e .NET 10. Instale apenas o que precisar:

```bash
# O padrão Result core — comece aqui
dotnet add package Arkn.Results

# Primitivos de domínio
dotnet add package Arkn.Core

# Cliente HTTP tipado fluente
dotnet add package Arkn.Http

# Agendamento de background jobs (com persistência, lock distribuído e DLQ)
dotnet add package Arkn.Jobs

# Logging estruturado
dotnet add package Arkn.Logging

# Abstração de notificações
dotnet add package Arkn.Notifications

# Notificadores (escolha o que precisar)
dotnet add package Arkn.Extensions.Notifications.Slack
dotnet add package Arkn.Extensions.Notifications.Email
dotnet add package Arkn.Extensions.Notifications.Teams
dotnet add package Arkn.Extensions.Notifications.Discord

# Sinks de logging (escolha o que precisar)
dotnet add package Arkn.Extensions.Logging.ApplicationInsights
dotnet add package Arkn.Extensions.Logging.Seq
dotnet add package Arkn.Extensions.Logging.Elasticsearch

# Analyzers Roslyn — imponha padrões Arkn em tempo de compilação (ARK001–ARK008)
dotnet add package Arkn.Analyzers

# Source generator — elimine o boilerplate de fábricas de erros
dotnet add package Arkn.SourceGen

# Servidor MCP — scaffold e validação assistidos por IA
dotnet tool install -g Arkn.MCP
```

Ou, se quiser criar um projeto completo a partir de um template:

```bash
dotnet new install Arkn.Templates

dotnet new arkn-api -n MyApi        # Minimal API com Results + Http
dotnet new arkn-job -n MyWorker     # Worker Service com Jobs + Notifications
dotnet new arkn-lib -n MyLibrary    # Class library com Core + Results
```

---

## O Que Esta Série Cobre

Este é o primeiro post de uma série de doze partes. Aqui está o roadmap:

1. **Apresentando o Arkn** ← você está aqui
2. Pare de Lançar Exceções: O Padrão `Result<T>` com `Arkn.Results`
3. Primitivos de Domínio do Jeito Certo: `Arkn.Core`
4. Clientes HTTP Tipados sem Boilerplate: `Arkn.Http`
5. Logging Estruturado que Realmente Faz Sentido: `Arkn.Logging`
6. Cron Jobs, Retry e Timeout do Jeito Certo: `Arkn.Jobs`
7. Notificações para Slack, Teams, Discord e Email sem SDKs Externos: `Arkn.Notifications`
8. Tornando seu Framework .NET Pronto para IA: `Arkn.Analyzers` e `Arkn.MCP`
9. Zero Boilerplate: Gerando Fábricas de Erros com `Arkn.SourceGen`
10. O Primeiro Framework .NET com Servidor MCP Nativo
11. Arkn v0.3.0: O Que Há de Novo
12. Logging Estruturado sem Serilog: `Arkn.Logging` + Seq + Elasticsearch

Cada post é independente. Se você se importa com clientes HTTP, vá direto para o post 4. Se está construindo um worker service, vá para o post 6. Se quer entender a base primeiro, leia na ordem.

---

## Um Pensamento Final

Frameworks são ferramentas. As melhores ferramentas são aquelas que você para de notar porque se encaixam tão naturalmente no trabalho. O objetivo do Arkn é ser o tipo de ferramenta onde, seis meses depois de iniciar um projeto, você não está lutando contra o framework — você está simplesmente escrevendo código.

Código-fonte, exemplos e documentação estão todos no GitHub: [github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn)

---

_Próximo na série: **Pare de Lançar Exceções: O Padrão `Result<T>` com `Arkn.Results`** — onde vamos fundo na API do `Result<T>`, fábricas de erros, encadeamento funcional e exemplos do mundo real._

_Todos os pacotes disponíveis no NuGet — versão atual: **v0.3.0**._

_Autor: Fernando Terra | [github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn)_
