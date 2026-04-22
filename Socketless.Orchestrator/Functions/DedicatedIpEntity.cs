using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions;

public class DedicatedIpEntity : TaskEntity<DedicatedIpEntityState>
{
    public static EntityInstanceId Id(DedicatedIpId dedicatedIpId) =>
        new(nameof(DedicatedIpEntity), dedicatedIpId.ToString());

    public DedicatedIpEntityState GetState() => State;

    /// <summary>
    /// Set the node this IP is attached to.
    /// </summary>
    public void OnAttached(NodeId nodeId) =>
        State.NodeId = nodeId;

    /// <summary>
    /// Remove the node from this IP when detached.
    /// </summary>
    public void OnDetached() =>
        State.NodeId = null;

    [Function(nameof(DedicatedIpEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<DedicatedIpEntity>();
}

// State
public class DedicatedIpEntityState
{
    public NodeId? NodeId { get; set; } = null;
}

// TODO: Does dedicated IP need an entity, or is an orchestrator that just awaits an event to delete sufficient?
