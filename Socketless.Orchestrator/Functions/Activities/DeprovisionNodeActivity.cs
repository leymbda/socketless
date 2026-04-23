using Microsoft.DurableTask;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions.Activities;

[DurableTask]
public class DeprovisionNodeActivity(IResourceManager resourceManager) : TaskActivity<NodeId, NodeId>
{
    public override async Task<NodeId> RunAsync(TaskActivityContext context, NodeId nodeId)
    {
        await resourceManager.DeleteNodeAsync(nodeId);
        return nodeId;
    }
}
