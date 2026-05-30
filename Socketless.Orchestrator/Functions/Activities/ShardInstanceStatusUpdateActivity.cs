using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class ShardInstanceStatusUpdateActivity(IWorkerPoolRepository repository) : TaskActivity<ShardInstanceStatusUpdateActivityInput, ShardInstance>
{
    public override async Task<ShardInstance> RunAsync(TaskActivityContext context, ShardInstanceStatusUpdateActivityInput input)
    {
        var shardInstance = await repository.UpdateShardInstanceStatus(input.ShardInstanceId, input.Status);

        if (shardInstance is null)
            throw new InvalidOperationException("Attempted to update status of a shard instance that does not exist");

        return shardInstance;
    }
}

public record ShardInstanceStatusUpdateActivityInput(ShardInstanceId ShardInstanceId, ShardInstanceStatus Status);
