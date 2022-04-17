using System;
using System.Security.Cryptography;
using System.Text;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services;

public class Sha256HashService : IHashService
{
    private const int HashBytesSize = 256 / 8;
    
    private static ReadOnlySpan<byte> HexAlphabet => new[]
    {
        (byte)'0',
        (byte)'1',
        (byte)'2',
        (byte)'3',
        (byte)'4',
        (byte)'5',
        (byte)'6',
        (byte)'7',
        (byte)'8',
        (byte)'9',
        (byte)'a',
        (byte)'b',
        (byte)'c',
        (byte)'d',
        (byte)'e',
        (byte)'f',
    };

    public string Hash(string input)
    {
        var encLen = (input.Length + 1) * 3;
        var enc = encLen <= 1024 ? stackalloc byte[encLen] : new byte[encLen];
        Span<byte> bytes = stackalloc byte[HashBytesSize];
        Span<char> res = stackalloc char[HashBytesSize * 2];

        var len = Encoding.UTF8.GetBytes(input, enc);
        SHA256.HashData(enc[..len], bytes);

        for (int i = 0, j = 0; i < HashBytesSize; ++i, ++j)
        {
            res[j] = (char)HexAlphabet[bytes[i] >> 4];
            res[++j] = (char)HexAlphabet[bytes[i] & 0xF];
        }

        return new string(res);
    }
}
