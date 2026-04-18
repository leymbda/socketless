namespace Socketless.Orchestrator.Core

open System

/// A Discord snowflake.
type [<Struct>] Snowflake = Snowflake of uint64

/// A unique identifier for a shard.
[<Struct>]
type ShardId = {
    ApplicationId: Snowflake
    ShardIndex: uint16
    ShardCount: uint16
}

/// A unique instance identifier for a node.
type [<Struct>] InstanceId = InstanceId of Guid
