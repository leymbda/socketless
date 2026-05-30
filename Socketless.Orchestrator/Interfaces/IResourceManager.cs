using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Interfaces;

public interface IResourceManager
{
    Task<WorkerId> ProvisionWorkerAsync(WorkerId workerId);

    Task DeprovisionWorkerAsync(WorkerId workerId);
}
