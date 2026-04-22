using Socketless.Orchestrator.Common;

namespace Socketless.Orchestrator.Entities;

public readonly record struct ShardId(Snowflake Snowflake, ushort ShardIndex, ushort ShardCount)
{
    public static ShardId Parse(string value)
    {
        var parts = value.Split(':');

        if (parts.Length != 3)
            throw new FormatException("Invalid shard ID format. Expected format: {snowflake}:{shardIndex}:{shardCount}");
    
        return new ShardId(Snowflake.Parse(parts[0]), ushort.Parse(parts[1]), ushort.Parse(parts[2]));
    }
    
    public override string ToString() => $"{Snowflake}:{ShardIndex}:{ShardCount}";
}

public enum ShardState
{
    Connecting,
    Identifying,
    Resuming,
    Ready,
    Reconnecting,
    Disconnected,
}
