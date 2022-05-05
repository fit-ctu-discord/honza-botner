using System;
using System.Security.Cryptography;
using System.Text;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services;

public class Sha256HashService : IHashService
{
    public string Hash(string input)
    {
        var encLen = (input.Length + 1) * 3;
        var enc = encLen <= 1024 ? stackalloc byte[encLen] : new byte[encLen];
        Span<byte> bytes = stackalloc byte[256 / 8];

        var len = Encoding.UTF8.GetBytes(input, enc);
        SHA256.HashData(enc[..len], bytes);

        return Convert.ToHexString(bytes).ToLower();
    }
}

