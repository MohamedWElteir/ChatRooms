using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatRooms.Domain.Shared;

public class DateTimeUtcJsonConverter : JsonConverter<DateTimeUtc>
{
    public override DateTimeUtc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTimeOffset();
        return DateTimeUtc.FromUtc(value.UtcDateTime);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeUtc value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}