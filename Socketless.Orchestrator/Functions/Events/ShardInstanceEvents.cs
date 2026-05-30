using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;

namespace Socketless.Orchestrator.Functions.Events;

public static class ShardInstanceEvents
{
    [Function(nameof(ShardInstanceActiveEventGridEvent))]
    public static async Task ShardInstanceActiveEventGridEvent([EventGridTrigger] EventGridEvent ev)
    {
    }

    [Function(nameof(ShardInstanceStoppedEventGridEvent))]
    public static async Task ShardInstanceStoppedEventGridEvent([EventGridTrigger] EventGridEvent ev)
    {
    }
}
