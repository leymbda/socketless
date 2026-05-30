using Socketless.Orchestrator.Common;

namespace Socketless.Orchestrator.Entities;

public class App(AppId id, AppIpTier ipTier)
{
    public AppId Id { get; } = id;

    public AppIpTier IpTier { get; private set; } = ipTier;

    public void SetIpTier(AppIpTier ipTier)
    {
        if (!Enum.IsDefined(ipTier))
            throw new InvalidOperationException($"IP tier must be a defined enum value");

        IpTier = ipTier;
    }
}

public readonly record struct AppId(Snowflake Value) : IParsable<AppId>
{
    public static AppId Parse(string value, IFormatProvider? format = null)
    {
        var snowflake = Snowflake.Parse(value);
        return new(snowflake);
    }

    public static bool TryParse(string? value, IFormatProvider? format, out AppId result)
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

public enum AppIpTier
{
    Shared,
    ClientShared,
    Dedicated,
}
