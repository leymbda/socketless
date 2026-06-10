using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Socketless.Orchestrator.Functions.Orchestrators;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class WorkerScaleOutFireAndForgetActivity(DurableTaskClient durableClient) : TaskActivity<bool, object?>
{
    public override async Task<object?> RunAsync(TaskActivityContext context, bool input)
    {
        await durableClient.ScheduleNewWorkerCreateOrchestratorInstanceAsync(options: new(InstanceId: WorkerCreateOrchestrator.InstanceId));
        return null;
    }
}
