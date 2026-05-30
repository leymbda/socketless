using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Interfaces;

public interface IWorkerPool
{
    Task StartShardInstance(WorkerId workerId, ShardInstanceId shardInstanceId);

    Task StopShardInstance(WorkerId workerId, ShardInstanceId shardInstanceId);
}
