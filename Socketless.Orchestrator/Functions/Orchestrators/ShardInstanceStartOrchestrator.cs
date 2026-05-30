using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Activities;

namespace Socketless.Orchestrator.Functions.Orchestrators;

[DurableTask]
public class ShardInstanceStartOrchestrator : TaskOrchestrator<ShardInstanceStartOrchestratorInput, ShardInstance>
{
    public override async Task<ShardInstance> RunAsync(TaskOrchestrationContext ctx, ShardInstanceStartOrchestratorInput input)
    {
        var orchestrationId = ShardInstanceStartOrchestratorId.Parse(ctx.InstanceId);
        var shardInstanceId = orchestrationId.ShardInstanceId;

        // Reserve worker capacity and create new worker immediately if insufficient space
        WorkerId? workerId = null;

        while (workerId is null)
        {
            workerId = await ctx.CallWorkerCapacityReserveActivityAsync(input.Cost);

            if (workerId is null)
            {
                // TODO: Create new worker (with handling for if the orchestrator is already in progress, maybe timeout)
            }
        }

        // Start shard instance
        await ctx.CallShardInstanceCreateActivityAsync(new(shardInstanceId, workerId.Value, input.Cost));
        await ctx.CallWorkerPoolShardInstanceStartActivityAsync(new(workerId.Value, shardInstanceId));

        await ctx.WaitForShardInstanceActiveEventAsync();
        var shardInstance = await ctx.CallShardInstanceStatusUpdateActivityAsync(new(shardInstanceId, ShardInstanceStatus.Active));

        // Optimistically increase worker capacity if now minimal
        var capacity = await ctx.CallWorkerCapacityReviewActivityAsync(true);

        if (capacity == WorkerCapacityReviewResult.MinimalCapacity)
        {
            // TODO: Create new worker (with handling for if the orchestrator is already in progress, maybe timeout)
        }

        return shardInstance;
    }
}

public record ShardInstanceStartOrchestratorInput(float Cost);

public readonly record struct ShardInstanceStartOrchestratorId(ShardInstanceId ShardInstanceId)
{
    public const string Prefix = nameof(ShardInstanceStartOrchestratorId) + "-";

    public static ShardInstanceStartOrchestratorId Parse(string value)
    {
        if (!value.StartsWith(Prefix))
            throw new ArgumentException($"Invalid format for {nameof(ShardInstanceStartOrchestratorId)}: {value}");

        var unprefixed = value[Prefix.Length..];
        var shardInstanceId = ShardInstanceId.Parse(unprefixed);
        return new ShardInstanceStartOrchestratorId(shardInstanceId);
    }

    public override string ToString() => Prefix + ShardInstanceId.ToString();
}

[DurableEvent]
public sealed record ShardInstanceActiveEvent();
