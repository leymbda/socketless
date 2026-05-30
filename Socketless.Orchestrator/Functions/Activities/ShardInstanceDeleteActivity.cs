using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class ShardInstanceDeleteActivity(IWorkerPoolRepository repository) : TaskActivity<ShardInstanceId, object?>
{
    public override async Task<object?> RunAsync(TaskActivityContext context, ShardInstanceId shardInstanceId)
    {
        await repository.DeleteShardInstance(shardInstanceId);
        return null;
    }
}
