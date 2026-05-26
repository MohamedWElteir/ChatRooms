using System.Text.Json;
using System.Text.Json.Serialization;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.Presentation.Users.Requests;

namespace ChatRooms.Presentation.Tests.Users;

public sealed class CreateUserRequestSerializationTests
{
    private static readonly JsonSerializerOptions OptionsWithStringEnum = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Theory]
    [InlineData("Male", Gender.Male)]
    [InlineData("Female", Gender.Female)]
    public void Deserialize_WithStringGender_ShouldMapToEnum(string genderString, Gender expected)
    {
        var json = $$"""{"Name":"John","Email":"john@test.com","Gender":"{{genderString}}","BirthDate":"2000-01-01"}""";

        var result = JsonSerializer.Deserialize<CreateUserRequest>(json, OptionsWithStringEnum);

        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
        Assert.Equal("john@test.com", result.Email);
        Assert.Equal(expected, result.Gender);
        Assert.Equal(new DateTime(2000, 1, 1), result.BirthDate);
    }

    [Fact]
    public void Deserialize_WithCaseInsensitivePropertyNames_ShouldMatchPascalCase()
    {
        var json = """{"name":"John","email":"john@test.com","gender":"Male","birthDate":"2000-01-01"}""";

        var result = JsonSerializer.Deserialize<CreateUserRequest>(json, OptionsWithStringEnum);

        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
        Assert.Equal(Gender.Male, result.Gender);
    }
}
