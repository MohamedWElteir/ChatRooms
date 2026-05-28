using ChatRooms.Infrastructure.Security;

namespace ChatRooms.Infrastructure.Tests.Security;

public class RoomCodeGeneratorTests
{
    private readonly RoomCodeGenerator _sut = new();

    [Fact]
    public void Generate_ShouldReturnValidRoomCode()
    {
        // Act
        var result = _sut.Generate();
        // Assert
        Assert.NotNull(result.Value);
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
         const int iterations = 100_000;
         var generatedCodes = new HashSet<string>(iterations);
     
         // Act & Assert
         for (int i = 0; i < iterations; i++)
         {
             var code = _sut.Generate().Value;
             Assert.True(generatedCodes.Add(code));
         }
         
         Assert.Equal(iterations, generatedCodes.Count);
     }
}