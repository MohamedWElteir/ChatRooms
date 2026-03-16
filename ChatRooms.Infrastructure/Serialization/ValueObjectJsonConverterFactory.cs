using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatRooms.Infrastructure.Serialization;

public sealed class ValueObjectJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsPrimitive || typeToConvert.IsEnum || typeToConvert.Namespace?.StartsWith("System") is true)
            return false;

        var hasValueProp = typeToConvert.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance) != null;
        var hasFromMethod = typeToConvert.GetMethod("From", BindingFlags.Public | BindingFlags.Static) != null;

        return hasValueProp && hasFromMethod;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var primitiveType = typeToConvert.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)!.PropertyType;
        var converterType = typeof(DynamicValueObjectJsonConverter<,>).MakeGenericType(typeToConvert, primitiveType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}