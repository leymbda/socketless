using Azure.Data.Tables;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Socketless.Orchestrator.Common;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;
using Socketless.Orchestrator.Models;

namespace Socketless.Orchestrator.Functions;

public class CreateNodeOrchestrator(
    IResourceManager resourceManager)
{
    public const int NodeProvisioningTimeoutMinutes = 10;

    [Function(nameof(CreateNodeOrchestrator))]
    public async Task RunAsync(
        [OrchestrationTrigger] TaskOrchestrationContext ctx,
        InstanceId nodeId,
        FunctionContext functionContext)
    {
        var logger = functionContext.GetLogger(nameof(CreateNodeOrchestrator));
        using var scope = logger.BeginScope(new Dictionary<string, string>
        {
            ["OrchestratorId"] = ctx.InstanceId,
            ["NodeId"] = nodeId.Value.ToString(),
        });

        try
        {
            // Create mapping in table storage to point node ID to this orchestration instance
            await ctx.CallCreateNodeSaveMappingActivityAsync(new CreateNodeSaveMappingActivityInput(nodeId, ctx.InstanceId));

            // Execute ARM client to create the node
            var output = await ctx.CallCreateNodeStandUpActivityAsync(nodeId);

            // Wait for either node to be come ready or timeout to occur
            using var cts = new CancellationTokenSource();
            var timeoutAt = ctx.CurrentUtcDateTime.AddMinutes(NodeProvisioningTimeoutMinutes);
            var timeoutTask = ctx.CreateTimer(timeoutAt, cts.Token);

            var readyEventTask = ctx.WaitForExternalEvent<NodeReadyEvent>(nameof(NodeReadyEvent));

            var winner = await Task.WhenAny(timeoutTask, readyEventTask);

            if (winner == readyEventTask) cts.Cancel();
            else throw new TimeoutException($"Node failed to become ready within the allotted time");

            logger.LogInformation("Node successfully created and ready to accept shards");
        }
        catch (TimeoutException)
        {
            // Delete the provisioned resources since they failed to launch in a reasonable time
            await ctx.CallCreateNodeStandDownActivityAsync(nodeId);

            logger.LogError("Node failed to ready up in time and was destroyed");
        }
        finally
        {
            // Remove mapping from table storage
            await ctx.CallCreateNodeRemoveMappingActivityAsync(nodeId);
        }
    }

    public record CreateNodeSaveMappingActivityInput(InstanceId NodeId, string OrchestratorId);

    [Function(nameof(CreateNodeSaveMappingActivity))]
    [TableOutput(nameof(KeyValueMapping))]
    public async Task<KeyValueMapping> CreateNodeSaveMappingActivity(
        [ActivityTrigger] CreateNodeSaveMappingActivityInput input)
    {
        return new KeyValueMapping
        {
            PartitionKey = nameof(CreateNodeOrchestrator),
            RowKey = input.NodeId.Value.ToString(),
            RowValue = input.OrchestratorId
        };
    }

    [Function(nameof(CreateNodeRemoveMappingActivity))]
    public async Task CreateNodeRemoveMappingActivity(
        [ActivityTrigger] InstanceId nodeId,
        [TableInput(nameof(KeyValueMapping))] TableClient tableClient)
    {
        await tableClient.DeleteEntityAsync(nameof(CreateNodeOrchestrator), nodeId.Value.ToString());
    }

    [Function(nameof(CreateNodeStandUpActivity))]
    public async Task<Node> CreateNodeStandUpActivity(
        [ActivityTrigger] InstanceId nodeId)
    {
        return await resourceManager.CreateNode(nodeId);

        // TODO: Consider retry logic in case of transient error
    }

    [Function(nameof(CreateNodeStandDownActivity))]
    public async Task CreateNodeStandDownActivity(
        [ActivityTrigger] InstanceId nodeId)
    {
        await resourceManager.DeleteNode(nodeId);
    }

    [Function(nameof(CreateNodeServiceBusEvents))]
    public async Task CreateNodeServiceBusEvents(
        [ServiceBusTrigger("node-events", nameof(CreateNodeServiceBusEvents))] ServiceBusReceivedMessage message,
        [TableInput(nameof(KeyValueMapping), nameof(CreateNodeOrchestrator), "{InstanceId}")] KeyValueMapping? mapping,
        [DurableClient] DurableTaskClient durableClient)
    {
        if (mapping is null)
            return;

        switch (message.Subject)
        {
            case nameof(NodeReadyEvent):
                var payload = message.Body.ToObjectFromJson<NodeReadyEvent>();
                await durableClient.RaiseEventAsync(mapping.RowValue, nameof(NodeReadyEvent), payload);
                break;
        }
    }
}
