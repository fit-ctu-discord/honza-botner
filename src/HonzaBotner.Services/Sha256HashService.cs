using System;
using System.Security.Cryptography;
using System.Text;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services;

public class Sha256HashService : IHashService
{
    private const int HashBytesSize = 256 / 8;

    public string Hash(string input)
    {
        var encLen = Encoding.UTF8.GetMaxByteCount(input.Length);
        var enc = encLen <= 1024 ? stackalloc byte[encLen] : new byte[encLen];
        Span<byte> bytes = stackalloc byte[HashBytesSize];

        var len = Encoding.UTF8.GetBytes(input, enc);
        SHA256.HashData(enc[..len], bytes);

        return Convert.ToHexString(bytes);
    }
}
