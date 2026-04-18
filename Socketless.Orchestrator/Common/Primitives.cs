namespace Socketless.Orchestrator.Common;

public readonly record struct Snowflake(ulong Value);

public readonly record struct ShardId(Snowflake Snowflake, ushort ShardIndex, ushort ShardCount);

public readonly record struct InstanceId(Guid Value);
