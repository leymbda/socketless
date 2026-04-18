namespace Socketless.Orchestrator.Core

open System
open System.Net

/// A node running many shards.
type Node = {
    InstanceId: InstanceId
    Shards: Map<ShardId, Shard>
    Ips: (IPAddress * IPAddress) list
    CreatedAt: DateTime
}

module Node =
    let [<Literal>] MaximumShards = 300
    let [<Literal>] InitialBalance = 1000.0
    
    /// Create a node.
    let create instanceId primaryPublicIp primaryPrivateIp createdAt =
        let shards = Map.empty
        let ips = List.singleton (primaryPublicIp, primaryPrivateIp)

        { InstanceId = instanceId; Shards = shards; Ips = ips; CreatedAt = createdAt }

    /// Get the remaining number of shards that can be added to the node.
    let space node =
        MaximumShards - Map.count node.Shards

    /// Get the remaining balance based on the current cost of shards in the node.
    let balance node =
        Map.fold (fun acc _ shard -> acc - shard.Cost) InitialBalance node.Shards

    /// Add (or replace) a shard to the node, returning an error if the shard cannot be added to the node.
    let addShard (shard: Shard) (node: Node) =
        if space node = 0 then Error "Shard limit reached"
        else if balance node - shard.Cost < 0.0 then Error "Insufficient balance"
        else if not <| List.exists (fst >> (=) shard.PublicIp) node.Ips then Error "Shard's IP not available in node"
        else if Map.exists (fun _ s -> s.Port = shard.Port) node.Shards then Error "Shard's port is already in use"
        else Ok { node with Shards = Map.add shard.Id shard node.Shards }

    /// Remove a shard from the node by its ID, doing nothing if it doesn't exist.
    let removeShard shardId node =
        { node with Shards = Map.remove shardId node.Shards }

    /// Add an IP address to the node, returning an error if the public or private IP is already in use.
    let addDedicatedIp publicIp privateIp node =
        if List.exists (fst >> (=) publicIp) node.Ips then Error "Public IP address already assigned"
        else if List.exists (snd >> (=) privateIp) node.Ips then Error "Private IP address already assigned"
        else Ok { node with Ips = (publicIp, privateIp) :: node.Ips }

    /// Remove an IP address from the node, returning an error if any shards are using it.
    let removeDedicatedIp publicIp node =
        if List.last node.Ips |> fst = publicIp then Error "Cannot remove primary IP address"
        else if Map.exists (fun _ s -> s.PublicIp = publicIp) node.Shards then Error "IP address is in use by a shard"
        else Ok { node with Ips = List.filter (fst >> (<>) publicIp) node.Ips }
