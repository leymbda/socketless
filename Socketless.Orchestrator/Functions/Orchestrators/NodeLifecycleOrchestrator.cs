using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Activities;

namespace Socketless.Orchestrator.Functions.Orchestrators;

[DurableTask]
public class NodeLifecycleOrchestrator : TaskOrchestrator<NodeLifecycleOrchestratorInput, TimeSpan>
{
    public override async Task<TimeSpan> RunAsync(TaskOrchestrationContext ctx, NodeLifecycleOrchestratorInput input)
    {
        var nodeId = NodeId.Parse(ctx.InstanceId);

        // Provision node
        await ctx.CallProvisionNodeActivityAsync(nodeId);

        // Await application ready
        await ctx.WaitForNodeReadyEventAsync(); // TODO: Notify entities of readiness
        var startTime = ctx.CurrentUtcDateTime;

        // Setup tasks to wait until necessary to shut down
        var unplannedOutageEventTask = ctx.WaitForNodeUnplannedOutageEventAsync();
        var heartbeatTask = Task.CompletedTask; // TODO: Sub-orchestrator for heartbeats, returns result when fails
        
        await Task.WhenAny(unplannedOutageEventTask, heartbeatTask); // TODO: Notify entities of shutdown

        // Deprovision node
        await ctx.CallDeprovisionNodeActivityAsync(nodeId); // TODO: Migrate shards off node before deprovisioning

        return ctx.CurrentUtcDateTime - startTime;

        // TODO: Support graceful planned outages/vacation (azure scheduled events)
    }
}

public record NodeLifecycleOrchestratorInput();

[DurableEvent]
public sealed record NodeReadyEvent();

[DurableEvent]
public sealed record NodeUnplannedOutageEvent();
