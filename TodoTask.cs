using Azure;
using Azure.Data.Tables;

public class TodoTask : ITableEntity
{
    public string PartitionKey { get; set; } // Represents the username or userID.
    public string RowKey { get; set; } // Unique identifier for the task.
    public string Title { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public long EstimatedTime { get; set; }
    public string DueDate { get; set; }
    public bool IsComplete { get; set; }
    public string CreatedAt { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
