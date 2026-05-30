using Microsoft.DurableTask;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerCapacityReviewActivity(IWorkerPoolRepository repository) : TaskActivity<bool, WorkerCapacityReviewResult>
{
    public override async Task<WorkerCapacityReviewResult> RunAsync(TaskActivityContext context, bool input)
    {
        var underscaled = await repository.HasMinimalAvailableCapacity();
        var overscaled = await repository.HasMinimalAvailableCapacity();

        if (underscaled) return WorkerCapacityReviewResult.MinimalCapacity;
        else if (overscaled) return WorkerCapacityReviewResult.ExcessiveCapacity;
        else return WorkerCapacityReviewResult.SufficientCapacity;
    }
}

public enum WorkerCapacityReviewResult
{
    MinimalCapacity,
    SufficientCapacity,
    ExcessiveCapacity,
}

// bool input is a dummy value because the durable task generator is generating invalid code when passing `object?`
// input to an activity function. This should be removed if/when a fix is made to the generator.
