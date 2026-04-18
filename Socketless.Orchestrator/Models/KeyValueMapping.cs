using Azure;
using Azure.Data.Tables;

namespace Socketless.Orchestrator.Models;

public class KeyValueMapping : ITableEntity
{
    public required string PartitionKey { get; set; }

    public required string RowKey { get; set; }

    public required string RowValue { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}
