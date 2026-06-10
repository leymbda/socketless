using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Socketless.Orchestrator.Functions.Orchestrators;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerScaleInFireAndForgetActivity(DurableTaskClient durableClient) : TaskActivity<bool, object?>
{
    public override async Task<object?> RunAsync(TaskActivityContext context, bool input)
    {
        // TODO: Start orchestrator to choose and scale in worker. Consider race condition caused if this is triggered while one is in progress, but then finishes before this point (probably just re-check capacity before starting the orchestration)
        return null;
    }
}
