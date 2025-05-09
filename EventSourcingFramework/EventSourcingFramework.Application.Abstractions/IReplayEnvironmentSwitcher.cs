namespace EventSourcingFramework.Application.Abstractions;

public interface IReplayEnvironmentSwitcher
{
    Task UseDebugDatabaseAsync();
    Task UseProductionDatabaseAsync();
}