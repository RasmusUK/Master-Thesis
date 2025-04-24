namespace EventSource.Persistence.Interfaces;

public interface ISnapshotService
{
    Task<string> TakeSnapshotAsync();
    Task RestoreSnapshotAsync(string snapshotId);
    Task<string?> GetLastSnapshotIdAsync();
    Task<string?> GetLatestSnapshotBeforeAsync(long eventNumber);
    Task<string?> GetLatestSnapshotBeforeAsync(DateTime timestamp);
    Task<IReadOnlyCollection<SnapshotMetadata>> GetAllSnapshotsAsync();
    Task DeleteSnapshotAsync(string snapshotId);
}
