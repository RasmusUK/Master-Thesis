using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Exceptions;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class PersonalDataInterceptor : IPersonalDataInterceptor
{
    private readonly IPersonalDataStore personalDataStore;

    public PersonalDataInterceptor(IPersonalDataStore personalDataStore)
    {
        this.personalDataStore = personalDataStore;
    }

    public async Task<Event> ProcessEventForStorage(Event e)
    {
        var properties = e.GetType().GetProperties();
        var modifiedEvent = e with { };

        foreach (var prop in properties)
        {
            if (prop.GetCustomAttributes(typeof(PersonalDataAttribute), false).Length == 1)
            {
                var value = prop.GetValue(e);

                if (value is not null)
                {
                    await personalDataStore.SavePersonalDataAsync(
                        new PersonalData(e.Id, value, prop.Name)
                    );

                    prop.SetValue(modifiedEvent, null);
                }
            }
        }

        return modifiedEvent;
    }

    public async Task<Event> ProcessEventForRetrieval(Event e)
    {
        var personalData = await personalDataStore.GetPersonalDataForEventAsync(e.Id);
        if (!personalData.Any())
            return e;

        var properties = e.GetType().GetProperties();
        var modifiedEvent = e with { };

        foreach (var prop in properties)
        {
            if (prop.GetCustomAttributes(typeof(PersonalDataAttribute), false).Length == 1)
            {
                var pd = personalData.FirstOrDefault(p => p.Name == prop.Name);

                if (pd is null)
                    throw new NotFoundException(
                        $"Personal data for property '{prop.Name}' not found on event '{e.Id}'"
                    );

                prop.SetValue(modifiedEvent, pd.Data);
            }
        }

        return modifiedEvent;
    }
}
