using Arkn.Notifications.Extensions;

namespace Arkn.Extensions.Notifications.Email.Extensions;

/// <summary>Extension methods for registering email notifiers with <see cref="ArknNotificationsBuilder"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the SMTP email notifier.</summary>
    public static ArknNotificationsBuilder AddSmtpEmailNotifier(
        this ArknNotificationsBuilder builder,
        Action<SmtpEmailOptions> configure)
    {
        var opts = new SmtpEmailOptions();
        configure(opts);
        builder.AddNotifier(new SmtpEmailNotifier(opts));
        return builder;
    }

    /// <summary>Registers the SendGrid HTTP email notifier (no SDK required).</summary>
    public static ArknNotificationsBuilder AddSendGridNotifier(
        this ArknNotificationsBuilder builder,
        Action<SendGridEmailOptions> configure)
    {
        var opts = new SendGridEmailOptions();
        configure(opts);
        builder.AddNotifier(new SendGridEmailNotifier(opts));
        return builder;
    }
}
