using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Activities;

namespace Socketless.Orchestrator.Functions.Orchestrators;

[DurableTask]
public class WorkerCreateOrchestrator : TaskOrchestrator<object?, Worker>
{
    public const string InstanceId = nameof(WorkerCreateOrchestrator);

    public override async Task<Worker> RunAsync(TaskOrchestrationContext ctx, object? input)
    {
        var workerId = WorkerId.Parse(ctx.NewGuid().ToString());

        if (ctx.InstanceId != InstanceId)
            throw new InvalidOperationException($"InstanceId must be {InstanceId} to ensure singleton");

        // Provision new worker
        await ctx.CallWorkerCreateActivityAsync(workerId);
        await ctx.CallWorkerProvisionActivityAsync(workerId);
        await ctx.WaitForWorkerProvisionedEventAsync();

        // Wait for worker to become active
        await ctx.WaitForWorkerActiveEventAsync();
        return await ctx.CallWorkerStatusUpdateActivityAsync(new(workerId, WorkerStatus.Active));
    }
}

[DurableEvent]
public sealed record WorkerProvisionedEvent();

[DurableEvent]
public sealed record WorkerActiveEvent();
