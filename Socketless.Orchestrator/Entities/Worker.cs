namespace Socketless.Orchestrator.Entities;

public class Worker(WorkerId id, WorkerStatus status)
{
    public WorkerId Id { get; } = id;

    public WorkerStatus Status { get; set; } = status;
}

public readonly record struct WorkerId(Guid Value)
{
    public static WorkerId New() => new(Guid.NewGuid());

    public static WorkerId Parse(string value, IFormatProvider? format = null)
    {
        var guid = Guid.Parse(value);
        return new(guid);
    }

    public static bool TryParse(string? value, IFormatProvider? format, out WorkerId result)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            result = Parse(value, format);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public override string ToString() => Value.ToString();
}

public enum WorkerStatus
{
    Starting,
    Active,
    Migrating,
}
