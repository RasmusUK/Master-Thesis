namespace EventSourcingFramework.Application.Abstractions.Replay;

public interface IReplayEnvironmentSwitcher
{
    Task UseDebugDatabaseAsync();
    Task UseProductionDatabaseAsync();
}