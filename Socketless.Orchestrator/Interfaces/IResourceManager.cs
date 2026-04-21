using Socketless.Orchestrator.Common;
using Socketless.Orchestrator.Entities;

namespace Socketless.Orchestrator.Interfaces;

public interface IResourceManager
{
    Task<Node> CreateNode(NodeId nodeId);

    Task DeleteNode(NodeId nodeId);
}
