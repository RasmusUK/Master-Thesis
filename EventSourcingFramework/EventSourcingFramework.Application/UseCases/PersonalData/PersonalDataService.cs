using System.Reflection;
using EventSourcingFramework.Application.Abstractions;
using EventSourcingFramework.Application.Abstractions.EventSourcingSettings;
using EventSourcingFramework.Application.Abstractions.PersonalData;
using EventSourcingFramework.Core;
using EventSourcingFramework.Core.Attributes;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.UseCases.PersonalData;

public class PersonalDataService : IPersonalDataService
{
    private readonly IPersonalDataStore personalDataStore;
    private readonly IEventSourcingSettings eventSourcingSettings;

    public PersonalDataService(
        IPersonalDataStore store, IEventSourcingSettings eventSourcingSettings)
    {
        personalDataStore = store;
        this.eventSourcingSettings = eventSourcingSettings;
    }

    public async Task StripAndStoreAsync(IEvent e)
    {
        if (!eventSourcingSettings.EnablePersonalDataStore)
            return;

        var entity = GetEntityFromEvent(e);
        if (entity is null)
            return;

        var dict = new Dictionary<string, object?>();
        StripPersonalData(entity, dict, "");

        if (dict.Count > 0)
            await personalDataStore.StoreAsync(e.Id, dict);
    }

    public async Task RestoreAsync(IEvent e)
    {
        if (!eventSourcingSettings.EnablePersonalDataStore)
            return;

        var entity = GetEntityFromEvent(e);
        if (entity is null)
            return;

        var data = await personalDataStore.RetrieveAsync(e.Id);
        if (data.Count > 0)
            RestorePersonalData(entity, data, "");
    }

    private object? GetEntityFromEvent(IEvent e)
    {
        var entityProp = e.GetType()
            .GetProperty("Entity", BindingFlags.Public | BindingFlags.Instance);
        return entityProp?.GetValue(e);
    }

    private void StripPersonalData(object obj, Dictionary<string, object?> dict, string path)
    {
        var type = obj.GetType();
        foreach (
            var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0)
        )
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var value = prop.GetValue(obj);
            var propPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";

            if (prop.GetCustomAttribute<PersonalDataAttribute>() != null)
            {
                dict[propPath] = value;
                prop.SetValue(obj, null);
            }
            else if (value != null && !IsPrimitive(prop.PropertyType))
            {
                StripPersonalData(value, dict, propPath);
            }
        }
    }

    private void RestorePersonalData(object obj, Dictionary<string, object?> dict, string path)
    {
        var type = obj.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";

            if (dict.TryGetValue(propPath, out var val))
            {
                prop.SetValue(obj, val);
            }
            else
            {
                var subObj = prop.GetValue(obj);
                if (subObj != null && !IsPrimitive(prop.PropertyType))
                {
                    RestorePersonalData(subObj, dict, propPath);
                }
            }
        }
    }

    private bool IsPrimitive(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(Guid)
            || type == typeof(DateTime);
    }
}
