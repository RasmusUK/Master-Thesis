using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EventSource.Persistence;

public static class JsonSerializerOptionsConfiguration
{
    public static JsonSerializerOptions Options =>
        new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { AddPrivateFieldsModifier },
            },
        };

    private static void AddPrivateFieldsModifier(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (
            var field in jsonTypeInfo.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
        )
        {
            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
            jsonPropertyInfo.Get = field.GetValue;
            jsonPropertyInfo.Set = field.SetValue;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }
}
