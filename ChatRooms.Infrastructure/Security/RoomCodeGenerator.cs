using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared.Contracts;
using System.Security.Cryptography;
namespace ChatRooms.Infrastructure.Security;

public sealed class RoomCodeGenerator : IGenerator<RoomCode>
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
