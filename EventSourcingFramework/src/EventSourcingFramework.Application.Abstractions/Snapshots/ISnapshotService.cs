namespace EventSourcingFramework.Application.Abstractions.Snapshots;

public interface ISnapshotService
{
    Task<string> TakeSnapshotAsync(long currentEventNumber);
    Task<bool> TakeSnapshotIfNeededAsync(long currentEventNumber);
    Task<bool> RestoreSnapshotAsync(string snapshotId);
    Task<string?> GetLastSnapshotIdAsync();
    Task<string?> GetLatestSnapshotBeforeAsync(long eventNumber);
    Task<string?> GetLatestSnapshotBeforeAsync(DateTime timestamp);
    Task<IReadOnlyCollection<SnapshotMetadata>> GetAllSnapshotsAsync();
    Task<SnapshotMetadata?> GetLastSnapshotAsync();
    Task DeleteSnapshotAsync(string snapshotId);
    Task<SnapshotMetadata?> GetSnapshotMetadataAsync(string snapshotId);
}