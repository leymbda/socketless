using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions;

public class AppEntity : TaskEntity<AppEntityState>
{
    public static EntityInstanceId Id(AppId appId) =>
        new(nameof(AppEntity), appId.ToString());

    public AppEntityState GetState() => State;

    public void UpdateTargetShardCount(int targetShardCount) =>
        State.TargetShardCount = targetShardCount;

    public void UpdateNodeShardCount(NodeId nodeId, int reportedCount)
    {
        if (reportedCount == 0) State.NodeShardCounts.Remove(nodeId);
        else State.NodeShardCounts[nodeId] = reportedCount;
    }

    public void OnNodeVacated(NodeId nodeId) =>
        State.NodeShardCounts.Remove(nodeId);

    public void OnDedicatedIpAssigned(AppEntityDedicatedIpId dedicatedIp) =>
        State.IpAddresses.Add(dedicatedIp);

    public void OnDedicatedIpRemoved(AppEntityDedicatedIpId dedicatedIp) =>
        State.IpAddresses.Remove(dedicatedIp);

    [Function(nameof(AppEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<AppEntity>();
}

public class AppEntityState
{
    public int TargetShardCount { get; set; } = 0;

    public Dictionary<NodeId, int> NodeShardCounts { get; set; } = [];

    public HashSet<AppEntityDedicatedIpId> IpAddresses { get; set; } = [];
}

public record AppEntityDedicatedIpId(DedicatedIpId IpId, NodeId NodeId);
