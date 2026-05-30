using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Interfaces;

public interface IWorkerPoolRepository
{
    /// <summary>
    /// Create a new worker. Throws if worker already exists.
    /// </summary>
    Task<Worker> CreateWorker(Worker worker);

    /// <summary>
    /// Create a new shard instance. Throws if shard instance already exists.
    /// </summary>
    Task<ShardInstance> CreateShardInstance(ShardInstance shardInstance);

    /// <summary>
    /// Get a worker by its ID, returning null if it does not exist.
    /// </summary>
    Task<Worker?> GetWorker(WorkerId workerId);

    /// <summary>
    /// Get a shard instance by its ID, returning null if it does not exist.
    /// </summary>
    Task<ShardInstance?> GetShardInstance(ShardInstanceId shardInstanceId);

    /// <summary>
    /// Update the status of a worker, returning null if the worker does not exist.
    /// </summary>
    Task<Worker?> UpdateWorkerStatus(WorkerId workerId, WorkerStatus status);

    /// <summary>
    /// Update the status of a shard instance, returning null if the shard instance does not exist.
    /// </summary>
    Task<ShardInstance?> UpdateShardInstanceStatus(ShardInstanceId shardInstanceId, ShardInstanceStatus status);

    /// <summary>
    /// Delete a worker. No-op if the worker does not exist.
    /// </summary>
    Task DeleteWorker(WorkerId workerId);

    /// <summary>
    /// Delete a shard instance. No-op if the shard instance does not exist.
    /// </summary>
    Task DeleteShardInstance(ShardInstanceId shardInstanceId);

    /// <summary>
    /// List shard instances on a worker, returning an empty set if the worker does not exist or has no shard instances.
    /// </summary>
    Task<ISet<ShardInstance>> ListShardInstancesByWorker(WorkerId workerId);

    /// <summary>
    /// List shard instances for a shard, returning an empty set if the shard has no shard instances.
    /// </summary>
    Task<ISet<ShardInstance>> ListShardInstancesByShard(ShardId shardId);

    /// <summary>
    /// Reserve capacity on a worker, returning null if no worker has sufficient capacity.
    /// </summary>
    Task<WorkerId?> ReserveWorkerCapacity(float cost);

    /// <summary>
    /// Release capacity from a worker.
    /// </summary>
    Task ReleaseWorkerCapacity(WorkerId workerId, float cost);

    /// <summary>
    /// Returns whether or not the worker pool is approaching max capacity, justifying an optimistic scale out.
    /// </summary>
    /// <returns></returns>
    Task<bool> HasMinimalAvailableCapacity();

    /// <summary>
    /// Returns whether or not the worker pool has excessive available capacity, justifying a migration and scale in.
    /// </summary>
    Task<bool> HasExcessiveAvailableCapacity();
}
