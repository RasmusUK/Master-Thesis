namespace EventSourcingFramework.Application.Abstractions.Snapshots;

public record SnapshotMetadata
{
    public string SnapshotId { get; set; } = default!;
    public long EventNumber { get; set; }
    public DateTime Timestamp { get; set; }
}