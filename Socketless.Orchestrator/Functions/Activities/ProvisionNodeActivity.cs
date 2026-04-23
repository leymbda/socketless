using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class ProvisionNodeActivity(IResourceManager resourceManager) : TaskActivity<NodeId, NodeId>
{
    public override async Task<NodeId> RunAsync(TaskActivityContext context, NodeId nodeId)
    {
        return await resourceManager.CreateNodeAsync(nodeId);
    }
}
