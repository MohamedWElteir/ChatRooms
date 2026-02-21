using ChatRooms.Infrastructure.Security;

namespace ChatRooms.Infrastructure.Tests.Security;

public class RoomCodeGeneratorTests
{
    private readonly RoomCodeGenerator _sut;

    public RoomCodeGeneratorTests()
    {
        _sut = new RoomCodeGenerator();
    }

    [Fact]
    public void Generate_ShouldReturnValidRoomCode()
    {
        // Act
        var result = _sut.Generate();
        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.Value.Length);
    }

    [Fact]
    public void Generate_ShouldOnlyContainAllowedCharacters()
    {
        // Arrange
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        // Act
        var result = _sut.Generate();
        // Assert
        foreach (var c in result.Value)
        {
            Assert.Contains(c, alphabet);
        }
    }

    [Fact]
    public void Generate_ShouldProduceUniqueCodes()
    {
        // Arrange
        var generatedCodes = new HashSet<string>();
        int iterations = 100_000;
        // Act
        for (int i = 0; i < iterations; i++)
        {
            var code = _sut.Generate();
            generatedCodes.Add(code.Value);
        }
        // Assert
        Assert.Equal(iterations, generatedCodes.Count);
    }
}