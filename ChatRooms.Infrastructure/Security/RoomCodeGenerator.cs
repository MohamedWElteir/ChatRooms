using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.ValueObjects;
using System.Security.Cryptography;
namespace ChatRooms.Infrastructure.Security;

public sealed class RoomCodeGenerator : IRoomCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 8;

    public RoomCode Generate()
    {
        Span<byte> random = stackalloc byte[CodeLength];
        RandomNumberGenerator.Fill(random);

        var chars = new char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            chars[i] = Alphabet[random[i] % Alphabet.Length];
        }

        return RoomCode.From(new string(chars));
    }
}
