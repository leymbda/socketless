using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerPoolShardInstanceStartActivity(IWorkerPool pool) : TaskActivity<WorkerPoolShardInstanceStartActivityInput, object?>
{
    public override async Task<object?> RunAsync(TaskActivityContext context, WorkerPoolShardInstanceStartActivityInput input)
    {
        await pool.StartShardInstance(input.WorkerId, input.ShardInstanceId);
        return null;
    }
}

public record WorkerPoolShardInstanceStartActivityInput(WorkerId WorkerId, ShardInstanceId ShardInstanceId);
