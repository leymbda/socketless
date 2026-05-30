using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerDeprovisionActivity(IResourceManager resourceManager) : TaskActivity<WorkerId, object?>
{
    public override async Task<object?> RunAsync(TaskActivityContext context, WorkerId workerId)
    {
        await resourceManager.DeprovisionWorkerAsync(workerId);
        return null;
    }
}
