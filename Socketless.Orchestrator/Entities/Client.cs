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

public readonly record struct ClientId(Guid Value)
{
    public static ClientId New() => new(Guid.NewGuid());

    public static ClientId Parse(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}

public enum ClientIpTier
{
    Shared,
    Dedicated,
}
