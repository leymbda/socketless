using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerCapacityReleaseActivity(IWorkerPoolRepository repository) : TaskActivity<WorkerReleaseCapacityActivityInput, object?>
{
    public override async Task<object?> RunAsync(TaskActivityContext context, WorkerReleaseCapacityActivityInput input)
    {
        await repository.ReleaseWorkerCapacity(input.WorkerId, input.Cost);
        return null;
    }
}

public record WorkerReleaseCapacityActivityInput(WorkerId WorkerId, float Cost);
