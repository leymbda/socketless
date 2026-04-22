using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Services;

namespace Socketless.Orchestrator.Functions;

public class ClusterManagerEntity : TaskEntity<ClusterManagerEntityState>
{
    public static EntityInstanceId Id() =>
        new(nameof(ClusterManagerEntity), "global");

    public ClusterManagerEntityState GetState() => State;

    /// <summary>
    /// Attempt to assign shards to a node, preferring the provided node if possible. Returns the ID of the node the
    /// shards were assigned to, or null if no suitable node was found.
    /// </summary>
    public NodeId? AssignShards(ClusterManagerEntityAssignShardsInput input)
    {
        if (input.PreferredNodeId is NodeId id && State.Nodes.TryGetValue(id, out var node))
        {
            if (node.AcceptingNewShards && NodePlacementService.CanAccomodate(node.ShardCount, input.Count, node.ShardCost, input.Cost))
                return id;
        }

        return State.Nodes
            .Where(n => n.Value.AcceptingNewShards && NodePlacementService.CanAccomodate(n.Value.ShardCount, input.Count, n.Value.ShardCost, input.Cost))
            .OrderBy(n => n.Value.ShardCount)
            .Select(n => n.Key)
            .FirstOrDefault();
    }

    /// <summary>
    /// Release the given amounts of load from the given node.
    /// </summary>
    public void ReleaseShards(ClusterManagerEntityReleaseShardsInput input)
    {
        if (State.Nodes.TryGetValue(input.NodeId, out var entry))
            State.Nodes[input.NodeId] = entry with
            {
                ShardCount = entry.ShardCount - input.Count,
                ShardCost = entry.ShardCost - input.Cost,
            };
    }

    /// <summary>
    /// Sync the amounts of load from the given node, replacing the current values with the provided ones.
    /// </summary>
    public void SyncNode(ClusterManagerEntitySyncNodeShardsInput input)
    {
        if (State.Nodes.TryGetValue(input.NodeId, out var entry))
            State.Nodes[input.NodeId] = entry with { ShardCount = input.Count, ShardCost = input.Cost };
    }

    /// <summary>
    /// Add an unready node to the cluster state when node provisioned.
    /// </summary>
    public void OnNodeProvisioned(NodeId nodeId) =>
        State.Nodes[nodeId] = new ClusterManagerNodeEntry(false, 0, 0);

    /// <summary>
    /// Mark a node as readay to accept shards when node is ready.
    /// </summary>
    public void OnNodeReady(NodeId nodeId)
    {
        if (State.Nodes.TryGetValue(nodeId, out var entry))
            State.Nodes[nodeId] = entry with { AcceptingNewShards = true };
    }

    /// <summary>
    /// Stop accepting new shards on a node that is vacating.
    /// </summary>
    public void OnNodeVacating(NodeId nodeId)
    {
        if (State.Nodes.TryGetValue(nodeId, out var entry))
            State.Nodes[nodeId] = entry with { AcceptingNewShards = false };
    }

    /// <summary>
    /// Stop accepting new shards on a node that is faulted.
    /// </summary>
    public void OnNodeFaulted(NodeId nodeId)
    {
        if (State.Nodes.TryGetValue(nodeId, out var entry))
            State.Nodes[nodeId] = entry with { AcceptingNewShards = false };
    }

    /// <summary>
    /// Remove nodes from the cluster state when deprovisioned.
    /// </summary>
    public void OnNodeDeprovisioned(NodeId nodeId) =>
        State.Nodes.Remove(nodeId);

    [Function(nameof(ClusterManagerEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<ClusterManagerEntity>();
}

// State
public class ClusterManagerEntityState
{
    public Dictionary<NodeId, ClusterManagerNodeEntry> Nodes { get; set; } = [];
}

public record ClusterManagerNodeEntry(
    bool AcceptingNewShards,
    int ShardCount,
    float ShardCost);

// Inputs
public record ClusterManagerEntityAssignShardsInput(NodeId? PreferredNodeId, int Count, float Cost);

public record ClusterManagerEntityReleaseShardsInput(NodeId NodeId, int Count, float Cost);

public record ClusterManagerEntitySyncNodeShardsInput(NodeId NodeId, int Count, float Cost);
