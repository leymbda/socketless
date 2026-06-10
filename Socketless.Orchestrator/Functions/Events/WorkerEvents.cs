using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;

namespace Socketless.Orchestrator.Functions.Events;

public static class WorkerEvents
{
    [Function(nameof(WorkerProvisionedEventGridEvent))]
    public static async Task WorkerProvisionedEventGridEvent([EventGridTrigger] EventGridEvent ev)
    {
    }

    [Function(nameof(WorkerDeprovisionedEventGridEvent))]
    public static async Task WorkerDeprovisionedEventGridEvent([EventGridTrigger] EventGridEvent ev)
    {
    }

    [Function(nameof(WorkerActiveEventGridEvent))]
    public static async Task WorkerActiveEventGridEvent([EventGridTrigger] EventGridEvent ev)
    {
    }

    [Function(nameof(WorkerStoppedEventGridEvent))]
    public static async Task WorkerStoppedEventGridEvent([EventGridTrigger] EventGridEvent ev)
    {
    }

    [Function(nameof(WorkerDiedEventGridEvent))]
    public static async Task WorkerDiedEventGridEvent([EventGridTrigger] EventGridEvent ev)
    {
    }
}
