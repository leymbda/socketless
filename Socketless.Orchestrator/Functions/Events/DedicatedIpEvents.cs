using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Entities;

namespace Socketless.Orchestrator.Functions.Events;

public static class DedicatedIpEvents
{
    [Function(nameof(DedicatedIpProvisionedEventGridEvent))]
    public static async Task DedicatedIpProvisionedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var ipId = DedicatedIpId.Parse(ev.Subject);

        await durableClient.ScheduleNewDedicatedIpProvisionedEventOrchestratorInstanceAsync(new(ipId));
    }

    [Function(nameof(DedicatedIpDeprovisionedEventGridEvent))]
    public static async Task DedicatedIpDeprovisionedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var ipId = DedicatedIpId.Parse(ev.Subject);

        await durableClient.ScheduleNewDedicatedIpDeprovisionedEventOrchestratorInstanceAsync(new(ipId));
    }

    [Function(nameof(DedicatedIpAttachedEventGridEvent))]
    public static async Task DedicatedIpAttachedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var ipId = DedicatedIpId.Parse(ev.Subject);
        var data = ev.Data.ToObjectFromJson<DedicatedIpAttachedEventData>()!;

        await durableClient.ScheduleNewDedicatedIpAttachedEventOrchestratorInstanceAsync(new(ipId, data.NodeId, data.ClientId, data.AppIds));
    }

    [Function(nameof(DedicatedIpDetachedEventGridEvent))]
    public static async Task DedicatedIpDetachedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        var ipId = DedicatedIpId.Parse(ev.Subject);
        var data = ev.Data.ToObjectFromJson<DedicatedIpDetachedEventData>()!;

        await durableClient.ScheduleNewDedicatedIpDetachedEventOrchestratorInstanceAsync(new(ipId, data.NodeId, data.ClientId, data.AppIds));
    }
}

public record DedicatedIpAttachedEventData(NodeId NodeId, ClientId ClientId, IEnumerable<AppId> AppIds);

public record DedicatedIpDetachedEventData(NodeId NodeId, ClientId ClientId, IEnumerable<AppId> AppIds);

// TODO: Above event data probably shouldnt contain the client and app IDs, these should probably be grabbed from getting entity states in orchestrators below

[DurableTask]
public class DedicatedIpProvisionedEventOrchestrator : TaskOrchestrator<DedicatedIpProvisionedEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, DedicatedIpProvisionedEventOrchestratorInput input)
    {
        // TODO: Will definitely need to do stuff here

        return null;
    }
}

public record DedicatedIpProvisionedEventOrchestratorInput(DedicatedIpId IpId);

[DurableTask]
public class DedicatedIpDeprovisionedEventOrchestrator : TaskOrchestrator<DedicatedIpDeprovisionedEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, DedicatedIpDeprovisionedEventOrchestratorInput input)
    {
        // TODO: Will definitely need to do stuff here

        return null;
    }
}

public record DedicatedIpDeprovisionedEventOrchestratorInput(DedicatedIpId IpId);

[DurableTask]
public class DedicatedIpAttachedEventOrchestrator : TaskOrchestrator<DedicatedIpAttachedEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, DedicatedIpAttachedEventOrchestratorInput input)
    {
        await ctx.Entities.SignalEntityAsync(DedicatedIpEntity.Id(input.IpId), nameof(DedicatedIpEntity.OnAttached), input.NodeId);
        await ctx.Entities.SignalEntityAsync(NodeEntity.Id(input.NodeId), nameof(NodeEntity.OnDedicatedIpAttached), input.IpId);
        await ctx.Entities.SignalEntityAsync(ClientEntity.Id(input.ClientId), nameof(ClientEntity.OnDedicatedIpAttached), new ClientEntityOnDedicatedIpAttachedInput(input.IpId, input.NodeId));

        foreach (var appId in input.AppIds)
            await ctx.Entities.SignalEntityAsync(AppEntity.Id(appId), nameof(AppEntity.OnDedicatedIpAttached), new AppEntityOnDedicatedIpAttachedInput(input.IpId, input.NodeId));

        return null;
    }
}

public record DedicatedIpAttachedEventOrchestratorInput(
    DedicatedIpId IpId,
    NodeId NodeId,
    ClientId ClientId,
    IEnumerable<AppId> AppIds);

[DurableTask]
public class DedicatedIpDetachedEventOrchestrator : TaskOrchestrator<DedicatedIpDetachedEventOrchestratorInput, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, DedicatedIpDetachedEventOrchestratorInput input)
    {
        await ctx.Entities.SignalEntityAsync(DedicatedIpEntity.Id(input.IpId), nameof(DedicatedIpEntity.OnDetached));
        await ctx.Entities.SignalEntityAsync(NodeEntity.Id(input.NodeId), nameof(NodeEntity.OnDedicatedIpDetached), input.IpId);
        await ctx.Entities.SignalEntityAsync(ClientEntity.Id(input.ClientId), nameof(ClientEntity.OnDedicatedIpDetached), new ClientEntityOnDedicatedIpDetachedInput(input.IpId, input.NodeId));

        foreach (var appId in input.AppIds)
            await ctx.Entities.SignalEntityAsync(AppEntity.Id(appId), nameof(AppEntity.OnDedicatedIpDetached), new AppEntityOnDedicatedIpDetachedInput(input.IpId, input.NodeId));

        return null;
    }
}

public record DedicatedIpDetachedEventOrchestratorInput(
    DedicatedIpId IpId,
    NodeId NodeId,
    ClientId ClientId,
    IEnumerable<AppId> AppIds);

// TODO: Entity locks?
// TODO: Should these orchestrators actually manage all entity events etc or should the lifecycle orchestrators etc instead?
