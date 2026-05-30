using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerCapacityReserveActivity(IWorkerPoolRepository repository) : TaskActivity<float, WorkerId?>
{
    public override async Task<WorkerId?> RunAsync(TaskActivityContext context, float cost)
    {
        return await repository.ReserveWorkerCapacity(cost);
    }
}
