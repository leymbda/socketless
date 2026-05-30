using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Activities;

namespace Socketless.Orchestrator.Functions.Orchestrators;

[DurableTask]
public class ShardInstanceStopOrchestrator : TaskOrchestrator<object?, object?>
{
    public override async Task<object?> RunAsync(TaskOrchestrationContext ctx, object? input)
    {
        var orchestrationId = ShardInstanceStopOrchestratorId.Parse(ctx.InstanceId);
        var shardInstanceId = orchestrationId.ShardInstanceId;

        // Stop shard instance
        var shardInstance = await ctx.CallShardInstanceStatusUpdateActivityAsync(new(shardInstanceId, ShardInstanceStatus.Stopping));
        await ctx.CallWorkerPoolShardInstanceStopActivityAsync(new(shardInstance.WorkerId, shardInstanceId));

        await ctx.WaitForShardInstanceStoppedEventAsync();

        // Delete shard instance and release capacity
        await ctx.CallShardInstanceDeleteActivityAsync(shardInstanceId);
        await ctx.CallWorkerCapacityReleaseActivityAsync(new(shardInstance.WorkerId, shardInstance.Cost));

        // Optimistically decrease worker capacity if now excessive
        var capacity = await ctx.CallWorkerCapacityReviewActivityAsync(true);

        if (capacity == WorkerCapacityReviewResult.ExcessiveCapacity)
        {
            // TODO: Scale in worker pool (and confirm this wont cause issues with race conditions)
        }

        return null;
    }
}

public readonly record struct ShardInstanceStopOrchestratorId(ShardInstanceId ShardInstanceId)
{
    public const string Prefix = nameof(ShardInstanceStopOrchestratorId) + "-";

    public static ShardInstanceStopOrchestratorId Parse(string value)
    {
        if (!value.StartsWith(Prefix))
            throw new ArgumentException($"Invalid format for {nameof(ShardInstanceStopOrchestratorId)}: {value}");

        var unprefixed = value[Prefix.Length..];
        var shardInstanceId = ShardInstanceId.Parse(unprefixed);
        return new ShardInstanceStopOrchestratorId(shardInstanceId);
    }

    public override string ToString() => Prefix + ShardInstanceId.ToString();

    // TODO: Implement IParsable
}

[DurableEvent]
public sealed record ShardInstanceStoppedEvent();
