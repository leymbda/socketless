using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Activities;
using Socketless.Orchestrator.Functions.Entities;

namespace Socketless.Orchestrator.Functions.Orchestrators;

[DurableTask]
public class NodeLifecycleOrchestrator : TaskOrchestrator<object?, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, object? input)
    {
        var nodeId = NodeId.Parse(ctx.InstanceId);

        // Provision node
        await ctx.CallProvisionNodeActivityAsync(nodeId);
        await ctx.WaitForNodeProvisionedEventAsync();

        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeProvisioned), nodeId);

        // Await application ready
        await ctx.WaitForNodeReadyEventAsync();

        await ctx.Entities.SignalEntityAsync(NodeEntity.Id(nodeId), nameof(NodeEntity.OnReady));
        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeReady), nodeId);

        // Await shutdown
        var shutdownEvent = await ctx.WaitForNodeShutdownEventAsync();

        // Attempt graceful transfer of shards
        bool drained = false;

        if (shutdownEvent.Graceful)
        {
            await ctx.Entities.SignalEntityAsync(NodeEntity.Id(nodeId), nameof(NodeEntity.OnVacating));
            await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeVacating));

            for (int i = 0; i < 3; i++)
            {
                var cts = new CancellationTokenSource();
                await ctx.CreateTimer(ctx.CurrentUtcDateTime.AddSeconds(10), cts.Token);

                var entityState = await ctx.Entities.CallEntityAsync<NodeEntityState>(NodeEntity.Id(nodeId), nameof(NodeEntity.GetState));

                if (entityState.Shards.Count == 0)
                {
                    drained = true;
                    break;
                }
            }
        }

        if (drained)
        {
            // TODO: Graceful
        }
        else
        {
            await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeFaulted));

            // TODO: Ungraceful
        }

        await ctx.Entities.SignalEntityAsync(NodeEntity.Id(nodeId), "delete");
        
        // Deprovision node
        await ctx.CallDeprovisionNodeActivityAsync(nodeId);
        await ctx.WaitForNodeDeprovisionedEventAsync();

        await ctx.Entities.SignalEntityAsync(ClusterManagerEntity.Id(), nameof(ClusterManagerEntity.OnNodeDeprovisioned), nodeId);

        return null;
    }
}

[DurableEvent]
public sealed record NodeProvisionedEvent();

[DurableEvent]
public sealed record NodeReadyEvent();

[DurableEvent]
public sealed record NodeShutdownEvent(bool Graceful);

[DurableEvent]
public sealed record NodeDeprovisionedEvent();
