namespace Socketless.Orchestrator.Entities;

public readonly record struct NodeId(Guid Value)
{
    public static NodeId New() => new(Guid.NewGuid());

    public static NodeId Parse(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}
