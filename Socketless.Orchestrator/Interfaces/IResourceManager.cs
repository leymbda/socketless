using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Interfaces;

public interface IResourceManager
{
    Task<NodeId> CreateNodeAsync(NodeId nodeId);

    Task DeleteNodeAsync(NodeId nodeId);
}
