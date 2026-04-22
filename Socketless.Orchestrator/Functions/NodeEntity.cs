using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions;

public class NodeEntity : TaskEntity<NodeEntityState>
{
    public static EntityInstanceId Id(NodeId nodeId) =>
        new(nameof(NodeEntity), nodeId.ToString());

    public NodeEntityState GetState() => State;

    /// <summary>
    /// Add a new shard to the node.
    /// </summary>
    public void AddShard(ShardId shardId) =>
        State.Shards.Add(shardId);

    /// <summary>
    /// Remove an existing shard from the node.
    /// </summary>
    public void RemoveShard(ShardId shardId) =>
        State.Shards.Remove(shardId);

    /// <summary>
    /// Update node status when node is ready.
    /// </summary>
    public void OnReady() =>
        State.Status = NodeEntityStatus.Active;

    /// <summary>
    /// Update node status when node is vacating.
    /// </summary>
    public void OnVacating() =>
        State.Status = NodeEntityStatus.Vacating;

    /// <summary>
    /// Update node status when node is faulted.
    /// </summary>
    public void OnFaulted() =>
        State.Status = NodeEntityStatus.Faulted;

    /// <summary>
    /// Add dedicated IP to node when attached.
    /// </summary>
    public void OnDedicatedIpAttached(DedicatedIpId dedicatedIpId) =>
        State.IpAddresses.Add(dedicatedIpId);

    /// <summary>
    /// Remove dedicated IP from node when detached.
    /// </summary>
    public void OnDedicatedIpDetached(DedicatedIpId dedicatedIpId) =>
        State.IpAddresses.Remove(dedicatedIpId);

    [Function(nameof(NodeEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<NodeEntity>();

}

// State
public class NodeEntityState
{
    public NodeEntityStatus Status { get; set; } = NodeEntityStatus.Initializing;

    public HashSet<ShardId> Shards { get; set; } = [];

    public HashSet<DedicatedIpId> IpAddresses { get; set; } = [];
}

public enum NodeEntityStatus
{
    Initializing,
    Active,
    Vacating,
    Faulted,
}
