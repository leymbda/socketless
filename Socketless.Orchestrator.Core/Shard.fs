namespace Socketless.Orchestrator.Core

open System
open System.Net

/// The current state of a shard.
type ShardState =
    | Connecting
    | Identifying
    | Resuming
    | Ready
    | Reconnecting
    | Disconnected

/// A shard connected to the Discord gateway.
type Shard = {
    Id: ShardId
    State: ShardState
    Cost: float
    PublicIp: IPAddress
    Port: int
    CreatedAt: DateTime
}

module Shard =
    let private calculateCost applicationGuildCount shardCount =
        (float applicationGuildCount / float shardCount / 150.0) + 1.0

    /// Create a shard.
    let create applicationId shardIndex shardCount applicationGuildCount state publicIp port createdAt =
        let id = { ApplicationId = applicationId; ShardIndex = shardIndex; ShardCount = shardCount }
        let cost = calculateCost applicationGuildCount shardCount

        { Id = id; State = state; Cost = cost; PublicIp = publicIp; Port = port; CreatedAt = createdAt }

    /// Update the state of a shard.
    let setState state shard =
        { shard with State = state }
        
    /// Update the cost of a shard to a recalculated value based on the new application guild count.
    let updateCost applicationGuildCount shard =
        { shard with Cost = calculateCost applicationGuildCount shard.Id.ShardCount }
