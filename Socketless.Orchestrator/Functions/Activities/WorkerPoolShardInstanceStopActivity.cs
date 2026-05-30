using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerPoolShardInstanceStopActivity(IWorkerPool pool) : TaskActivity<WorkerPoolShardInstanceStopActivityInput, object?>
{
    public override async Task<object?> RunAsync(TaskActivityContext context, WorkerPoolShardInstanceStopActivityInput input)
    {
        await pool.StopShardInstance(input.WorkerId, input.ShardInstanceId);
        return null;
    }
}

public record WorkerPoolShardInstanceStopActivityInput(WorkerId WorkerId, ShardInstanceId ShardInstanceId);
