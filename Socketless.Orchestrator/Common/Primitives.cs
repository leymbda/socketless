namespace Socketless.Orchestrator.Common;

public readonly record struct Snowflake(ulong Value)
{
    public static Snowflake Parse(ulong Value) => new(Value);

    public static Snowflake Parse(string value) => new(ulong.Parse(value));

    public override string ToString() => Value.ToString();
}
