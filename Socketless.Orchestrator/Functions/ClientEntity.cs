using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Functions;

public class ClientEntity : TaskEntity<ClientEntityState>
{
    public static EntityInstanceId Id(ClientId clientId) =>
        new(nameof(ClientEntity), clientId.ToString());

    public ClientEntityState GetState() => State;

    /// <summary>
    /// Add an app to the client.
    /// </summary>
    public void AddApp(AppId appId) =>
        State.Apps.Add(appId);

    /// <summary>
    /// Remove an app from the client.
    /// </summary>
    public void RemoveApp(AppId appId) =>
        State.Apps.Remove(appId);

    /// <summary>
    /// Update state to reflect an attached dedicated IP being used for this client.
    /// </summary>
    public void OnDedicatedIpAttached(ClientEntityOnDedicatedIpAttachedInput input) =>
        State.IpAddresses.Add(new ClientEntityDedicatedIpId(input.IpId, input.NodeId));

    /// <summary>
    /// Update state to reflect a detached dedicated IP that was previously being used for this client.
    /// </summary>
    public void OnDedicatedIpDetached(ClientEntityOnDedicatedIpDetachedInput input) =>
        State.IpAddresses.Remove(new ClientEntityDedicatedIpId(input.IpId, input.NodeId));

    [Function(nameof(ClientEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<ClientEntity>();
}

// State
public class ClientEntityState
{
    public HashSet<AppId> Apps { get; set; } = [];

    public HashSet<ClientEntityDedicatedIpId> IpAddresses { get; set; } = [];
}

public record ClientEntityDedicatedIpId(DedicatedIpId IpId, NodeId NodeId);

// Inputs
public record ClientEntityOnDedicatedIpAttachedInput(DedicatedIpId IpId, NodeId NodeId);

public record ClientEntityOnDedicatedIpDetachedInput(DedicatedIpId IpId, NodeId NodeId);
