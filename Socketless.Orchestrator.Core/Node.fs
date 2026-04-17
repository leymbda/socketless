namespace Socketless.Orchestrator.Core

open System

/// A node running many shards.
type Node = {
    Id: NodeId
    Shards: Map<ShardId, Shard>
    CreatedAt: DateTime
}

module Node =
    let [<Literal>] MaximumShards = 300
    let [<Literal>] InitialBalance = 1000.0
    
    /// Create a node.
    let create id createdAt =
        { Id = id; Shards = Map.empty; CreatedAt = createdAt }

    /// Get the remaining number of shards that can be added to the node.
    let space node =
        MaximumShards - Map.count node.Shards

    /// Get the remaining balance based on the current cost of shards in the node.
    let balance node =
        Map.fold (fun acc _ shard -> acc - shard.Cost) InitialBalance node.Shards

    /// Add (or replace) a shard to the node, returning an error if the node cannot fit or afford the shard.
    let addShard (shard: Shard) (node: Node) =
        if space node = 0 then Error "Shard limit reached"
        else if balance node - shard.Cost < 0.0 then Error "Insufficient balance"
        else Ok { node with Shards = Map.add shard.Id shard node.Shards }

    /// Remove a shard from the node by its ID, doing nothing if it doesn't exist.
    let removeShard shardId node =
        { node with Shards = Map.remove shardId node.Shards }
