namespace Socketless.Orchestrator.Entities;

public class ShardInstance(ShardInstanceId id, ShardInstanceStatus status, WorkerId workerId, float cost)
{
    public ShardInstanceId Id { get; } = id;

    public ShardInstanceStatus Status { get; set; } = status;

    public WorkerId WorkerId { get; } = workerId;

    public float Cost { get; set; } = cost;
}

public readonly record struct ShardInstanceId(ShardId ShardId, Guid Identifier)
{
    public static ShardInstanceId New(ShardId shardId) => new(shardId, Guid.NewGuid());

    public static ShardInstanceId Parse(string value, IFormatProvider? format = null)
    {
        try
        {
            var parts = value.Split(':').ToList();
            var identifier = Guid.Parse(parts.Last());

            parts.RemoveAt(parts.Count - 1);
            var shardId = ShardId.Parse(string.Join(':', parts));

            return new(shardId, identifier);
        }
        catch (Exception ex)
        {
            throw new FormatException("Invalid shard instance ID format. Expected format: {shardId}:{guid}", ex);
        }
    }

    public static bool TryParse(string? value, IFormatProvider? format, out ShardInstanceId result)
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

    public override string ToString() => $"{ShardId}:{Identifier}";
}

public enum ShardInstanceStatus
{
    Starting,
    Active,
    Migrating,
    Stopping,
}
