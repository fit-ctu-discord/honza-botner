using System.Security.Cryptography;
using System.Text;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services;

public class Sha256HashService : IHashService
{
    private static readonly char[] HexAlphabet = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

    public string Hash(string input)
    {
        byte[] toBeHashed = Encoding.UTF8.GetBytes(input);
        byte[] bytes = SHA256.HashData(toBeHashed);

        var c = new char[bytes.Length * 2];
        for (int i = 0, j = 0; i < bytes.Length; ++i, ++j)
        {
            c[j] = HexAlphabet[bytes[i] >> 4];
            c[++j] = HexAlphabet[bytes[i] & 0xF];
        }

        return new string(c);
    }
}
