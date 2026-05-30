using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class ShardInstanceCreateActivity(IWorkerPoolRepository repository) : TaskActivity<ShardInstanceCreateActivityInput, ShardInstance>
{
    public override async Task<ShardInstance> RunAsync(TaskActivityContext context, ShardInstanceCreateActivityInput input)
    {
        var shardInstance = new ShardInstance(input.ShardInstanceId, ShardInstanceStatus.Starting, input.WorkerId, input.Cost);
        return await repository.CreateShardInstance(shardInstance);
    }
}

public record ShardInstanceCreateActivityInput(ShardInstanceId ShardInstanceId, WorkerId WorkerId, float Cost);
