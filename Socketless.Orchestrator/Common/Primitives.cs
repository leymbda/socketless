namespace Socketless.Orchestrator.Common;

public readonly record struct Snowflake(ulong Value) : IParsable<Snowflake>
{
    public static Snowflake Parse(string value, IFormatProvider? format = null)
    {
        var num = ulong.Parse(value);
        return new(num);
    }

    public static bool TryParse(string? value, IFormatProvider? format, out Snowflake result)
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

public readonly record struct AppToken(string Value) : IParsable<AppToken>
{
    public static AppToken Parse(string value, IFormatProvider? format = null)
    {
        // TODO: Validate token (if any reasonable validation can be done)

        return new(value);
    }

    public static bool TryParse(string? value, IFormatProvider? format, out AppToken result)
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

    public override string ToString() => Value;
}
