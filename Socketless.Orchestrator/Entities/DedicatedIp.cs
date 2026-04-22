namespace Socketless.Orchestrator.Entities;

public class DedicatedIp(DedicatedIpId id)
{
    public DedicatedIpId Id { get; } = id;
}

public readonly record struct DedicatedIpId(Guid Value)
{
    public static DedicatedIpId New() => new(Guid.NewGuid());

    public static DedicatedIpId Parse(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}
