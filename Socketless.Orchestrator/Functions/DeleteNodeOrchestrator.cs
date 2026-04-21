using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Socketless.Orchestrator.Common;
using Socketless.Orchestrator.Interfaces;

namespace Socketless.Orchestrator.Functions;

public class DeleteNodeOrchestrator(
    IResourceManager resourceManager)
{
    [Function(nameof(DeleteNodeOrchestrator))]
    public async Task RunAsync(
        [OrchestrationTrigger] TaskOrchestrationContext ctx,
        InstanceId nodeId,
        FunctionContext functionContext)
    {
        var logger = functionContext.GetLogger(nameof(DeleteNodeOrchestrator));
        using var scope = logger.BeginScope(new Dictionary<string, string>
        {
            ["OrchestratorId"] = ctx.InstanceId,
            ["NodeId"] = nodeId.Value.ToString(),
        });

        // Execute ARM client to delete the node
        await ctx.CallDeleteNodeStandDownActivityAsync(nodeId);

        // TODO: How can we verify a VM got deleted successfully? Do we need event grid after all?

        logger.LogInformation("Node successfully created and ready to accept shards");
    }

    [Function(nameof(DeleteNodeStandDownActivity))]
    public async Task DeleteNodeStandDownActivity(
        [ActivityTrigger] InstanceId nodeId)
    {
        await resourceManager.DeleteNode(nodeId);
    }
}
