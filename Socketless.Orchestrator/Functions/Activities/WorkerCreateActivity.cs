using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerCreateActivity(IWorkerPoolRepository repository) : TaskActivity<WorkerId, Worker>
{
    public override async Task<Worker> RunAsync(TaskActivityContext context, WorkerId workerId)
    {
        var worker = new Worker(workerId, WorkerStatus.Starting);
        return await repository.CreateWorker(worker);
    }
}
