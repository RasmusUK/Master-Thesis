namespace EventSourcingFramework.Persistence.Snapshot;

public interface ISnapshotService
{
    Task<string> TakeSnapshotAsync();
    Task TakeSnapshotIfNeededAsync(long currentEventNumber);
    Task RestoreSnapshotAsync(string snapshotId);
    Task<string?> GetLastSnapshotIdAsync();
    Task<string?> GetLatestSnapshotBeforeAsync(long eventNumber);
    Task<string?> GetLatestSnapshotBeforeAsync(DateTime timestamp);
    Task<IReadOnlyCollection<SnapshotMetadata>> GetAllSnapshotsAsync();
    Task<SnapshotMetadata?> GetLastSnapshotAsync();
    Task DeleteSnapshotAsync(string snapshotId);
}
