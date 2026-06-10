using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.DurableTask.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Socketless.Orchestrator.Entities;
using Socketless.Orchestrator.Functions.Activities;
using Socketless.Orchestrator.Functions.Orchestrators;
using Socketless.Orchestrator.Interfaces;
using Socketless.Orchestrator.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddHttpClient()
    .AddSingleton(_ => new ArmClient(new DefaultAzureCredential()))
    .AddSingleton<IDiscord, Discord>()
    .AddSingleton<IResourceManager, AzureResourceManager>()
    .AddSingleton<IWorkerPool>(_ => new StubWorkerPool())
    .AddSingleton<IWorkerPoolRepository>(_ => new StubWorkerPoolRepository())
    .AddDurableTaskWorker(b =>
    {
        b.AddTasks(registry =>
        {
            registry.AddActivity<ShardInstanceCreateActivity>();
            registry.AddActivity<ShardInstanceDeleteActivity>();
            registry.AddActivity<ShardInstanceStatusUpdateActivity>();
            registry.AddActivity<WorkerCapacityReleaseActivity>();
            registry.AddActivity<WorkerCapacityReserveActivity>();
            registry.AddActivity<WorkerCapacityReviewActivity>();
            registry.AddActivity<WorkerCreateActivity>();
            registry.AddActivity<WorkerDeleteActivity>();
            registry.AddActivity<WorkerDeprovisionActivity>();
            registry.AddActivity<WorkerPoolShardInstanceStartActivity>();
            registry.AddActivity<WorkerPoolShardInstanceStopActivity>();
            registry.AddActivity<WorkerProvisionActivity>();
            registry.AddActivity<WorkerScaleInFireAndForgetActivity>();
            registry.AddActivity<WorkerScaleOutFireAndForgetActivity>();
            registry.AddActivity<WorkerStatusUpdateActivity>();

            registry.AddOrchestrator<ShardInstanceStartOrchestrator>();
            registry.AddOrchestrator<ShardInstanceStopOrchestrator>();
            registry.AddOrchestrator<WorkerCreateOrchestrator>();
            registry.AddOrchestrator<WorkerDestroyOrchestrator>();
        });
    });

builder.Build().Run();

// TODO: Implement below services to replace stubs

file class StubWorkerPool : IWorkerPool
{
    public Task StartShardInstance(WorkerId workerId, ShardInstanceId shardInstanceId) => throw new NotImplementedException();
    public Task StopShardInstance(WorkerId workerId, ShardInstanceId shardInstanceId) => throw new NotImplementedException();
}

file class StubWorkerPoolRepository : IWorkerPoolRepository
{
    public Task<ShardInstance> CreateShardInstance(ShardInstance shardInstance) => throw new NotImplementedException();
    public Task<Worker> CreateWorker(Worker worker) => throw new NotImplementedException();
    public Task DeleteShardInstance(ShardInstanceId shardInstanceId) => throw new NotImplementedException();
    public Task DeleteWorker(WorkerId workerId) => throw new NotImplementedException();
    public Task<ShardInstance?> GetShardInstance(ShardInstanceId shardInstanceId) => throw new NotImplementedException();
    public Task<Worker?> GetWorker(WorkerId workerId) => throw new NotImplementedException();
    public Task<bool> HasExcessiveAvailableCapacity() => throw new NotImplementedException();
    public Task<bool> HasMinimalAvailableCapacity() => throw new NotImplementedException();
    public Task<ISet<ShardInstance>> ListShardInstancesByShard(ShardId shardId) => throw new NotImplementedException();
    public Task<ISet<ShardInstance>> ListShardInstancesByWorker(WorkerId workerId) => throw new NotImplementedException();
    public Task ReleaseWorkerCapacity(WorkerId workerId, float cost) => throw new NotImplementedException();
    public Task<WorkerId?> ReserveWorkerCapacity(float cost) => throw new NotImplementedException();
    public Task<ShardInstance?> UpdateShardInstanceStatus(ShardInstanceId shardInstanceId, ShardInstanceStatus status) => throw new NotImplementedException();
    public Task<Worker?> UpdateWorkerStatus(WorkerId workerId, WorkerStatus status) => throw new NotImplementedException();
}
