namespace Arkn.Extensions.Notifications.Email;

/// <summary>An email address with an optional display name.</summary>
public sealed record EmailAddress(string Address, string? DisplayName = null)
{
    public override string ToString() =>
        DisplayName is not null ? $"{DisplayName} <{Address}>" : Address;
}
