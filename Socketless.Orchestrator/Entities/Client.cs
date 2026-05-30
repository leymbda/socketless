namespace Socketless.Orchestrator.Entities;

public class Client(ClientId id, ClientIpTier ipTier)
{
    public ClientId Id { get; } = id;

    public ClientIpTier IpTier { get; private set; } = ipTier;

    public void SetIpTier(ClientIpTier ipTier)
    {
        if (!Enum.IsDefined(ipTier))
            throw new InvalidOperationException($"IP tier must be a defined enum value");

        IpTier = ipTier;
    }
}

public readonly record struct ClientId(Guid Value) : IParsable<ClientId>
{
    public static ClientId New() => new(Guid.NewGuid());

    public static ClientId Parse(string value, IFormatProvider? format = null)
    {
        var guid = Guid.Parse(value);
        return new(guid);
    }

    public static bool TryParse(string? value, IFormatProvider? format, out ClientId result)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            result = Parse(value, format);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public override string ToString() => Value.ToString();
}

public enum ClientIpTier
{
    Shared,
    Dedicated,
}
