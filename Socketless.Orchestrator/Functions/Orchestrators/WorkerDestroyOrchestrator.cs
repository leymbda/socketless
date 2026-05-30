using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Activities;

namespace Socketless.Orchestrator.Functions.Orchestrators;

[DurableTask]
public class WorkerDestroyOrchestrator : TaskOrchestrator<object?, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, object? input)
    {
        var orchestrationId = WorkerDestroyOrchestratorId.Parse(ctx.InstanceId);
        var workerId = orchestrationId.WorkerId;

        // Migrate shards off worker if any running
        await ctx.CallWorkerStatusUpdateActivityAsync(new(workerId, WorkerStatus.Migrating));

        // TODO: Orchestrate migration

        // Deprovision worker
        await ctx.CallWorkerDeprovisionActivityAsync(workerId);
        await ctx.WaitForWorkerDeprovisionedEventAsync();
        await ctx.CallWorkerDeleteActivityAsync(workerId);

        return null;
    }
}

public readonly record struct WorkerDestroyOrchestratorId(WorkerId WorkerId)
{
    public const string Prefix = nameof(WorkerDestroyOrchestratorId) + "-";

    public static WorkerDestroyOrchestratorId Parse(string value)
    {
        if (!value.StartsWith(Prefix))
            throw new ArgumentException($"Invalid format for {nameof(WorkerDestroyOrchestratorId)}: {value}");

        var unprefixed = value[Prefix.Length..];
        var workerId = WorkerId.Parse(unprefixed);
        return new WorkerDestroyOrchestratorId(workerId);
    }

    public override string ToString() => Prefix + WorkerId.ToString();
}

[DurableEvent]
public sealed record WorkerDeprovisionedEvent();
