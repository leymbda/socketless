using Socketless.Orchestrator.Common;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Interfaces;

public interface IResourceManager
{
    Task<Node> CreateNode(InstanceId instanceId);

    Task DeleteNode(InstanceId instanceId);
}
