using System.Text.Json;

namespace ChatRooms.Infrastructure.Serialization;

public static class JsonOptionsFactory
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        options.Converters.Add(new DateTimeUtcJsonConverter());
        options.Converters.Add(new ValueObjectJsonConverterFactory());

        return options;
    }
}
