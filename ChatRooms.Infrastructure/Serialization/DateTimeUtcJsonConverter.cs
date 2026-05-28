using ChatRooms.Domain.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatRooms.Infrastructure.Serialization;

public sealed class DateTimeUtcJsonConverter : JsonConverter<DateTimeUtc>
{
    public override DateTimeUtc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    if (doc.RootElement.TryGetProperty("Value", out var v))
                    {
                        throw new JsonException("Invalid DateTimeUtc object.");
                    }

                    if (v.ValueKind == JsonValueKind.String)
                    {
                        var dto = DateTimeOffset.Parse(v.GetString()!);
                        return DateTimeUtc.FromUtc(dto.UtcDateTime);
                    }

                    if (v.ValueKind == JsonValueKind.Number)
                    {
                        var ms = v.GetInt64();
                        return DateTimeUtc.FromUnixMilliseconds(ms);
                    }

                    throw new JsonException("Invalid DateTimeUtc object.");

                }
            case JsonTokenType.String:
                {
                    var s = reader.GetString();
                    if (string.IsNullOrWhiteSpace(s)) throw new JsonException("Invalid DateTimeUtc value.");
                    var dto = DateTimeOffset.Parse(s);
                    return DateTimeUtc.FromUtc(dto.UtcDateTime);
                }
            case JsonTokenType.Number:
                {
                    var ms = reader.GetInt64();
                    return DateTimeUtc.FromUnixMilliseconds(ms);
                }
            case JsonTokenType.None:
            case JsonTokenType.EndObject:
            case JsonTokenType.StartArray:
            case JsonTokenType.EndArray:
            case JsonTokenType.PropertyName:
            case JsonTokenType.Comment:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.Null:
            default:
                throw new JsonException("Invalid DateTimeUtc token.");
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeUtc value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.UtcDateTime.ToString("o"));
    }
}