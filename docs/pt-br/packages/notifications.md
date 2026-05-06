# Arkn.Notifications

Abstrações de notificação plugáveis — fan-out para N destinos.

```bash
dotnet add package Arkn.Notifications
dotnet add package Arkn.Extensions.Notifications.Slack
dotnet add package Arkn.Extensions.Notifications.Email
dotnet add package Arkn.Extensions.Notifications.Teams   # v0.3.0+
dotnet add package Arkn.Extensions.Notifications.Discord # v0.3.0+
```

## Configuração

```csharp
builder.Services.AddArknNotifications(n =>
{
    n.AddSlackNotifier(slack =>
    {
        slack.WebhookUrl   = config["Slack:WebhookUrl"];
        slack.Channel      = "#alerts";
        slack.Username     = "Arkn";
        slack.MinimumLevel = NotificationLevel.Warning;
    });

    n.AddTeamsNotifier(teams =>
    {
        teams.WebhookUrl   = config["Teams:WebhookUrl"];
        teams.MinimumLevel = NotificationLevel.Warning;
    });

    n.AddDiscordNotifier(discord =>
    {
        discord.WebhookUrl   = config["Discord:WebhookUrl"];
        discord.Username     = "Arkn";
        discord.MinimumLevel = NotificationLevel.Warning;
    });

    n.AddSmtpEmailNotifier(smtp =>
    {
        smtp.Host = "smtp.example.com";
        smtp.Port = 587;
        smtp.From = new EmailAddress("noreply@example.com", "Arkn");
        smtp.To   = [new EmailAddress("ops@example.com")];
    });
});
```

## Microsoft Teams — Adaptive Cards <Badge type="tip" text="v0.3.0" />

```bash
dotnet add package Arkn.Extensions.Notifications.Teams
```

Utiliza **Adaptive Cards 1.4** via Incoming Webhook — sem Teams SDK.

- Container de cabeçalho colorido por nível (`good` / `warning` / `attention`)
- Renderização de `FactSet` para metadados de job (RunId, Duration, etc.)
- Bloco de trecho de log monoespaçado
- Rodapé com tag de origem e timestamp UTC

**Configuração do webhook:** Canal do Teams → Conectores → Incoming Webhook → Configurar → copie a URL.

```csharp
n.AddTeamsNotifier(opts =>
{
    opts.WebhookUrl   = "https://your-org.webhook.office.com/...";
    opts.MinimumLevel = NotificationLevel.Warning;
    opts.MaxLogLines  = 5;
    opts.Timeout      = TimeSpan.FromSeconds(10);
});
```

## Discord — Embeds <Badge type="tip" text="v0.3.0" />

```bash
dotnet add package Arkn.Extensions.Notifications.Discord
```

Utiliza **Discord Webhook Embeds** — sem Discord SDK.

- Barra lateral colorida por nível (verde / âmbar / vermelho / vermelho escuro)
- Campos de metadados inline (RunId, Duration, …)
- Trecho de log em bloco de código com máximo de linhas configurável
- Timestamp ISO 8601 e rodapé com tag de origem
- Override opcional de username e avatar URL do bot

**Configuração do webhook:** Configurações do canal Discord → Integrações → Webhooks → Novo Webhook → copie a URL.

```csharp
n.AddDiscordNotifier(opts =>
{
    opts.WebhookUrl   = "https://discord.com/api/webhooks/...";
    opts.Username     = "Arkn";
    opts.AvatarUrl    = "https://example.com/arkn-avatar.png";
    opts.MinimumLevel = NotificationLevel.Warning;
    opts.MaxLogLines  = 5;
});
```

## Envio manual

```csharp
public class AlertService(IArknNotifierRegistry notifier)
{
    public async Task SendAlertAsync(string title, string body)
    {
        await notifier.DispatchAsync(
            ArknNotification.Error(title, body, "MyService"));
    }
}
```

## Notificador customizado

```csharp
public sealed class PagerDutyNotifier : IArknNotifier
{
    public async Task NotifyAsync(ArknNotification notification, CancellationToken ct = default)
    {
        // POST para PagerDuty Events API v2
    }
}

// Registro
builder.Services.AddArknNotifications(n => n.AddNotifier<PagerDutyNotifier>());
```

## Níveis de notificação

| Nível | Valor | Mínimo padrão |
|---|---|---|
| `Info` | 0 | — |
| `Warning` | 1 | ✅ Maioria dos notificadores usa este |
| `Error` | 2 | — |
| `Critical` | 3 | — |

## Integração com Arkn.Jobs

```csharp
jobs.Add<InvoiceProcessorJob>("0 2 * * *")
    .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

// Fallback global — dispara em qualquer falha não tratada de job
jobs.OnFailure<SlackNotifier>();
```
