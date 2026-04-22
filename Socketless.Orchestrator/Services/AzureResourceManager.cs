using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.DeploymentStacks;
using Azure.ResourceManager.Resources.DeploymentStacks.Models;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Interfaces;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Socketless.Orchestrator.Services;

public class AzureResourceManager(ArmClient client) : IResourceManager
{
    private readonly string _resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME")!;
    private readonly string _vnetName = Environment.GetEnvironmentVariable("VNET_NAME")!;
    private readonly string _subnetName = Environment.GetEnvironmentVariable("SUBNET_NAME")!;
    private readonly string _sshPublicKey = Environment.GetEnvironmentVariable("SSH_PUBLIC_KEY")!;

    // TODO: Proper options configuration rather than direct env variable access

    public async Task<NodeId> CreateNodeAsync(NodeId nodeId)
    {
        var subscription = await client.GetDefaultSubscriptionAsync();
        ResourceGroupResource resourceGroup = await subscription.GetResourceGroups().GetAsync(_resourceGroupName);

        // Create stack
        var stackData = new DeploymentStackData()
        {
            Location = AzureLocation.EastUS,
            Template = BinaryData.FromString(""), // TODO: vm.bicep -> json then put here (assembly resource)
            ActionOnUnmanage = new ActionOnUnmanage(UnmanageActionResourceMode.Delete),
        };

        stackData.Parameters.Add("vnetName", new DeploymentParameterItem() { Value = new BinaryData(_vnetName) });
        stackData.Parameters.Add("subnetName", new DeploymentParameterItem() { Value = new BinaryData(_subnetName) });
        stackData.Parameters.Add("sshPublicKey", new DeploymentParameterItem() { Value = new BinaryData(_sshPublicKey) });
        stackData.Parameters.Add("nodeId", new DeploymentParameterItem() { Value = new BinaryData(nodeId.ToString()) });
        // TODO: 'additionalIpResourceIds' to handle dedicated IPs

        var operation = await client
            .GetDeploymentStacks(resourceGroup.Id)
            .CreateOrUpdateAsync(WaitUntil.Completed, GetStackName(nodeId), stackData);

        // Get outputs of stack
        var outputs = GetDeploymentOutputs(operation.Value.Data.Outputs);

        // TODO: Get public/private IP mappings from NIC

        throw new NotImplementedException(); // TODO: Return node
    }

    public async Task DeleteNodeAsync(NodeId nodeId)
    {
        var subscription = await client.GetDefaultSubscriptionAsync();
        ResourceGroupResource resourceGroup = await subscription.GetResourceGroups().GetAsync(_resourceGroupName);

        DeploymentStackResource stack = await client
            .GetDeploymentStacks(resourceGroup.Id)
            .GetAsync(GetStackName(nodeId));
        
        await stack.DeleteAsync(WaitUntil.Completed);
    }

    private string GetStackName(NodeId nodeId) =>
        $"socketless-stack-{nodeId.Value}";

    private IReadOnlyDictionary<string, string> GetDeploymentOutputs(BinaryData data)
    {
        using var doc = JsonDocument.Parse(data);
        var dictionary = new Dictionary<string, string>();

        foreach (var property in doc.RootElement.EnumerateObject())
            dictionary.Add(property.Name, property.Value.GetProperty("value").GetString()!);

        return new ReadOnlyDictionary<string, string>(dictionary);
    }
}
