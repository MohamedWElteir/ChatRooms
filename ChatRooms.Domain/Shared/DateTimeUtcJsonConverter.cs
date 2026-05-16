using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatRooms.Domain.Shared;

public sealed class DateTimeUtcJsonConverter : JsonConverter<DateTimeUtc>
{
    public override DateTimeUtc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            if (doc.RootElement.TryGetProperty("Value", out var v))
            {
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
            }
            throw new JsonException("Invalid DateTimeUtc object.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) throw new JsonException("Invalid DateTimeUtc value.");
            var dto = DateTimeOffset.Parse(s);
            return DateTimeUtc.FromUtc(dto.UtcDateTime);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var ms = reader.GetInt64();
            return DateTimeUtc.FromUnixMilliseconds(ms);
        }

        throw new JsonException("Invalid DateTimeUtc token.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeUtc value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.UtcDateTime.ToString("o"));
    }
}
