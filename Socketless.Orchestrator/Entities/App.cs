using Socketless.Orchestrator.Common;

namespace Socketless.Orchestrator.Entities;

public class App(AppId id)
{
    public AppId Id { get; } = id;

    public AppIpTier IpTier { get; private set; } = AppIpTier.Shared;

    public void SetIpTier(AppIpTier ipTier)
    {
        if (!Enum.IsDefined(ipTier))
            throw new InvalidOperationException($"IP tier must be a defined enum value");

        IpTier = ipTier;
    }
}

public readonly record struct AppId(Snowflake Value)
{
    public static AppId Parse(string value) => new(Snowflake.Parse(value));

    public override string ToString() => Value.ToString();
}

public enum AppIpTier
{
    Shared,
    ClientShared,
    Dedicated,
}
