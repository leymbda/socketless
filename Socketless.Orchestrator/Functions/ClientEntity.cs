using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions;

public class ClientEntity : TaskEntity<ClientEntityState>
{
    public static EntityInstanceId Id(ClientId clientId) =>
        new(nameof(ClientEntity), clientId.ToString());

    public ClientEntityState GetState() => State;

    public void AddApp(AppId appId) =>
        State.Apps.Add(appId);

    public void RemoveApp(AppId appId) =>
        State.Apps.Remove(appId);

    public void OnDedicatedIpAssigned(ClientEntityDedicatedIpId dedicatedIp) =>
        State.IpAddresses.Add(dedicatedIp);

    public void OnDedicatedIpRemoved(ClientEntityDedicatedIpId dedicatedIp) =>
        State.IpAddresses.Remove(dedicatedIp);

    [Function(nameof(ClientEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<ClientEntity>();
}

public class ClientEntityState
{
    public HashSet<AppId> Apps { get; set; } = [];

    public HashSet<ClientEntityDedicatedIpId> IpAddresses { get; set; } = [];
}

public record ClientEntityDedicatedIpId(DedicatedIpId IpId, NodeId NodeId);
