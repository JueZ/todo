using Azure;

public class TodoTask : Azure.Data.Tables.ITableEntity
{
    public string PartitionKey { get; set; } // This can represent the category of the task.
    public string RowKey { get; set; } // This can be a unique identifier for the task.
    public string Title { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public int EstimatedTime { get; set; }
    public string DueDate { get; set; }
    public bool IsComplete { get; set; }
    public string CreatedAt { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
