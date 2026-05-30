namespace Socketless.Orchestrator.Entities;

public class Worker(WorkerId id, WorkerStatus status)
{
    public WorkerId Id { get; } = id;

    public WorkerStatus Status { get; set; } = status;
}

public readonly record struct WorkerId(Guid Value)
{
    public static WorkerId New() => new(Guid.NewGuid());

    public static WorkerId Parse(Guid value) => new(value);

    public static WorkerId Parse(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}

public enum WorkerStatus
{
    Starting,
    Active,
    Migrating,
}
