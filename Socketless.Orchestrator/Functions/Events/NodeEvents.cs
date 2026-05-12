using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Socketless.Orchestrator.Functions.Orchestrators;

namespace Socketless.Orchestrator.Functions.Events;

public static class NodeEvents
{
    [Function(nameof(NodeProvisionedEventGridEvent))]
    public static async Task NodeProvisionedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        await durableClient.RaiseEventAsync(ev.Subject, nameof(NodeProvisionedEvent), new NodeProvisionedEvent());
    }

    [Function(nameof(NodeReadyEventGridEvent))]
    public static async Task NodeReadyEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        await durableClient.RaiseEventAsync(ev.Subject, nameof(NodeReadyEvent), new NodeReadyEvent());
    }

    [Function(nameof(NodeFaultedEventGridEvent))]
    public static async Task NodeFaultedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        await durableClient.RaiseEventAsync(ev.Subject, nameof(NodeShutdownEvent), new NodeShutdownEvent(false));
    }

    [Function(nameof(NodeVacatingEventGridEvent))]
    public static async Task NodeVacatingEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        await durableClient.RaiseEventAsync(ev.Subject, nameof(NodeShutdownEvent), new NodeShutdownEvent(true));
    }

    [Function(nameof(NodeDeprovisionedEventGridEvent))]
    public static async Task NodeDeprovisionedEventGridEvent(
        [EventGridTrigger] EventGridEvent ev,
        [DurableClient] DurableTaskClient durableClient)
    {
        await durableClient.RaiseEventAsync(ev.Subject, nameof(NodeDeprovisionedEvent), new NodeDeprovisionedEvent());
    }

    // TODO: ev.Subject is the azure resource ID for system events
    // TODO: May need separate fauled events for healthresources extension + custom events
    // TODO: Support planned outages (azure scheduled events)
}
