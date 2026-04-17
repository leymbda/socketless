module Socketless.Orchestrator.Core.Networking

open System.Net

type [<Struct>] AzureResourceId = AzureResourceId of string

type Ip = {
    ResourceId: AzureResourceId
    Name: string
    PublicAddress: IPAddress
}

type IpConfiguration = {
    Name: string
    Ip: Ip
    PrivateAddress: IPAddress
    Shards: Map<int, ShardId> // this should be defined elsewhere
}

type VirtualMachine = {
    ResourceId: AzureResourceId
    NicResourceId: AzureResourceId
    Ips: IpConfiguration list // first is primary, any subsequent are client specific ones
}

// a 4gb VM can comfortably run up to 300 shards, with a max balance of 1000 for different sizes of shards to prevent a
// VM being overloaded with busy shards. Would definitely want to see the insights on this too though. $1 per shard
// seems profitable, dedicated IPs can be done but won't do for now as the receive-event-only way the existing IPs get
// used is more resilient to IP bans.

// Use Azure ARM SDK to manage VMs and IPs. Service Bus topics/subscriptions for orchestrator -> VMs, single queue for
// VMs -> orchestrator. Durable entities to store the VM states, and a single cluster manager. Read data periodically
// uploaded to a DB from the entities for dashboard. CQRS.

// TODO: This file doesn't belong in core
