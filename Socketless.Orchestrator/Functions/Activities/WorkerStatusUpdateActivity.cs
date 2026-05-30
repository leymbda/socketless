using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerStatusUpdateActivity(IWorkerPoolRepository repository) : TaskActivity<WorkerStatusUpdateActivityInput, Worker>
{
    public override async Task<Worker> RunAsync(TaskActivityContext context, WorkerStatusUpdateActivityInput input)
    {
        var worker = await repository.UpdateWorkerStatus(input.WorkerId, input.Status);

        if (worker is null)
            throw new InvalidOperationException("Attempted to update status of a worker that does not exist");

        return worker;
    }
}

public record WorkerStatusUpdateActivityInput(WorkerId WorkerId, WorkerStatus Status);
