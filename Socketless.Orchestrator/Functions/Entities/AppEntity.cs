using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions.Entities;

[DurableTask]
public class AppEntity : TaskEntity<AppEntityState>
{
    public static EntityInstanceId Id(AppId appId) =>
        new(nameof(AppEntity), appId.ToString());

    public Task<AppEntityState> GetState() => Task.FromResult(State);

    /// <summary>
    /// Set the ID of the primary node in use for this entity. A node may have multiple active nodes during a migration.
    /// </summary>
    public void SetPrimaryNodeId(NodeId nodeId) =>
        State.PrimaryNodeId = nodeId;

    /// <summary>
    /// Update the target shard count for this entity, used for calculating cost involved in shard placement.
    /// </summary>
    public void SetTargetShardCount(int targetShardCount) =>
        State.TargetShardCount = targetShardCount;

    /// <summary>
    /// Update the current state of where this app is running. It will be across multiple nodes during a migration.
    /// </summary>
    public void UpdateNodeShardCount(AppEntityUpdateNodeShardCountInput input)
    {
        if (input.Count == 0) State.NodeShardCounts.Remove(input.NodeId);
        else State.NodeShardCounts[input.NodeId] = input.Count;
    }

    /// <summary>
    /// Update state to reflect nodes no longer running this app due to vacation.
    /// </summary>
    public void OnNodeVacated(NodeId nodeId) =>
        State.NodeShardCounts.Remove(nodeId); // TODO: App should not be concerned with vacation, this should just be a removal type of event

    /// <summary>
    /// Update state to reflect an attached dedicated IP being used for this app.
    /// </summary>
    public void OnDedicatedIpAttached(AppEntityOnDedicatedIpAttachedInput input) =>
        State.IpAddresses.Add(new AppEntityDedicatedIpId(input.IpId, input.NodeId));

    /// <summary>
    /// Update state to reflect a detached dedicated IP that was previously being used for this app.
    /// </summary>
    public void OnDedicatedIpDetached(AppEntityOnDedicatedIpDetachedInput input) =>
        State.IpAddresses.Remove(new AppEntityDedicatedIpId(input.IpId, input.NodeId));
}

public class AppEntityState
{
    public NodeId? PrimaryNodeId { get; set; } = null;

    public int TargetShardCount { get; set; } = 0;

    public Dictionary<NodeId, int> NodeShardCounts { get; set; } = [];

    public HashSet<AppEntityDedicatedIpId> IpAddresses { get; set; } = [];
}

public record AppEntityDedicatedIpId(DedicatedIpId IpId, NodeId NodeId);

public record AppEntityOnDedicatedIpAttachedInput(DedicatedIpId IpId, NodeId NodeId);

public record AppEntityOnDedicatedIpDetachedInput(DedicatedIpId IpId, NodeId NodeId);

public record AppEntityUpdateNodeShardCountInput(NodeId NodeId, int Count);
