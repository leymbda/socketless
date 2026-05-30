using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerProvisionActivity(IResourceManager resourceManager) : TaskActivity<WorkerId, WorkerId>
{
    public override async Task<WorkerId> RunAsync(TaskActivityContext context, WorkerId workerId)
    {
        return await resourceManager.ProvisionWorkerAsync(workerId);
    }
}
