using System.Security.Cryptography;
namespace ChatRooms.Domain.Rooms;

public static class RoomCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = (int)RoomCodeLength.CodeLength;

    public static string Generate()
    {
        Span<byte> random = stackalloc byte[CodeLength];
        RandomNumberGenerator.Fill(random);

        var chars = new char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            chars[i] = Alphabet[random[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}
