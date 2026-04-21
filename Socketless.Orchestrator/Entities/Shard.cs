using Socketless.Orchestrator.Common;
using System.Net;

namespace Socketless.Orchestrator.Entities;

public class Shard(
    ShardId id,
    ShardState state,
    int applicationGuildCount,
    IPAddress publicIpAddress,
    DateTime createdAt)
{
    private int _applicationGuildCount = applicationGuildCount;

    public ShardId Id { get; } = id;

    public ShardState State { get; private set; } = state;

    public IPAddress PublicIpAddress { get; } = publicIpAddress;

    public DateTime CreatedAt { get; } = createdAt;

    public float Cost => (float)_applicationGuildCount / Id.ShardCount / 150 + 1;

    /// <summary>
    /// Set the crurent state of the shard.
    /// </summary>
    /// <param name="state">The new state to set</param>
    /// <exception cref="ArgumentOutOfRangeException">Occurs if the given state is not a defined enum value</exception>
    public void SetState(ShardState state)
    {
        if (!Enum.IsDefined(state))
            throw new ArgumentOutOfRangeException(nameof(state), "State must be a defined enum value");

        State = state;
    }

    /// <summary>
    /// Updates the number of guilds the application is handling across shards. Used to approximate the cost of this shard.
    /// </summary>
    /// <param name="applicationGuildCount">The total guilds the application is installed on, across all shards</param>
    /// <exception cref="ArgumentOutOfRangeException">Occurs if the given guild count is negative</exception>
    public void UpdateApplicationGuildCount(int applicationGuildCount)
    {
        if (applicationGuildCount < 0)
            throw new ArgumentOutOfRangeException(nameof(applicationGuildCount), "Application guild count cannot be negative.");
        
        _applicationGuildCount = applicationGuildCount;
    }
}

public readonly record struct ShardId(Snowflake Snowflake, ushort ShardIndex, ushort ShardCount);

// TODO: parse/toString for ShardId

public enum ShardState
{
    Connecting,
    Identifying,
    Resuming,
    Ready,
    Reconnecting,
    Disconnected,
}

// TODO: Shouldn't be concerned with specific IP addresses
