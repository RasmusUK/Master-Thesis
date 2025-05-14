namespace EventSourcingFramework.Infrastructure.Shared.Interfaces;

public interface IMongoDbRegistrationService
{
    void Register(params (Type Type, string CollectionName)[] entities);
    List <(Type Type, string CollectionName)> GetRegistered();
}