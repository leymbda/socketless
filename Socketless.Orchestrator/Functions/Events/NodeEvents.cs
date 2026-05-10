using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Entities;
using Socketless.Orchestrator.Functions.Orchestrators;

namespace Socketless.Orchestrator.Functions.Events;

public static class NodeEvents
{
    [Function(nameof(NodeProvisionedEventGridEvent))]
    public static async Task NodeProvisionedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var nodeId = NodeId.Parse(ev.Subject);

        await durableClient.ScheduleNewNodeProvisionedEventOrchestratorInstanceAsync(new(nodeId));
    }

    [Function(nameof(NodeDeprovisionedEventGridEvent))]
    public static async Task NodeDeprovisionedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var nodeId = NodeId.Parse(ev.Subject);

        await durableClient.ScheduleNewNodeDeprovisionedEventOrchestratorInstanceAsync(new(nodeId));
    }

    [Function(nameof(NodeReadyEventGridEvent))]
    public static async Task NodeReadyEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var nodeId = NodeId.Parse(ev.Subject);

        await durableClient.ScheduleNewNodeReadyEventOrchestratorInstanceAsync(new(nodeId));
    }

    [Function(nameof(NodeFaultedEventGridEvent))]
    public static async Task NodeFaultedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var nodeId = NodeId.Parse(ev.Subject);

        await durableClient.ScheduleNewNodeFaultedEventOrchestratorInstanceAsync(new(nodeId));
    }

    [Function(nameof(NodeVacatingEventGridEvent))]
    public static async Task NodeVacatingEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var nodeId = NodeId.Parse(ev.Subject);

        await durableClient.ScheduleNewNodeVacatingEventOrchestratorInstanceAsync(new(nodeId));
    }
}

[DurableTask]
public class NodeProvisionedEventOrchestrator : TaskOrchestrator<NodeProvisionedEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, NodeProvisionedEventOrchestratorInput input)
    {
        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeProvisioned), input.NodeId);
        
        return null;
    }
}

public record NodeProvisionedEventOrchestratorInput(NodeId NodeId);

[DurableTask]
public class NodeDeprovisionedEventOrchestrator : TaskOrchestrator<NodeDeprovisionedEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, NodeDeprovisionedEventOrchestratorInput input)
    {
        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeDeprovisioned), input.NodeId);
        
        return null;
    }
}

public record NodeDeprovisionedEventOrchestratorInput(NodeId NodeId);

[DurableTask]
public class NodeReadyEventOrchestrator : TaskOrchestrator<NodeReadyEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, NodeReadyEventOrchestratorInput input)
    {
        ctx.SendNodeReadyEvent(input.NodeId.ToString(), new NodeReadyEvent());
        await ctx.Entities.SignalEntityAsync(NodeEntity.Id(input.NodeId), nameof(NodeEntity.OnReady));
        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeReady), input.NodeId);

        return null;
    }
}

public record NodeReadyEventOrchestratorInput(NodeId NodeId);

[DurableTask]
public class NodeFaultedEventOrchestrator : TaskOrchestrator<NodeFaultedEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, NodeFaultedEventOrchestratorInput input)
    {
        ctx.SendNodeUnplannedOutageEvent(input.NodeId.ToString(), new NodeUnplannedOutageEvent());
        await ctx.Entities.SignalEntityAsync(NodeEntity.Id(input.NodeId), nameof(NodeEntity.OnFaulted));
        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeFaulted), input.NodeId);

        return null;
    }
}

public record NodeFaultedEventOrchestratorInput(NodeId NodeId);

[DurableTask]
public class NodeVacatingEventOrchestrator : TaskOrchestrator<NodeVacatingEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, NodeVacatingEventOrchestratorInput input)
    {
        await ctx.Entities.SignalEntityAsync(NodeEntity.Id(input.NodeId), nameof(NodeEntity.OnVacating));
        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeVacating), input.NodeId);

        return null;
    }
}

public record NodeVacatingEventOrchestratorInput(NodeId NodeId);

// TODO: Entity locks?
// TODO: Should these orchestrators actually manage all entity events etc or should the lifecycle orchestrators etc instead?
