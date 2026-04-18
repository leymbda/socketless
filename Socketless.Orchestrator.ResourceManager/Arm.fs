module Socketless.Orchestrator.ResourceManager.Arm

open Azure
open Azure.Core
open Azure.Identity
open Azure.ResourceManager
open Azure.ResourceManager.Resources
open Azure.ResourceManager.Resources.DeploymentStacks
open Azure.ResourceManager.Resources.DeploymentStacks.Models
open FsToolkit.ErrorHandling
open System
open System.Text.Json

let private getResourceGroup resourceGroupName (subscription: SubscriptionResource) = asyncResult {
    let! resourceGroupResponse = subscription.GetResourceGroups().GetAsync(resourceGroupName)
    do! Result.requireTrue "Could not find resource group" resourceGroupResponse.HasValue
    return resourceGroupResponse.Value
}

let private getStackName (instanceId: string) =
    $"socketless-stack-{instanceId}"

let private getDeploymentOutputs (data: BinaryData) =
    use doc = JsonDocument.Parse(data)

    doc.RootElement.EnumerateObject()
    |> Seq.map (fun prop -> prop.Name, prop.Value.GetProperty("value").ToString())
    |> Map.ofSeq

let createVirtualMachine (resourceGroupName: string) (vnetName: string) (subnetName: string) (sshPublicKey: string) (instanceId: string) = asyncResult {
    let client = ArmClient(DefaultAzureCredential())
    let! subscription = client.GetDefaultSubscriptionAsync()
    let! resourceGroup = getResourceGroup resourceGroupName subscription

    // Create stack
    let stackData = new DeploymentStackData(
        Location = AzureLocation.EastUS,
        Template = BinaryData.FromString(""), // TODO: Build vm.bicep into ARM JSON template to put here (assembly resource)
        ActionOnUnmanage = ActionOnUnmanage(UnmanageActionResourceMode.Delete))
        
    stackData.Parameters.Add("vnetName", DeploymentParameterItem(Value = BinaryData(vnetName)))
    stackData.Parameters.Add("subnetName", DeploymentParameterItem(Value = BinaryData(subnetName)))
    stackData.Parameters.Add("sshPublicKey", DeploymentParameterItem(Value = BinaryData(sshPublicKey)))
    stackData.Parameters.Add("instanceId", DeploymentParameterItem(Value = BinaryData(instanceId)))
    // TODO: 'additionalIpResourceIds' to handle dedicated IPs

    let stacks = client.GetDeploymentStacks(resourceGroup.Id)
    let! operation = stacks.CreateOrUpdateAsync(WaitUntil.Completed, getStackName instanceId, stackData)

    // Get outputs of stack
    do! Result.requireTrue "Attempted to read result of incomplete stack operation" operation.HasCompleted
    do! Result.requireTrue "Stack operation failed" operation.HasValue
    do! Result.requireTrue "Stack operation has no data" operation.Value.HasData
    let outputs = getDeploymentOutputs operation.Value.Data.Outputs

    // TODO: Get public/private IP mappings from NIC
    // TODO: Return 'Node'?
    return ()
}

let deleteVirtualMachine (resourceGroupName: string) (instanceId: string) = asyncResult {
    let client = ArmClient(DefaultAzureCredential())
    let! subscription = client.GetDefaultSubscriptionAsync()
    let! resourceGroup = getResourceGroup resourceGroupName subscription

    // Get stack
    let! stackResponse = client.GetDeploymentStacks(resourceGroup.Id).GetAsync(getStackName instanceId)
    do! Result.requireTrue "Could not find stack" stackResponse.HasValue
    let stack = stackResponse.Value

    // Delete stack
    do! stack.DeleteAsync(WaitUntil.Completed) |> Task.ignore
}

// TODO: Create/delete dedicated IPs as separate stacks, then add the param to 'createVirtualMachine' to handle binding
// TODO: This project should probably be renamed to be an infrastructure project

// a 4gb VM can comfortably run 300+ shards, with a max balance of 1000 for different sizes of shards to prevent a VM
// being overloaded with busy shards. Would definitely want to see the insights on this too though. $1 per shard seems
// profitable, dedicated IPs can be done but won't do for now as the receive-event-only way the existing IPs get used
// is more resilient to IP bans.

// Use Azure ARM SDK to manage VMs and IPs. Service Bus topics/subscriptions for orchestrator -> VMs, single queue for
// VMs -> orchestrator. Durable entities to store the VM states, and a single cluster manager. Read data periodically
// uploaded to a DB from the entities for dashboard. CQRS.
