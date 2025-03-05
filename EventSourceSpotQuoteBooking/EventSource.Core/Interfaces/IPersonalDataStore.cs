namespace EventSource.Core.Interfaces;

public interface IPersonalDataStore
{
    Task SavePersonalDataAsync(PersonalData p);
    Task<ICollection<PersonalData>> GetPersonalDataForEventAsync(Guid eventId);
}
