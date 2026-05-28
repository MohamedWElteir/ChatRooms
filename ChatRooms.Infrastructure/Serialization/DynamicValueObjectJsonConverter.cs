using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatRooms.Infrastructure.Serialization;

public sealed class DynamicValueObjectJsonConverter<TValueObject, TPrimitive> : JsonConverter<TValueObject>
{
    private readonly Func<TPrimitive, TValueObject> _createMethod;
    private readonly Func<TValueObject, TPrimitive> _getValueMethod;

    public DynamicValueObjectJsonConverter()
    {
        var fromMethod = typeof(TValueObject).GetMethod("From", BindingFlags.Public | BindingFlags.Static, [typeof(TPrimitive)])
            ?? throw new InvalidOperationException($"Method 'From({typeof(TPrimitive).Name})' not found on {typeof(TValueObject).Name}.");

        _createMethod = (Func<TPrimitive, TValueObject>)Delegate.CreateDelegate(typeof(Func<TPrimitive, TValueObject>), fromMethod);


        var valueProperty = typeof(TValueObject).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property 'Value' not found on {typeof(TValueObject).Name}.");

        var param = Expression.Parameter(typeof(TValueObject), "vo");
        var body = Expression.Convert(Expression.Property(param, valueProperty), typeof(TPrimitive));
        _getValueMethod = Expression.Lambda<Func<TValueObject, TPrimitive>>(body, param).Compile();
    }

    public override TValueObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    if (doc.RootElement.TryGetProperty("Value", out var element))
                    {
                        var nestedValue = element.Deserialize<TPrimitive>(options);
                        return nestedValue is not null
                            ? _createMethod(nestedValue)
                            : throw new JsonException($"Invalid {typeof(TValueObject).Name} object.");
                    }
                    throw new JsonException($"Missing 'Value' property on {typeof(TValueObject).Name}.");
                }
            case JsonTokenType.None:
            case JsonTokenType.EndObject:
            case JsonTokenType.StartArray:
            case JsonTokenType.EndArray:
            case JsonTokenType.PropertyName:
            case JsonTokenType.Comment:
            case JsonTokenType.String:
            case JsonTokenType.Number:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.Null:
            default:
                {
                    var flatValue = JsonSerializer.Deserialize<TPrimitive>(ref reader, options);
                    return flatValue is not null
                        ? _createMethod(flatValue)
                        : throw new JsonException($"Invalid {typeof(TValueObject).Name} value.");
                }
        }
    }

    public override void Write(Utf8JsonWriter writer, TValueObject value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, _getValueMethod(value), options);
    }
}